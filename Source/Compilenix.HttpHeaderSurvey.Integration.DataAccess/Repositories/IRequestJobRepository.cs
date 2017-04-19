using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using JetBrains.Annotations;

namespace Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public interface IRequestJobRepository : IRepository<RequestJob>
    {
        [NotNull]
        Task<RequestJob> AddIfNotExistingAsync([NotNull] RequestJob job);

        [NotNull]
        Task<bool> ContainsRequestJobAsync([NotNull] string method, [NotNull] string uri);

        [ItemNotNull]
        [NotNull]
        IEnumerable<RequestJob> GetRequestJobsTodoAndNotScheduled(bool withRequestHeaders = false, int count = int.MaxValue, bool checkout = false);

        [NotNull]
        Task<RequestJob> GetWithRequestHeadersAsync(int id);
    }
}