using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Integration.DataAccess.Entitys;
using Integration.Domain;

namespace Implementation.Domain
{
    public class DataTransferObjectConverter : IDataTransferObjectConverter
    {
        private static DataTable ConvertCsvToDataTable(string filePath, char seperator)
        {
            var dataTable = new DataTable();

            using (var streamReader = new StreamReader(filePath, Encoding.UTF8))
            {
                dataTable.Columns.AddRange(GetDataColumnFromCsvHeader(seperator, streamReader));

                while (!streamReader.EndOfStream)
                {
                    dataTable.Rows.Add(
                        GetDataRowFromCsvDataLine(streamReader.ReadLine()?.Split(seperator), dataTable.Columns.Count, dataTable.NewRow()));
                }
            }

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
            var jobs = new List<NewRequestJobDataTransferObject>();

            foreach (DataRow dataTableRow in ConvertCsvToDataTable(filePath, seperator).Rows)
            {
                if (dataTableRow.ItemArray.Length == typeof(NewRequestJobDataTransferObject).GetProperties().Length)
                {
                    jobs.Add(
                        new NewRequestJobDataTransferObject
                            {
                                Method = dataTableRow.ItemArray[0]?.ToString(),
                                HttpVersion = dataTableRow.ItemArray[1]?.ToString(),
                                IsRunOnce = bool.Parse(dataTableRow.ItemArray[2]?.ToString()),
                                Uri = dataTableRow.ItemArray[3]?.ToString()
                            });
                }
            }

            return MappingUtils.MapRange<RequestJob>(jobs);
        }
    }
}