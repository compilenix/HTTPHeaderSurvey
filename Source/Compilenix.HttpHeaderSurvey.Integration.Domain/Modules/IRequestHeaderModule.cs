using System.Collections.Generic;
using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories;
using JetBrains.Annotations;

namespace Compilenix.HttpHeaderSurvey.Integration.Domain.Modules
{
    public interface IRequestHeaderModule : IBaseModule<IRequestHeaderRepository, RequestHeader>
    {
        [ItemNotNull]
        [NotNull]
        Task<IEnumerable<RequestHeader>> GetDefaultRequestHeadersAsync();
    }
}