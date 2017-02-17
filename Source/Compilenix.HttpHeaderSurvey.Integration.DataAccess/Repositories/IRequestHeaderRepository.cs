using System.Collections.Generic;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;

namespace Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories
{
    public interface IRequestHeaderRepository : IRepository<RequestHeader>
    {
        /// <summary>
        /// If succeded returns the added object else null.
        /// </summary>
        RequestHeader AddIfNotExisting(RequestHeader header);

        /// <summary>
        /// Ignores Case
        /// </summary>
        bool ContainsRequestHeader(string header, string headerValue);

        /// <summary>
        /// Ignores Case
        /// </summary>
        IEnumerable<RequestHeader> GetByHeader(string header);
    }
}