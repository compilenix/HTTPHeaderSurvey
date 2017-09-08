using System.Collections.Generic;
using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories;


namespace Compilenix.HttpHeaderSurvey.Integration.Domain.Modules
{
    public interface IRequestHeaderModule : IBaseModule<IRequestHeaderRepository, RequestHeader>
    {
        
        
        Task<IEnumerable<RequestHeader>> GetDefaultRequestHeadersAsync();
    }
}