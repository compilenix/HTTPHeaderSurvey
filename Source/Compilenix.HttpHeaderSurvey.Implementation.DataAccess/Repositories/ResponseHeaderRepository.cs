using System.Collections.Generic;
using System.Linq;
using Compilenix.HttpHeaderSurvey.Implementation.Shared;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories;

namespace Compilenix.HttpHeaderSurvey.Implementation.DataAccess.Repositories
{
    public class ResponseHeaderRepository : Repository<ResponseHeader>, IResponseHeaderRepository
    {
        private DataAccessContext DataAccessContext => Context as DataAccessContext;

        public ResponseHeaderRepository(DataAccessContext context) : base(context)
        {
        }

        public override ResponseHeader Add(ResponseHeader header)
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

        /// <summary>
        /// Ignores Case
        /// </summary>
        public IEnumerable<ResponseHeader> GetByHeader(string header)
        {
            header = header.ToLower();
            return Entities?.Where(h => h.Key == header).ToList();
        }

        /// <summary>
        /// Ignores Case
        /// </summary>
        public ResponseHeader GetByHeaderAndValue(string header, string value)
        {
            var hash = HashUtils.Hash(value.ToLower());
            header = header.ToLower();
            return Entities?.SingleOrDefault(h => h.Key == header && h.ValueHash == hash);
        }

        /// <summary>
        /// If succeded the added object else null.
        /// </summary>
        public ResponseHeader AddIfNotExisting(ResponseHeader header)
        {
            return !ContainsResponseHeader(header.Key, header.Value) ? Add(header) : null;
        }

        /// <summary>
        /// Ignores Case.
        /// </summary>
        public bool ContainsResponseHeader(string header, string headerValue)
        {
            header = header.ToLower();
            headerValue = headerValue.ToLower();

            var headerValueHash = HashUtils.Hash(headerValue);
            var containsHeader = Entities.Local.Any(j => j.Key == header && j.ValueHash == headerValueHash);

            return containsHeader ? containsHeader : Entities.Any(j => j.Key == header && j.ValueHash == headerValueHash);
        }
    }
}