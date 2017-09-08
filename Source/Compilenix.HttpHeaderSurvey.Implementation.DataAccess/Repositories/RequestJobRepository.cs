using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Implementation.Shared;
using Compilenix.HttpHeaderSurvey.Implementation.Shared.IoC;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories;

namespace Compilenix.HttpHeaderSurvey.Implementation.DataAccess.Repositories
{
    public class RequestJobRepository : Repository<RequestJob>, IRequestJobRepository
    {
        //TODO Where to store / get from, this value?
        private TimeSpan _scheduleDays;

        public RequestJobRepository(DataAccessContext context) : base(context)
        {
            _scheduleDays = TimeSpan.FromDays(30);
        }

        private static IQueryable<RequestJob> RequestJobWithHeadersQueryable(IQueryable<RequestJob> query, bool doInclude)
        {
            return doInclude ? query.Include(h => h.Headers) : query;
        }

        private static void ValidateRequestJobEntity(RequestJob requestJob)
        {
            if (requestJob == null)
            {
                throw new ArgumentNullException(nameof(requestJob));
            }

            if (string.IsNullOrWhiteSpace(requestJob.Method))
            {
                throw new ArgumentNullException($"{nameof(requestJob.Method)} cannot be null or empty");
            }

            if (string.IsNullOrWhiteSpace(requestJob.Uri))
            {
                throw new ArgumentNullException($"{nameof(requestJob.Uri)} cannot be null or empty");
            }
        }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public IEnumerable<RequestJob> GetRequestJobsTodoAndNotScheduled(bool withRequestHeaders = false, int count = int.MaxValue, bool checkout = false)
        {
            int itemsGot;

            do
            {
                itemsGot = 0;
                var compareToDate = DateTime.Now.Subtract(_scheduleDays);
                var batchSize = count < 1000 ? count : 1000;

                // TODO include runonce and not processed jet
                var someJobs = RequestJobWithHeadersQueryable(
                                       Entities.Where(j => DbFunctions.DiffSeconds(j.LastTimeProcessed, compareToDate) > _scheduleDays.TotalSeconds)
                                           .Where(j => !j.IsRunOnce)
                                           .Where(j => !j.IsCurrentlyScheduled)
                                           .Take(batchSize), withRequestHeaders)
                                   ?.ToList()
                                   .Where(x => x != null) ?? new List<RequestJob>();

                CheckoutAsync(someJobs).Wait();

                foreach (var job in someJobs)
                {
                    itemsGot++;
                    yield return job;
                }

                count -= itemsGot;
            }
            while (count > 0 && itemsGot > 0);
        }

        public async Task<RequestJob> GetWithRequestHeadersAsync(int id)
        {
            // ReSharper disable once PossibleNullReferenceException
            return await Entities.Include(j => j.Headers)?.SingleOrDefaultAsync(j => j.Id == id);
        }

        public async Task<bool> ContainsRequestJobAsync(string method, string uri)
        {
            var hash = HashUtils.Hash(uri);
            method = method.ToUpper();
            // ReSharper disable once PossibleNullReferenceException
            return await Entities.Where(j => j.Method == method && j.UriHash == hash).CountAsync() > 0;
        }

        public override RequestJob Add(RequestJob requestJob)
        {
            ValidateRequestJobEntity(requestJob);

            // ReSharper disable once PossibleNullReferenceException
            requestJob.Method = requestJob.Method.ToUpper();
            // ReSharper disable once PossibleNullReferenceException
            requestJob.Uri = requestJob.Uri.ToLower();
            requestJob.UriHash = HashUtils.Hash(requestJob.Uri);
            requestJob.LastTimeProcessed = DateTime.Now.Subtract(TimeSpan.FromDays(365));
            return base.Add(requestJob);
        }

        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public async Task<RequestJob> AddIfNotExistingAsync(RequestJob job)
        {
            ValidateRequestJobEntity(job);
            return !await ContainsRequestJobAsync(job.Method, job.Uri) ? Add(job) : null;
        }

        // TODO move into domain layer
        private async Task CheckoutAsync(IEnumerable<RequestJob> someJobs)
        {
            foreach (var job in someJobs)
            {
                try
                {
                    ValidateRequestJobEntity(job);

                    using (var unit = IoC.Resolve<IUnitOfWork>())
                    {
                        // ReSharper disable once AssignNullToNotNullAttribute
                        var requestJob = await unit.Resolve<IRequestJobRepository>().GetAsync(job?.Id);
                        if (requestJob != null)
                        {
                            requestJob.IsCurrentlyScheduled = true;
                            await unit.CompleteAsync();
                        }
                    }
                }
                catch (Exception exception)
                {
                    this.Log().Error($"Error setting job IsCurrentlyScheduled of job {job?.Id} ({job?.Uri})", exception);
                }
            }
        }
    }
}