using Integration.DataAccess.Entitys;
using System.Collections.Generic;

namespace Integration.DataAccess.Repositories
{
    public interface IRequestHeaderRepository : IRepository<RequestHeader>
    {
        IEnumerable<RequestHeader> GetByHeader(string header, bool ignoreCase);
    }
}
