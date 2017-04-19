using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using JetBrains.Annotations;

namespace Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories
{
    [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
    public interface IRequestHeaderRepository : IRepository<RequestHeader>
    {
        /// <summary>
        /// If succeded returns the added object else null.
        /// </summary>
        [NotNull]
        Task<RequestHeader> AddIfNotExistingAsync([NotNull] RequestHeader header);

        /// <summary>
        /// Ignores Case
        /// </summary>
        [NotNull]
        Task<bool> ContainsRequestHeaderAsync([NotNull] string header, [NotNull] string headerValue);

        /// <summary>
        /// Ignores Case
        /// </summary>
        [ItemNotNull]
        [NotNull]
        Task<IEnumerable<RequestHeader>> GetByHeaderAsync([NotNull] string header);
    }
}