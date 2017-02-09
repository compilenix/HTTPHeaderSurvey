using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Integration.DataAccess.Entitys;

namespace Integration.DataAccess.Repositories
{
    public interface IRequestJobRepository : IRepository<RequestJob>
    {
        RequestJob AddIfNotExisting(RequestJob job);
        bool ContainsRequestJob(string method, string uri);
        IEnumerable<RequestJob> FindWithRequestHeaders(Expression<Func<RequestJob, bool>> predicate);
        IEnumerable<RequestJob> GetAll(bool withRequestHeaders = false);
        IEnumerable<RequestJob> GetRequestJobsCurrentlyScheduled(bool withRequestHeaders = false);
        IEnumerable<RequestJob> GetRequestJobsRunOnce(bool withRequestHeaders = false);
        IEnumerable<RequestJob> GetRequestJobsRunOnceNot(bool withRequestHeaders = false);
        IEnumerable<RequestJob> GetRequestJobsTodoAndNotScheduled(bool withRequestHeaders = false, int count = int.MaxValue, bool checkout = false);
        int GetRequestJobsTodoAndNotScheduledCount();
        RequestJob GetWithRequestHeaders(int id);
        RequestJob SingleOrDefaultWithRequestHeaders(Expression<Func<RequestJob, bool>> predicate);
    }
}