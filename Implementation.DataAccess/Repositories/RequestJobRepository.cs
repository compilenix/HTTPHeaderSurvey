using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Implementation.Shared;
using Implementation.Shared.IoC;
using Integration.DataAccess;
using Integration.DataAccess.Entitys;
using Integration.DataAccess.Repositories;

namespace Implementation.DataAccess.Repositories
{
    public class RequestJobRepository : Repository<RequestJob>, IRequestJobRepository
    {
        //TODO Where to store / get from, this value?
        private TimeSpan _scheduleDays;
        private DataAccessContext DataAccessContext => Context as DataAccessContext;

        public RequestJobRepository(DataAccessContext context) : base(context)
        {
            _scheduleDays = TimeSpan.FromDays(30);
        }

        private static IQueryable<RequestJob> RequestJobWithHeadersQueryable(IQueryable<RequestJob> query, bool doInclude)
        {
            return doInclude ? query.Include(h => h.Headers) : query;
        }

        public IEnumerable<RequestJob> GetRequestJobsCurrentlyScheduled(bool withRequestHeaders = false)
        {
            return RequestJobWithHeadersQueryable(Entities?.Where(j => j.IsCurrentlyScheduled), withRequestHeaders)?.ToList();
        }

        public IEnumerable<RequestJob> GetRequestJobsTodoAndNotScheduled(bool withRequestHeaders = false,
                                                                         int count = int.MaxValue,
                                                                         bool checkout = false)
        {
            int itemsGot;

            do
            {
                itemsGot = 0;
                var compareToDate = DateTime.Now.Subtract(_scheduleDays);
                var batchSize = count < 1000 ? count : 1000;
                var someJobs =
                    RequestJobWithHeadersQueryable(
                        Entities?.Where(j => DbFunctions.DiffSeconds(j.LastTimeProcessed, compareToDate) > _scheduleDays.TotalSeconds)
                            .Where(j => !j.IsRunOnce)
                            .Where(j => !j.IsCurrentlyScheduled)
                            .Take(batchSize),
                        withRequestHeaders)?.ToList() ?? new List<RequestJob>();

                var errorJobs = new List<RequestJob>();
                if (checkout)
                {
                    errorJobs = Checkout(someJobs);
                }

                foreach (var requestJob in errorJobs)
                {
                    someJobs.Remove(requestJob);
                }

                if (errorJobs.Count > 0)
                {
                }

                foreach (var job in someJobs)
                {
                    itemsGot++;
                    yield return job;
                }

                GarbageCollectionUtils.CollectNow();

                count -= itemsGot;
            }
            while (count > 0 && itemsGot > 0);
        }

        public IEnumerable<RequestJob> GetRequestJobsRunOnce(bool withRequestHeaders = false)
        {
            return RequestJobWithHeadersQueryable(Entities?.Where(j => j.IsRunOnce), withRequestHeaders)?.ToList();
        }

        public IEnumerable<RequestJob> GetRequestJobsRunOnceNot(bool withRequestHeaders = false)
        {
            return RequestJobWithHeadersQueryable(Entities?.Where(j => !j.IsRunOnce), withRequestHeaders)?.ToList();
        }

        public RequestJob GetWithRequestHeaders(int id)
        {
            return Entities.Include(j => j.Headers)?.SingleOrDefault(j => j.Id == id);
        }

        public IEnumerable<RequestJob> GetAll(bool withRequestHeaders = false)
        {
            return Entities.Include(j => j.Headers)?.ToList() ?? new List<RequestJob>();
        }

        public IEnumerable<RequestJob> FindWithRequestHeaders(Expression<Func<RequestJob, bool>> predicate)
        {
            return predicate != null ? Entities?.Include(j => j.Headers)?.Where(predicate).ToList() : null;
        }

        public RequestJob SingleOrDefaultWithRequestHeaders(Expression<Func<RequestJob, bool>> predicate)
        {
            return predicate != null ? Entities?.Include(j => j.Headers)?.SingleOrDefault(predicate) : null;
        }

        public bool ContainsRequestJob(string method, string uri)
        {
            var hash = HashUtils.Hash(uri);
            method = method.ToUpper();
            return Entities.Any(j => j.Method == method && j.UriHash == hash);
        }

        public override RequestJob Add(RequestJob requestJob)
        {
            requestJob.Method = requestJob.Method.ToUpper();
            requestJob.Uri = requestJob.Uri.ToLower();
            requestJob.UriHash = HashUtils.Hash(requestJob.Uri);
            requestJob.LastTimeProcessed = DateTime.Parse("2016-01-01");
            return base.Add(requestJob);
        }

        public RequestJob AddIfNotExisting(RequestJob job)
        {
            return !ContainsRequestJob(job.Method, job.Uri) ? Add(job) : null;
        }

        public int GetRequestJobsTodoAndNotScheduledCount()
        {
            var compareToDate = DateTime.Now.Subtract(_scheduleDays);
            return
                Entities?.Where(j => DbFunctions.DiffSeconds(j.LastTimeProcessed, compareToDate) > _scheduleDays.TotalSeconds)
                    .Where(j => !j.IsRunOnce).Count(j => !j.IsCurrentlyScheduled) ?? 0;
        }

        private List<RequestJob> Checkout(IEnumerable<RequestJob> someJobs)
        {
            var errorJobs = new List<RequestJob>();

            // TODO ...
            foreach (var job in someJobs)
            {
                try
                {
                    using (var unit = IoC.Resolve<IUnitOfWork>())
                    {
                        var requestJob = unit.Repository<IRequestJobRepository>().Get(job?.Id);
                        requestJob.IsCurrentlyScheduled = true;
                        unit.Complete();
                    }
                }
                catch (Exception exception)
                {
                    this.Log()?.Error($"Error setting job IsCurrentlyScheduled of job {job?.Id} ({job?.Uri})", exception);
                    errorJobs.Add(job);
                }
            }

            return errorJobs;
        }
    }
}