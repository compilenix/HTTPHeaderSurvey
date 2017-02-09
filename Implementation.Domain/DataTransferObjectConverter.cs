using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Implementation.Shared;
using Integration.DataAccess.Entitys;
using Integration.Domain;
using Integration.Domain.DataTransferObjects;

namespace Implementation.Domain
{
    public class DataTransferObjectConverter : IDataTransferObjectConverter
    {
        private static DataTable ConvertCsvToDataTable(string filePath, char seperator)
        {
            var dataTable = new DataTable();

            using (var streamReader = new StreamReader(filePath, Encoding.UTF8))
            {
                typeof(DataTransferObjectConverter).Log()?.Debug("Loading csv file");
                dataTable.Columns.AddRange(GetDataColumnFromCsvHeader(seperator, streamReader));

                while (!streamReader.EndOfStream)
                {
                    dataTable.Rows.Add(
                        GetDataRowFromCsvDataLine(streamReader.ReadLine()?.Split(seperator), dataTable.Columns.Count, dataTable.NewRow()));
                }
            }

            typeof(DataTransferObjectConverter).Log()?.Debug("Csv file loaded");
            return dataTable;
        }

        private static DataRow GetDataRowFromCsvDataLine(string[] lineValues, int columnCount, DataRow row)
        {
            if (lineValues?.Length == columnCount)
            {
                row.ItemArray = lineValues;
                return row;
            }

            throw new ArgumentOutOfRangeException(nameof(lineValues), $"Value count does not match {nameof(columnCount)}");
        }

        private static DataColumn[] GetDataColumnFromCsvHeader(char seperator, TextReader streamReader)
        {
            return streamReader.ReadLine().Split(seperator).Select(column => new DataColumn(column)).ToArray();
        }

        public IEnumerable<RequestJob> RequestJobsFromCsv(string filePath, char seperator)
        {
            var jobs = new List<NewRequestJobDto>();

            Parallel.ForEach(
                ConvertCsvToDataTable(filePath, seperator).Rows.AsQueryable() as IEnumerable<DataRow>,
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                (row) => {
                    if (row.ItemArray.Length == typeof(NewRequestJobDto).GetProperties().Length)
                    {
                        jobs.Add(
                            new NewRequestJobDto
                            {
                                Method = row.ItemArray[0]?.ToString(),
                                HttpVersion = row.ItemArray[1]?.ToString(),
                                IsRunOnce = bool.Parse(row.ItemArray[2]?.ToString()),
                                Uri = row.ItemArray[3]?.ToString()
                            });
                    }
                });

            return MappingUtils.MapRange<RequestJob>(jobs);
        }
    }
}