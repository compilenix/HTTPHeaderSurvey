using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Implementation.Shared;
using Compilenix.HttpHeaderSurvey.Implementation.Shared.IoC;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories;
using Compilenix.HttpHeaderSurvey.Integration.Domain.Modules;

namespace Compilenix.HttpHeaderSurvey.Implementation.Domain.Modules
{
    public class RequestJobModule : BaseModule<IRequestJobRepository, RequestJob>, IRequestJobModule
    {
        public RequestJobModule(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task ImportFromCsv(string filePath, char delimiter = ',')
        {
            await ImportFromCsv(filePath, IoC.Resolve<IRequestHeaderModule>().GetDefaultRequestHeaders(), delimiter);
        }

        public async Task ImportFromCsv(string filePath, IEnumerable<RequestHeader> requestHeaders, char delimiter = ',')
        {
            var jobsFromCsv = await new DataTransferObjectConverter().RequestJobsFromCsv(filePath, delimiter);

            Parallel.ForEach(
                jobsFromCsv,
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 },
                async job => { await ProcessBatchImport(job, requestHeaders); });

            GarbageCollectionUtils.CollectNow();
        }

        private async Task ProcessBatchImport(RequestJob requestJob, IEnumerable<RequestHeader> headers)
        {
            using (var unit = IoC.Resolve<IUnitOfWork>())
            {
                var headerRepository = unit.Repository<IRequestHeaderRepository>();

                var headersToAdd = new List<RequestHeader>();
                foreach (var header in headers)
                {
                    headersToAdd.Add(headerRepository.AddIfNotExisting(header));
                }
                headersToAdd = headersToAdd.Where(x => x != null).ToList();

                requestJob.Headers = headersToAdd;
                unit.Repository<IRequestJobRepository>().AddIfNotExisting(requestJob);

                await Save(unit);
            }
        }
    }
}