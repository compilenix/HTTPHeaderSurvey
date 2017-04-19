using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using JetBrains.Annotations;

namespace Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
    public interface IResponseHeaderRepository : IRepository<ResponseHeader>
    {
        [NotNull]
        Task<ResponseHeader> AddIfNotExistingAsync([NotNull] ResponseHeader header);

        [NotNull]
        Task<bool> ContainsResponseHeaderAsync([NotNull] string header, [NotNull] string headerValue);

        [NotNull]
        Task<ResponseHeader> GetByHeaderAndValueAsync([NotNull] string header, [NotNull] string value);

        [NotNull]
        Task<IEnumerable<ResponseHeader>> GetByHeaderAsync([NotNull] string header);
    }
}