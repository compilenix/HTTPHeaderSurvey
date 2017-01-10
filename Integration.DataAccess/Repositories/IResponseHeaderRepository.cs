using System.Collections.Generic;
using Integration.DataAccess.Entitys;

namespace Integration.DataAccess.Repositories
{
    public interface IResponseHeaderRepository : IRepository<ResponseHeader>
    {
        ResponseHeader AddIfNotExisting(ResponseHeader header);
        IEnumerable<ResponseHeader> AddIfNotExisting(IEnumerable<ResponseHeader> headers);
        bool ContainsResponseHeader(string header, string headerValue);
        IEnumerable<ResponseHeader> GetByHeader(string header);
    }
}