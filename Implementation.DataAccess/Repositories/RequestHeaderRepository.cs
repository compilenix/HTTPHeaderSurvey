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

        public IEnumerable<RequestHeader> GetByHeader(string header)
        {
            header = header.ToLower();
            return Entities?.Where(h => h.Key == header).ToList();
        }

        public bool ContainsRequestHeader(string header, string headerValue)
        {
            header = header.ToLower();
            headerValue = headerValue.ToLower();

            var headerValueHash = HashUtils.Hash(headerValue);
            return Entities.Any(j => j.Key == header && j.ValueHash == headerValueHash);
        }

        public RequestHeader AddIfNotExisting(RequestHeader header)
        {
            return !ContainsRequestHeader(header.Key, header.Value) ? Add(header) : null;
        }
    }
}