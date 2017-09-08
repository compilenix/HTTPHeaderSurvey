using System.Collections.Generic;
using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories;


namespace Compilenix.HttpHeaderSurvey.Integration.Domain.Modules
{
    public interface IResponseHeaderModule : IBaseModule<IResponseHeaderRepository, ResponseHeader>
    {
        
        
        Task<List<ResponseHeader>> GetResponseHeadersFromListAsync( IEnumerable<KeyValuePair<string, IEnumerable<string>>> headerList,  IUnitOfWork unit);
    }
}