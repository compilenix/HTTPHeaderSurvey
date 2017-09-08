using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Compilenix.HttpHeaderSurvey.Implementation.Shared.IoC;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using Compilenix.HttpHeaderSurvey.Integration.Domain;
using Compilenix.HttpHeaderSurvey.Integration.Domain.Modules;

namespace Compilenix.HttpHeaderSurvey.Implementation.Domain
{
    public class RequestJobImporter : IRequestJobImporter
    {
        private readonly IUnitOfWork _unit;

        public RequestJobImporter(IUnitOfWork unit)
        {
            _unit = unit;
        }

        public async Task FromCsvAsync(string filePath, IEnumerable<RequestHeader> requestHeaders, char delimiter = ',')
        {
            var jobsFromCsv = await new DataTransferObjectConverter().RequestJobsFromCsv(filePath, delimiter);

            var input = new BufferBlock<RequestJob>();
            var processingList = new List<ActionBlock<RequestJob>>();

            for (var i = 0; i < 4; i++)
            {
                var item = new ActionBlock<RequestJob>(
                    job =>
                        {
                            if (job != null)
                            {
                                ImportAsync(job, requestHeaders).Wait();
                            }
                        }, new ExecutionDataflowBlockOptions
                        {
                            BoundedCapacity = 3,
                            TaskScheduler = new ConcurrentExclusiveSchedulerPair().ConcurrentScheduler,
                            MaxDegreeOfParallelism = Environment.ProcessorCount
                        });
                input.LinkTo(item);
                input.Completion?.ContinueWith(task => item.Complete());
                processingList.Add(item);
            }

            foreach (var requestJob in jobsFromCsv)
            {
                // ReSharper disable once PossibleNullReferenceException
                while (!await input.SendAsync(requestJob)) { }
            }
            // ReSharper disable once RedundantAssignment
            jobsFromCsv = null;

            input.Complete();
            Task.WaitAll(processingList.Select(x => x?.Completion).ToArray());
        }

        public async Task FromCsvAsync(string filePath, char delimiter = ',')
        {
            var module = _unit.Resolve<IRequestHeaderModule>();
            await FromCsvAsync(filePath, await module.GetDefaultRequestHeadersAsync(), delimiter);
        }

        public async Task ImportAsync(RequestJob requestJob, IEnumerable<RequestHeader> headers)
        {
            using (var unit = IoC.Resolve<IUnitOfWork>())
            {
                var headersToAdd = new List<RequestHeader>();
                foreach (var header in headers)
                {
                    if (header != null)
                    {
                        headersToAdd.Add(await unit.Resolve<IRequestHeaderModule>().AddOrUpdateAsync(header));
                    }
                }
                headersToAdd = headersToAdd.Where(x => x != null).ToList();

                requestJob.Headers = headersToAdd;
                await unit.Resolve<IRequestJobModule>().AddOrUpdateAsync(requestJob);
            }
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            _unit.Dispose();
        }
    }
}