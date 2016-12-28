using Integration.DataAccess.Entitys;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Integration.DataAccess.Repositories
{
    public interface IRequestJobRepository : IRepository<RequestJob>
    {
        IEnumerable<RequestJob> GetAll(bool withRequestHeaders = false);
        IEnumerable<RequestJob> GetRequestJobsCurrentlyScheduled(bool withRequestHeaders = false);
        IEnumerable<RequestJob> GetRequestJobsTodoAndNotScheduled(bool withRequestHeaders = false);
        IEnumerable<RequestJob> GetRequestJobsRunOnce(bool withRequestHeaders = false);
        IEnumerable<RequestJob> GetRequestJobsRunOnceNot(bool withRequestHeaders = false);
        RequestJob GetWithRequestHeaders(int id);
        IEnumerable<RequestJob> FindWithRequestHeaders(Expression<Func<RequestJob, bool>> predicate);
        RequestJob SingleOrDefaultWithRequestHeaders(Expression<Func<RequestJob, bool>> predicate);
    }
}
