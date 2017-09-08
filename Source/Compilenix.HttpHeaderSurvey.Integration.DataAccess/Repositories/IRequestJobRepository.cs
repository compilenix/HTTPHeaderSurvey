using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;

namespace Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public interface IRequestJobRepository : IRepository<RequestJob>
    {
        Task<RequestJob> AddIfNotExistingAsync(RequestJob job);

        Task<bool> ContainsRequestJobAsync(string method, string uri);

        IEnumerable<RequestJob> GetRequestJobsTodoAndNotScheduled(bool withRequestHeaders = false, int count = int.MaxValue, bool checkout = false);

        Task<RequestJob> GetWithRequestHeadersAsync(int id);
    }
}