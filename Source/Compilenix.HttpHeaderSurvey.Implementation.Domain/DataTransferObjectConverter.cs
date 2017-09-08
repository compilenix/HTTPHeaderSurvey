using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Compilenix.HttpHeaderSurvey.Implementation.Shared;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using Compilenix.HttpHeaderSurvey.Integration.Domain;
using Compilenix.HttpHeaderSurvey.Integration.Domain.DataTransferObjects;


namespace Compilenix.HttpHeaderSurvey.Implementation.Domain
{
    public class DataTransferObjectConverter : IDataTransferObjectConverter
    {
        
        
        private static async Task<DataTable> ConvertCsvToDataTable( string filePath, char seperator)
        {
            var dataTable = new DataTable();

            using (var streamReader = new StreamReader(filePath, Encoding.UTF8))
            {
                typeof(DataTransferObjectConverter).Log().Debug("Loading csv file");
                dataTable.Columns.AddRange(GetDataColumnsFromCsvHeader(seperator, streamReader));

                var blockOptions = new ExecutionDataflowBlockOptions { BoundedCapacity = Environment.ProcessorCount * 2, MaxDegreeOfParallelism = Environment.ProcessorCount * 2 };

                var coloumCount = dataTable.Columns.Count;

                var bufferBlock = new BufferBlock<string>(new DataflowBlockOptions { BoundedCapacity = 5000 });
                var processLineBock = new TransformBlock<string, DataRow>(
                    line =>
                        {
                            var row = default(DataRow);

                            try
                            {
                                dataTable.ThreadSafeAction(() => { row = dataTable.NewRow(); });

                                if (line == null)
                                {
                                    return row;
                                }

                                var columnValues = line.Split(seperator);
                                if (columnValues.Length == coloumCount)
                                {
                                    // ReSharper disable once CoVariantArrayConversion
                                    row.ItemArray = columnValues;
                                }
                                return row;
                            }
                            catch
                            {
                                // ignored
                            }

                            return row;
                        }, blockOptions);
                var completeBlock = new ActionBlock<DataRow>(
                    row =>
                        {
                            if (row == null)
                            {
                                typeof(DataTransferObjectConverter).Log().Debug("Got null row");
                                return;
                            }

                            dataTable.ThreadSafeAction(() => dataTable.Rows?.Add(row));
                        }, blockOptions);

                bufferBlock.LinkTo(processLineBock);
                processLineBock.LinkTo(completeBlock);

#pragma warning disable 4014
                bufferBlock.Completion?.ContinueWith(t => processLineBock.Complete());
                processLineBock.Completion?.ContinueWith(t => completeBlock.Complete());
#pragma warning restore 4014

                while (!streamReader.EndOfStream)
                {
                    var readLineAsync = streamReader.ReadLineAsync();
                    if (readLineAsync == null)
                    {
                        continue;
                    }

                    var sendAsync = bufferBlock.SendAsync(await readLineAsync);
                    if (sendAsync != null)
                    {
                        await sendAsync;
                    }
                }
                bufferBlock.Complete();

                // ReSharper disable once PossibleNullReferenceException
                await bufferBlock.Completion;
                // ReSharper disable once PossibleNullReferenceException
                await processLineBock.Completion;
                // ReSharper disable once PossibleNullReferenceException
                await completeBlock.Completion;
            }

            typeof(DataTransferObjectConverter).Log().Debug("Csv file loaded");
            return dataTable;
        }

        private static DataColumn[] GetDataColumnsFromCsvHeader(char seperator,  TextReader streamReader)
        {
            return streamReader.ReadLine()?.Split(seperator).Select(column => new DataColumn(column)).ToArray();
        }

        public async Task<IEnumerable<RequestJob>> RequestJobsFromCsv(string filePath, char seperator)
        {
            var jobs = new List<NewRequestJobDto>();
            var dataTable = await ConvertCsvToDataTable(filePath, seperator);

            if (dataTable.Rows == null)
            {
                return new List<RequestJob>();
            }

            var rows = new DataRow[dataTable.Rows.Count];
            dataTable.Rows.CopyTo(rows, 0);
            dataTable.Dispose();

            Parallel.ForEach(
                rows, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, row =>
                    {
                        if (row != null && row.ItemArray.Length == typeof(NewRequestJobDto).GetProperties().Length)
                        {
                            jobs.ThreadSafeAction(
                                () => jobs.Add(
                                    new NewRequestJobDto
                                        {
                                            Method = row.ItemArray[0]?.ToString(),
                                            HttpVersion = row.ItemArray[1]?.ToString(),
                                            IsRunOnce = bool.Parse(row.ItemArray[2]?.ToString() ?? "true"),
                                            Uri = row.ItemArray[3]?.ToString()
                                        }));
                        }
                    });

            // ReSharper disable once AssignNullToNotNullAttribute
            return MappingUtils.MapRange<RequestJob>(jobs);
        }
    }
}