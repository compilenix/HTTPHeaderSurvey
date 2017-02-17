using System.Collections.Generic;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;

namespace Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories
{
    public interface IResponseHeaderRepository : IRepository<ResponseHeader>
    {
        ResponseHeader AddIfNotExisting(ResponseHeader header);
        bool ContainsResponseHeader(string header, string headerValue);
        IEnumerable<ResponseHeader> GetByHeader(string header);
        ResponseHeader GetByHeaderAndValue(string header, string value);
    }
}