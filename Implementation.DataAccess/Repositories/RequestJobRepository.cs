using Integration.DataAccess.Entitys;
using Integration.DataAccess.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Implementation.DataAccess.Repositories
{
    public class RequestJobRepository : Repository<RequestJob>, IRequestJobRepository
    {
        private readonly TimeSpan _scheduleTimeSpan;
        private HttpHeaderDbContext HttpHeaderDbContext => Context as HttpHeaderDbContext;

        public RequestJobRepository(HttpHeaderDbContext context) : base(context)
        {
        }

        // ReSharper disable once ReturnTypeCanBeEnumerable.Local
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
            return RequestJobWithHeadersQueryable(Entities?
                .Where(j => j.LastCompletedDateTime <= DateTime.Now - _scheduleTimeSpan)
                .Where(j => !j.IsRunOnce)
                .Where(j => !j.IsCurrentlyScheduled), withRequestHeaders)?.ToList();
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
            return Entities.Include(j => j.Headers)?.ToList();
        }

        public IEnumerable<RequestJob> FindWithRequestHeaders(Expression<Func<RequestJob, bool>> predicate)
        {
            return predicate != null ? Entities?.Include(j => j.Headers)?.Where(predicate) : null;
        }

        public RequestJob SingleOrDefaultWithRequestHeaders(Expression<Func<RequestJob, bool>> predicate)
        {
            return predicate != null ? Entities?.Include(j => j.Headers)?.SingleOrDefault(predicate) : null;
        }
    }
}