using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;

namespace Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories
{
    [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
    public interface IRequestHeaderRepository : IRepository<RequestHeader>
    {
        /// <summary>
        /// If succeded returns the added object else null.
        /// </summary>
        Task<RequestHeader> AddIfNotExistingAsync(RequestHeader header);

        /// <summary>
        /// Ignores Case
        /// </summary>
        Task<bool> ContainsRequestHeaderAsync(string header, string headerValue);

        /// <summary>
        /// Ignores Case
        /// </summary>
        Task<IEnumerable<RequestHeader>> GetByHeaderAsync(string header);
    }
}