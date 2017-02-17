using System.Collections.Generic;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories;

namespace Compilenix.HttpHeaderSurvey.Integration.Domain.Modules
{
    public interface IRequestHeaderModule : IBaseModule<IRequestHeaderRepository, RequestHeader>
    {
        IEnumerable<RequestHeader> GetDefaultRequestHeaders();
        List<ResponseHeader> GetResponseHeadersFromList(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headerList, IUnitOfWork unit);
    }
}