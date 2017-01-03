using System.Collections.Generic;
using Integration.DataAccess.Entitys;

namespace Integration.DataAccess.Repositories
{
    public interface IRequestHeaderRepository : IRepository<RequestHeader>
    {
        RequestHeader AddIfNotExisting(RequestHeader header);
        IEnumerable<RequestHeader> AddIfNotExisting(IEnumerable<RequestHeader> headers);
        bool ContainsRequestHeader(string header, string headerValue);
        IEnumerable<RequestHeader> GetByHeader(string header);
    }
}