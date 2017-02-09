using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Implementation.Shared;
using Implementation.Shared.IoC;
using Integration.DataAccess.Entitys;
using Integration.Domain.Modules;

namespace HTTPHeaderSurvey
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            Bootstrapper.Initialize();

            AddDefaultRequestHeaders();
            ImportRequestJobsIfThereAreNone(@"C:\Temp\top-1m.csv").Wait();
            HttpClientUtils.DefaultTimeout = TimeSpan.FromSeconds(30);
            var maxDegreeOfParallelism = 100;
            ProcessSomeJobs(int.MaxValue, maxDegreeOfParallelism).Wait();
        }

        private static async Task ProcessSomeJobs(int count, int maxDegreeOfParallelism)
        {
            using (var requestJobModule = IoC.Resolve<IRequestJobModule>())
            {
                await requestJobModule.ProcessPendingJobs(count, maxDegreeOfParallelism);
            }
        }

        private static async Task ImportRequestJobsIfThereAreNone(string fromCsvFile)
        {
            using (var requestJobModule = IoC.Resolve<IRequestJobModule>())
            {
                if (requestJobModule.Count < 1)
                {
                    await requestJobModule.ImportFromCsv(fromCsvFile);
                }
            }
        }

        private static void AddDefaultRequestHeaders()
        {
            using (var requestHeaderModule = IoC.Resolve<IRequestHeaderModule>())
            {
                var list = new List<RequestHeader>
                    {
                        new RequestHeader
                            {
                                Key = "user-agent",
                                Value = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.76 Safari/537.36"
                            },
                        new RequestHeader { Key = "accept-encoding", Value = "gzip, deflate, br" },
                        new RequestHeader { Key = "accept-language", Value = "en-US,en;q=0.8,de;q=0.6" },
                        new RequestHeader { Key = "upgrade-insecure-requests", Value = "1" }
                    };
                foreach (var header in list)
                {
                    requestHeaderModule.AddOrUpdate(header);
                }
            }
        }
    }
}