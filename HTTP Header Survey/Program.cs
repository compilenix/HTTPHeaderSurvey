using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
            ImportRequestJobsIfThereAreNone();
            ProcessRequestJobs(GetSomeRequestJobs(10));
        }

        private static IEnumerable<RequestJob> GetSomeRequestJobs(int count)
        {
            using (var unit = new UnitOfWork())
            {
                return unit.RequestJobs.GetRequestJobsTodoAndNotScheduled(withRequestHeaders: true, count: count);
            }
        }

        private static void ImportRequestJobsIfThereAreNone()
        {
            int countOfRequestJobs;
            using (var unit = new UnitOfWork())
            {
                countOfRequestJobs = unit.RequestJobs.Count();
            }

            if (countOfRequestJobs < 1)
            {
                ImportRequestJobsFromCsv(@"C:\Users\Compilenix\Downloads\top-1m.csv.new.csv");
            }
        }

        private static void ProcessRequestJobs(IEnumerable<RequestJob> someRequestJobs)
        {
            HttpClientUtils.DefaultTimeout = TimeSpan.FromSeconds(5);
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

                HttpResponseMessage jobResult;
                try
                {
                    jobResult = HttpClientUtils.MakeSimpleWebRequest(httpClientOptions).Result;
                }
                catch (Exception exception)
                {
                    continue;
                }

                using (var unit = new UnitOfWork())
                {
                    var headers = new List<ResponseHeader>();

                    foreach (var header in jobResult.Headers)
                    {
                        foreach (var headerValue in header.Value)
                        {
                            headers.Add(unit.ResponseHeaders.AddIfNotExisting(new ResponseHeader { Key = header.Key, Value = headerValue }));
                        }
                    }

                    unit.ResponseMessages.Add(
                        new ResponseMessage
                            {
                                RequestJob = unit.RequestJobs.Get(job.Id),
                                ResponseHeaders = headers,
                                ProtocolVersion = jobResult.Version.ToString(),
                                StatusCode = (int)jobResult.StatusCode
                            });

                    unit.Complete();
                    Console.WriteLine($"completed job -> {job.Uri}");
                }

                jobResult.Dispose();
            }
        }

        private static void ImportRequestJobsFromCsv(string csvFilePath)
        {
            typeof(Program).Log()?.Info("start parallel batch processing of converted jobs");
            Parallel.ForEach(
                new DataTransferObjectConverter().RequestJobsFromCsv(csvFilePath, ',').Batch(10000),
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                ProcessBatchImport);
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

        private static void ProcessBatchImport(IEnumerable<RequestJob> batch)
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