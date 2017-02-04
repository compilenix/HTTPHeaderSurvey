﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Implementation.Shared;
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

        public IEnumerable<RequestJob> GetRequestJobsTodoAndNotScheduled(bool withRequestHeaders = false, int count = int.MaxValue)
        {
            var compareToDate = DateTime.Now.Subtract(_scheduleDays);
            return
                RequestJobWithHeadersQueryable(
                    Entities?.Where(j => DbFunctions.DiffSeconds(j.LastCompletedDateTime, compareToDate) > _scheduleDays.TotalSeconds)
                        .Where(j => !j.IsRunOnce)
                        .Where(j => !j.IsCurrentlyScheduled)
                        .Take(count),
                    withRequestHeaders)?.ToList();
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

        public override IEnumerable<RequestJob> AddRange(IEnumerable<RequestJob> requestJobs)
        {
            foreach (var requestJob in requestJobs)
            {
                yield return Add(requestJob);
            }
        }

        public RequestJob AddIfNotExisting(RequestJob job)
        {
            return !ContainsRequestJob(job.Method, job.Uri) ? Add(job) : null;
        }

        public void AddIfNotExisting(IEnumerable<RequestJob> jobs)
        {
            foreach (var requestJob in jobs)
            {
                AddIfNotExisting(requestJob);
            }
        }
    }
}