using System;
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
        private readonly TimeSpan _scheduleTimeSpan;
        private DataAccessContext DataAccessContext => Context as DataAccessContext;

        public RequestJobRepository(DataAccessContext context) : base(context)
        {
        }

        private static IQueryable<RequestJob> RequestJobWithHeadersQueryable(IQueryable<RequestJob> query, bool doInclude)
        {
            return doInclude ? query.Include(h => h.Headers) : query;
        }

        public IEnumerable<RequestJob> GetRequestJobsCurrentlyScheduled(bool withRequestHeaders = false)
        {
            return RequestJobWithHeadersQueryable(Entities?.Where(j => j.IsCurrentlyScheduled), withRequestHeaders)?.ToList();
        }

        public IEnumerable<RequestJob> GetRequestJobsTodoAndNotScheduled(bool withRequestHeaders = false)
        {
            return
                RequestJobWithHeadersQueryable(
                    Entities?.Where(j => j.LastCompletedDateTime <= DateTime.Now - _scheduleTimeSpan)
                        .Where(j => !j.IsRunOnce)
                        .Where(j => !j.IsCurrentlyScheduled),
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
            return predicate != null ? Entities?.Include(j => j.Headers)?.Where(predicate) : null;
        }

        public RequestJob SingleOrDefaultWithRequestHeaders(Expression<Func<RequestJob, bool>> predicate)
        {
            return predicate != null ? Entities?.Include(j => j.Headers)?.SingleOrDefault(predicate) : null;
        }

        public bool ContainsRequestJob(string method, string uri)
        {
            var hash = HashUtils.Hash(uri);
            return Entities.Any(j => j.Method == method && j.UriHash == hash);
        }

        public override RequestJob Add(RequestJob entity)
        {
            entity.UriHash = HashUtils.Hash(entity.Uri);
            return base.Add(entity);
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