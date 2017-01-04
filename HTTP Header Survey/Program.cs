using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Implementation.DataAccess;
using Implementation.Domain;
using Implementation.Shared;
using Integration.DataAccess.Entitys;
using MoreLinq;

namespace HTTPHeaderSurvey
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            AddDefaultRequestHeaders();

            int countOfRequestJobs;
            using (var unit = new UnitOfWork())
            {
                countOfRequestJobs = unit.RequestJobs.Count();
            }

            if (countOfRequestJobs < 10)
            {
                ImportRequestJobsFromCsv(@"C:\Users\Compilenix\Downloads\top-1m.csv.new.csv");
            }

            IEnumerable<RequestJob> someRequestJobs = null;
            using (var unit = new UnitOfWork())
            {
                someRequestJobs = unit.RequestJobs.GetRequestJobsTodoAndNotScheduled(withRequestHeaders: true, count: 1);
            }

            foreach (var job in someRequestJobs)
            {
                using (var requestMessage = HttpClientUtils.NewHttpRequestMessage(job.Method, new Uri(job.Uri), job.Headers, new Version(job.HttpVersion)))
                using (
                    var responseMessage = HttpClientUtils.NewHttpClientRequest(
                        requestMessage,
                        HttpClientUtils.NewHttpClient(HttpClientUtils.NewWebRequestHandler())))
                {
                    var result = responseMessage?.Result;
                    if (result?.RequestMessage != null)
                    {
                        Console.WriteLine(result.RequestMessage.RequestUri);
                        Console.Write(result.Headers);
                        Console.WriteLine($"{(int)result.StatusCode} {result.StatusCode}\n");
                    }
                }
            }
        }

        private static void ImportRequestJobsFromCsv(string csvFilePath)
        {
            typeof(Program).Log()?.Info("start parallel batch processing of converted jobs");
            Parallel.ForEach(
                new DataTransferObjectConverter().RequestJobsFromCsv(csvFilePath, ',').Batch(10000),
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                ProcessBatch);
            typeof(Program).Log()?.Info("parallel batch processing of converted jobs done");
        }

        private static void AddDefaultRequestHeaders()
        {
            using (var unit = new UnitOfWork())
            {
                unit.RequestHeaders.AddIfNotExisting(
                    new RequestHeader
                        {
                            Key = "User-Agent",
                            Value = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36"
                        });
                unit.Complete();
            }
        }

        private static void ProcessBatch(IEnumerable<RequestJob> batch)
        {
            typeof(Program).Log()?.Debug("Start of a batch worker");
            using (var unit = new UnitOfWork())
            {
                var header = unit.RequestHeaders.GetByHeader("User-Agent").First();

                foreach (var requestJob in batch)
                {
                    requestJob.Headers.Add(header);
                }

                unit.RequestJobs.AddIfNotExisting(batch);
                unit.Complete();
            }

            GarbageCollectionUtils.CollectNow();
            typeof(Program).Log()?.Debug("A batch has been processed");
        }
    }
}