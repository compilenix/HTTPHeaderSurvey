using System.Collections.Generic;
using System.Linq;
using Implementation.Shared;
using Integration.DataAccess.Entitys;
using Integration.DataAccess.Repositories;

namespace Implementation.DataAccess.Repositories
{
    public class RequestHeaderRepository : Repository<RequestHeader>, IRequestHeaderRepository
    {
        private DataAccessContext DataAccessContext => Context as DataAccessContext;

        public RequestHeaderRepository(DataAccessContext context) : base(context)
        {
        }

        public override RequestHeader Add(RequestHeader header)
        {
            header.Key = header.Key.ToLower();

            switch (header.Key)
            {
                case "date":
                    return null;
            }

            header.Value = header.Value.ToLower();
            header.ValueHash = HashUtils.Hash(header.Value);
            return base.Add(header);
        }

        public override IEnumerable<RequestHeader> AddRange(IEnumerable<RequestHeader> requestHeaders)
        {
            foreach (var header in requestHeaders)
            {
                yield return Add(header);
            }
        }

        /// <summary>
        /// Ignores Case
        /// </summary>
        public IEnumerable<RequestHeader> GetByHeader(string header)
        {
            return Entities?.Where(h => h.Key.ToLower() == header.ToLower()).ToList();
        }

        /// <summary>
        /// Ignores Case.
        /// </summary>
        public bool ContainsRequestHeader(string header, string headerValue)
        {
            var headerValueHash = HashUtils.Hash(headerValue.ToLower());
            return Entities.Any(j => j.Key.ToLower() == header.ToLower() && j.ValueHash == headerValueHash);
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