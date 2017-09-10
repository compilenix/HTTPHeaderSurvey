using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Compilenix.HttpHeaderSurvey.Implementation.Shared;
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

        public async Task FromCsvAsync(
            string filePath,
            IEnumerable<RequestHeader> requestHeaders,
            char seperator = ',')
        {
            void ProcessBlock(RequestJob job)
            {
                try
                {
                    if (job != null) ImportAsync(job, requestHeaders).Wait();
                }
                catch (Exception exception)
                {
                    this.Log()?.Error("Import error", exception);
                }
            }

            var processingBlock = new ActionBlock<RequestJob>(
                (Action<RequestJob>)ProcessBlock,
                new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = Environment.ProcessorCount << 1,
                        BoundedCapacity = Environment.ProcessorCount << 2
                    });

            using (var requestJobIterator = new DataTransferObjectConverter(seperator).RequestJobsFromCsv(filePath).GetEnumerator())
            {
                int itemsGot;
                const int BatchSize = 5000;
                do
                {
                    itemsGot = 0;

                    for (var i = 0; i < BatchSize && requestJobIterator.MoveNext(); i++)
                    {
                        itemsGot++;
                        await processingBlock.SendAsync(requestJobIterator.Current);
                    }
                }
                while (itemsGot > 0);
                processingBlock.Complete();
            }

            await processingBlock.Completion;
        }

        public async Task FromCsvAsync(string filePath, char delimiter = ',')
        {
            await FromCsvAsync(filePath, await _unit.Resolve<IRequestHeaderModule>().GetDefaultRequestHeadersAsync(), delimiter);
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

        /// <inheritdoc />
        public void Dispose()
        {
            _unit.Dispose();
        }
    }
}