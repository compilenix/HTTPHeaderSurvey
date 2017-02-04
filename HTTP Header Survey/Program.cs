using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Implementation.Domain;
using Implementation.Shared;
using Implementation.Shared.IoC;
using Integration.DataAccess;
using Integration.DataAccess.Entitys;
using Integration.DataAccess.Repositories;

namespace HTTPHeaderSurvey
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            Bootstrapper.Initialize();

            AddDefaultRequestHeaders();
            ImportRequestJobsIfThereAreNone(@"C:\Temp\top-1m.csv");
            ProcessRequestJobs(GetSomeRequestJobs(10));
        }

        private static IEnumerable<RequestJob> GetSomeRequestJobs(int count)
        {
            using (var unit = IoC.Resolve<IUnitOfWork>())
            {
                return unit.Repository<IRequestJobRepository>().GetRequestJobsTodoAndNotScheduled(withRequestHeaders: true, count: count);
            }
        }

        private static void ImportRequestJobsIfThereAreNone(string fromCsvFile)
        {
            int countOfRequestJobs;

            using (var unit = IoC.Resolve<IUnitOfWork>())
            {
                countOfRequestJobs = unit.Repository<IRequestJobRepository>().Count();
            }

            if (countOfRequestJobs < 1)
            {
                ImportRequestJobsFromCsv(fromCsvFile);
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

                using (var unit = IoC.Resolve<IUnitOfWork>())
                {
                    var headers = new List<ResponseHeader>();
                    var headersFromResponse = new List<ResponseHeader>();
                    var addedHeaders = new List<ResponseHeader>();

                    foreach (var header in jobResult.Headers)
                    {
                        foreach (var headerValue in header.Value)
                        {
                            var tmpHeader = new ResponseHeader { Key = header.Key, Value = headerValue };
                            headersFromResponse.Add(tmpHeader);
                            addedHeaders.Add(unit.Repository<IResponseHeaderRepository>().AddIfNotExisting(tmpHeader));
                        }
                    }

                    addedHeaders = addedHeaders.Where(h => h != null).ToList();
                    headersFromResponse.RemoveAll(header => addedHeaders.Contains(header));

                    foreach (var responseHeader in headersFromResponse)
                    {
                        if (responseHeader != null)
                        {
                            headers.Add(unit.Repository<IResponseHeaderRepository>().GetByHeaderAndValue(responseHeader.Key, responseHeader.Value));
                        }
                    }

                    headers.AddRange(addedHeaders);
                    headers = addedHeaders.Where(h => h != null).ToList();

                    var message = unit.Repository<IResponseMessageRepository>().Add(
                        new ResponseMessage
                            {
                                RequestJob = unit.Repository<IRequestJobRepository>().Get(job.Id),
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
            using (var unit = IoC.Resolve<IUnitOfWork>())
            {
                unit.Repository<IRequestHeaderRepository>().AddIfNotExisting(
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

            using (var unit = IoC.Resolve<IUnitOfWork>())
            {
                var header = unit.Repository<IRequestHeaderRepository>().GetByHeader("User-Agent").First();

                foreach (var requestJob in batch)
                {
                    requestJob.Headers.Add(header);
                }

                unit.Repository<IRequestJobRepository>().AddIfNotExisting(batch);
                unit.Complete();
            }

            GarbageCollectionUtils.CollectNow();
            typeof(Program).Log()?.Debug("A batch has been processed");
        }
    }
}