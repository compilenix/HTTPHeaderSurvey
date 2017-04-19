using System.Collections.Generic;
using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Implementation.Shared.IoC;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using Compilenix.HttpHeaderSurvey.Integration.Domain;
using Compilenix.HttpHeaderSurvey.Integration.Domain.Modules;
using JetBrains.Annotations;

namespace Compilenix.HttpHeaderSurvey.Agent.Cli
{
    internal static class Program
    {
        // ReSharper disable once UnusedParameter.Local
        private static void Main(string[] args)
        {
            Bootstrapper.Initialize();

            AddDefaultRequestHeadersAsync().Wait();
            ImportRequestJobsIfThereAreNoneAsync(@"C:\Temp\top-1m.csv").Wait();
            ProcessSomeJobsAsync(int.MaxValue).Wait();
        }

        private static async Task ProcessSomeJobsAsync(int count)
        {
            using (var worker = IoC.Resolve<IRequestJobWorker>()) {
                await worker.StartAsync(count);
                await worker.Completion;
            }
        }

        private static async Task ImportRequestJobsIfThereAreNoneAsync([NotNull] string fromCsvFile)
        {
            using (var unit = IoC.Resolve<IUnitOfWork>())
            {
                if (unit.Resolve<IRequestJobModule>().Count < 1)
                {
                    await unit.Resolve<IRequestJobImporter>().FromCsvAsync(fromCsvFile);
                }
            }
        }

        private static async Task AddDefaultRequestHeadersAsync()
        {
            using (var unit = IoC.Resolve<IUnitOfWork>())
            {
                var requestHeaderModule = unit.Resolve<IRequestHeaderModule>();

                var list = new List<RequestHeader> { new RequestHeader { Key = "user-agent", Value = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36" }, new RequestHeader { Key = "accept-encoding", Value = "gzip, deflate, br" }, new RequestHeader { Key = "accept-language", Value = "en-US,en;q=0.8,de;q=0.6" }, new RequestHeader { Key = "upgrade-insecure-requests", Value = "1" } };

                foreach (var header in list)
                {
                    await requestHeaderModule.AddOrUpdateAsync(header);
                }
            }
        }
    }
}