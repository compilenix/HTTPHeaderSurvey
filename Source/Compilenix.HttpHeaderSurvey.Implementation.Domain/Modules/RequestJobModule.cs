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
        private IRequestJobRepository _repository;

        public RequestJobModule(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _repository = unitOfWork.Repository<IRequestJobRepository>();
        }

        public async Task ImportFromCsvAsync(string filePath, char delimiter = ',')
        {
            using (var module = IoC.Resolve<IRequestHeaderModule>())
            {
                await ImportFromCsvAsync(filePath, await module.GetDefaultRequestHeadersAsync(), delimiter);
            }
        }

        public async Task ImportFromCsvAsync(string filePath, IEnumerable<RequestHeader> requestHeaders, char delimiter = ',')
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
                    headersToAdd.Add(await headerRepository.AddIfNotExistingAsync(header));
                }
                headersToAdd = headersToAdd.Where(x => x != null).ToList();

                requestJob.Headers = headersToAdd;
                await _repository.AddIfNotExistingAsync(requestJob);

                await SaveAsync(unit);
            }
        }
    }
}