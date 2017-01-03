using System;
using System.Collections.Generic;
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
        // ReSharper disable once UnusedParameter.Local
        private static void Main(string[] args)
        {
            using (var unit = new UnitOfWork(new HttpHeaderDbContext()))
            {
                unit.RequestHeaders.AddIfNotExisting(
                    new RequestHeader
                        {
                            Key = "User-Agent",
                            Value = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36"
                        });
                unit.Complete();
            }

            Console.WriteLine("start parallel batch processing of converted jobs");
            Parallel.ForEach(
                new DataTransferObjectConverter().RequestJobsFromCsv(@"C:\Users\Compilenix\Downloads\top-1m.csv.new.csv", ',').Batch(10000),
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 },
                ProcessBatch);
        }

        private static void ProcessBatch(IEnumerable<RequestJob> batch)
        {
            using (var context = new HttpHeaderDbContext())
            using (var unit = new UnitOfWork(context))
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.ValidateOnSaveEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;

                var header = unit.RequestHeaders.Get(1);

                foreach (var requestJob in batch)
                {
                    requestJob.Headers.Add(header);
                }

                unit.RequestJobs.AddIfNotExisting(batch);
                unit.Complete();
            }

            GarbageCollectionUtils.CollectNow();
        }
    }
}