using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using Compilenix.HttpHeaderSurvey.Integration.Domain.DataTransferObjects;

namespace Compilenix.HttpHeaderSurvey.Implementation.Domain
{
    public class DataTransferObjectConverter
    {
        private readonly char _objectSeperator;
        private Dictionary<string, int> _headers;

        public DataTransferObjectConverter(char seperator)
        {
            _objectSeperator = seperator;
        }

        public IEnumerable<RequestJob> RequestJobsFromCsv(string filePath)
        {
            using (var streamReader = new StreamReader(filePath, Encoding.UTF8))
            {
                _headers = GetColumnsFromCsvHeaderAsync(streamReader).Result;
                ValidateCsvHeader();

                do
                {
                    yield return MappingUtils.Map<RequestJob>(ProcessLine(streamReader.ReadLine()));
                }
                while (!streamReader.EndOfStream);
            }
        }

        private async Task<Dictionary<string, int>> GetColumnsFromCsvHeaderAsync(TextReader stream)
        {
            var dict = new Dictionary<string, int>();
            var items = (await stream.ReadLineAsync())?.Split(_objectSeperator);

            for (var i = 0; i < items.Length; i++)
            {
                dict.Add(items[i].ToLower(), i);
            }

            return dict;
        }

        private NewRequestJobDto ProcessLine(string line)
        {
            var values = line.Split(_objectSeperator);
            return new NewRequestJobDto
                {
                    Method = values[_headers["method"]],
                    HttpVersion = values[_headers["httpversion"]],
                    IsRunOnce = bool.Parse(values[_headers["isrunonce"]] ?? "true"),
                    Uri = values[_headers["uri"]]
                };
        }

        private void ValidateCsvHeader()
        {
            var errors = new List<Exception>();

            if (typeof(NewRequestJobDto).GetProperties().Length != _headers.Count)
                errors.Add(new ArgumentOutOfRangeException($"The column count ({_headers.Count}) does not match the required amount of {typeof(NewRequestJobDto).GetProperties().Length}"));

            foreach (var propertyInfo in typeof(NewRequestJobDto).GetProperties())
            {
                if (!_headers.ContainsKey(propertyInfo.Name.ToLower()))
                    errors.Add(new AggregateException($"The culumns does not contain the required value of \"{propertyInfo.Name}\""));
            }

            if (errors.Any()) throw new AggregateException(errors);
        }
    }
}