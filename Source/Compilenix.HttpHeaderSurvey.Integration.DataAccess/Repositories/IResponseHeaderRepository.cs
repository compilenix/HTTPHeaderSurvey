using System.Collections.Generic;
using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;

namespace Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories
{
    public interface IResponseHeaderRepository : IRepository<ResponseHeader>
    {
        Task<ResponseHeader> AddIfNotExistingAsync(ResponseHeader header);
        Task<bool> ContainsResponseHeaderAsync(string header, string headerValue);
        Task<ResponseHeader> GetByHeaderAndValueAsync(string header, string value);
        Task<IEnumerable<ResponseHeader>> GetByHeaderAsync(string header);
    }
}