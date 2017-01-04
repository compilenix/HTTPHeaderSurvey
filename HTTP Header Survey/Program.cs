using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

            HttpClientUtils.DefaultTimeout = TimeSpan.FromSeconds(30);
            foreach (var job in someRequestJobs)
            {
                var httpClientOptions = new HttpClientRequestOptions
                    {
                        Headers = job.Headers,
                        HttpVersion = job.HttpVersion,
                        Method = job.Method,
                        Uri = new Uri(job.Uri),
                        CancellationToken = CancellationToken.None
                    };
                var jobResult = HttpClientUtils.MakeSimpleWebRequest(httpClientOptions).Result;

                Console.WriteLine(jobResult.RequestMessage.RequestUri);
                Console.Write(jobResult.Headers);
                Console.WriteLine($"{(int)jobResult.StatusCode} {jobResult.StatusCode}\n");
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