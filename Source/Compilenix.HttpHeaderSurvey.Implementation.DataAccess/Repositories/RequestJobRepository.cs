using System;
using System.Collections.Generic;
using System.Data.Entity;
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
                // TODO include runonce and not processed jet
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
                    errorJobs = CheckoutAsync(someJobs).Result;
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

                count -= itemsGot;
            }
            while (count > 0 && itemsGot > 0);
        }

        public async Task<RequestJob> GetWithRequestHeadersAsync(int id)
        {
            return await Entities.Include(j => j.Headers)?.SingleOrDefaultAsync(j => j.Id == id);
        }

        public async Task<bool> ContainsRequestJobAsync(string method, string uri)
        {
            var hash = HashUtils.Hash(uri);
            method = method.ToUpper();
            return await Entities.AnyAsync(j => j.Method == method && j.UriHash == hash);
        }

        public override RequestJob Add(RequestJob requestJob)
        {
            requestJob.Method = requestJob.Method.ToUpper();
            requestJob.Uri = requestJob.Uri.ToLower();
            requestJob.UriHash = HashUtils.Hash(requestJob.Uri);
            requestJob.LastTimeProcessed = DateTime.Parse("2016-01-01");
            return base.Add(requestJob);
        }

        public async Task<RequestJob> AddIfNotExistingAsync(RequestJob job)
        {
            return !await ContainsRequestJobAsync(job.Method, job.Uri) ? Add(job) : null;
        }

        private async Task<List<RequestJob>> CheckoutAsync(IEnumerable<RequestJob> someJobs)
        {
            var errorJobs = new List<RequestJob>();

            // TODO ...
            foreach (var job in someJobs)
            {
                try
                {
                    using (var unit = IoC.Resolve<IUnitOfWork>())
                    {
                        var requestJob = await unit.Repository<IRequestJobRepository>().GetAsync(job?.Id);
                        requestJob.IsCurrentlyScheduled = true;
                        await unit.CompleteAsync();
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