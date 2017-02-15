using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Implementation.Shared;
using Integration.DataAccess.Entitys;
using Integration.Domain;
using Integration.Domain.DataTransferObjects;

namespace Implementation.Domain
{
    public class DataTransferObjectConverter : IDataTransferObjectConverter
    {
        // TODO TEST new implementation
        private static async Task<DataTable> ConvertCsvToDataTable(string filePath, char seperator)
        {
            var dataTable = new DataTable();

            using (var streamReader = new StreamReader(filePath, Encoding.UTF8))
            {
                typeof(DataTransferObjectConverter).Log()?.Debug("Loading csv file");
                dataTable.Columns.AddRange(GetDataColumnsFromCsvHeader(seperator, streamReader));

                var blockOptions = new ExecutionDataflowBlockOptions
                    {
                        BoundedCapacity = Environment.ProcessorCount * 2,
                        MaxDegreeOfParallelism = Environment.ProcessorCount * 2
                    };

                var coloumCount = dataTable.Columns.Count;

                var bufferBlock = new BufferBlock<string>(new DataflowBlockOptions { BoundedCapacity = 5000 });
                var processLineBock = new TransformBlock<string, DataRow>(
                    line =>
                        {
                            try
                            {
                                var row = default(DataRow);
                                dataTable.ThreadSafeAction(() => { row = dataTable.NewRow(); });
                                var columnValues = line.Split(seperator);
                                if (columnValues.Length == coloumCount)
                                {
                                    row.ItemArray = columnValues;
                                }
                                return row;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                throw;
                            }
                        },
                    blockOptions);
                var completeBlock = new ActionBlock<DataRow>(
                    row =>
                        {
                            if (row == null)
                            {
                                typeof(DataTransferObjectConverter).Log()?.Debug("Got null row");
                                return;
                            }

                            dataTable.ThreadSafeAction(() => dataTable.Rows.Add(row));
                        },
                    blockOptions);

                bufferBlock.LinkTo(processLineBock);
                processLineBock.LinkTo(completeBlock);

                bufferBlock.Completion?.ContinueWith(t => processLineBock.Complete());
                processLineBock.Completion?.ContinueWith(t => completeBlock.Complete());

                while (!streamReader.EndOfStream)
                {
                    await bufferBlock.SendAsync(await streamReader.ReadLineAsync());
                }
                bufferBlock.Complete();

                await bufferBlock.Completion;
                await processLineBock.Completion;
                await completeBlock.Completion;
            }

            typeof(DataTransferObjectConverter).Log()?.Debug("Csv file loaded");
            return dataTable;
        }

        private static DataColumn[] GetDataColumnsFromCsvHeader(char seperator, TextReader streamReader)
        {
            return streamReader.ReadLine().Split(seperator).Select(column => new DataColumn(column)).ToArray();
        }

        public async Task<IEnumerable<RequestJob>> RequestJobsFromCsv(string filePath, char seperator)
        {
            var jobs = new List<NewRequestJobDto>();
            var dataTable = await ConvertCsvToDataTable(filePath, seperator);
            var rows = new DataRow[dataTable.Rows.Count];
            dataTable.Rows.CopyTo(rows, 0);
            dataTable.Dispose();
            GarbageCollectionUtils.CollectNow();

            Parallel.ForEach(
                rows,
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 },
                row =>
                    {
                        if (row.ItemArray.Length == typeof(NewRequestJobDto).GetProperties().Length)
                        {
                            jobs.ThreadSafeAction(
                                () =>
                                    jobs.Add(
                                        new NewRequestJobDto
                                            {
                                                Method = row.ItemArray[0]?.ToString(),
                                                HttpVersion = row.ItemArray[1]?.ToString(),
                                                IsRunOnce = bool.Parse(row.ItemArray[2]?.ToString()),
                                                Uri = row.ItemArray[3]?.ToString()
                                            }));
                        }
                    });

            return MappingUtils.MapRange<RequestJob>(jobs);
        }
    }
}