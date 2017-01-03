using System;
using System.Collections.Generic;
using System.Linq;
using Implementation.Shared;
using Integration.DataAccess.Entitys;
using Integration.DataAccess.Repositories;

namespace Implementation.DataAccess.Repositories
{
    public class RequestHeaderRepository : Repository<RequestHeader>, IRequestHeaderRepository
    {
        private HttpHeaderDbContext HttpHeaderDbContext => Context as HttpHeaderDbContext;

        public RequestHeaderRepository(HttpHeaderDbContext context) : base(context)
        {
        }

        public override RequestHeader Add(RequestHeader entity)
        {
            entity.ValueHash = HashUtils.Hash(entity.Value);
            return base.Add(entity);
        }

        /// <summary>
        /// Ignores Case
        /// </summary>
        public IEnumerable<RequestHeader> GetByHeader(string header)
        {
            return Entities?.Where(h => string.Equals(h.Key, header, StringComparison.CurrentCultureIgnoreCase)).ToList();
        }

        /// <summary>
        /// Ignores Case.
        /// </summary>
        public bool ContainsRequestHeader(string header, string headerValue)
        {
            var headerValueHash = HashUtils.Hash(headerValue);
            return Entities.Any(j => string.Equals(j.Key, header, StringComparison.CurrentCultureIgnoreCase) && j.ValueHash == headerValueHash);
        }

        /// <summary>
        /// If succeded the added object else null.
        /// </summary>
        public RequestHeader AddIfNotExisting(RequestHeader header)
        {
            return !ContainsRequestHeader(header.Key, header.Value) ? Add(header) : null;
        }

        /// <summary>
        /// If succeded the added object else an empty list.
        /// </summary>
        public IEnumerable<RequestHeader> AddIfNotExisting(IEnumerable<RequestHeader> headers)
        {
            var addedHeaders = new List<RequestHeader>();

            foreach (var requestHeader in headers)
            {
                addedHeaders.Add(AddIfNotExisting(requestHeader));
            }

            return addedHeaders;
        }
    }
}