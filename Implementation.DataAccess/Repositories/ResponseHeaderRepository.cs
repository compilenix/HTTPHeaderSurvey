using System;
using System.Collections.Generic;
using System.Linq;
using Implementation.Shared;
using Integration.DataAccess.Entitys;
using Integration.DataAccess.Repositories;

namespace Implementation.DataAccess.Repositories
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
            return Entities?.Where(h => h.Key.ToLower() == header.ToLower()).ToList();
        }

        /// <summary>
        /// Ignores Case
        /// </summary>
        public ResponseHeader GetByHeaderAndValue(string header, string value)
        {
            var hash = HashUtils.Hash(value.ToLower());
            return Entities?.SingleOrDefault(h => h.Key.ToLower() == header.ToLower() && h.ValueHash == hash);
        }

        public override IEnumerable<ResponseHeader> AddRange(IEnumerable<ResponseHeader> headers)
        {
            foreach (var header in headers)
            {
                yield return Add(header);
            }
        }

        /// <summary>
        /// If succeded the added object else null.
        /// </summary>
        public ResponseHeader AddIfNotExisting(ResponseHeader header)
        {
            return !ContainsResponseHeader(header.Key, header.Value) ? Add(header) : null;
        }

        /// <summary>
        /// If succeded the added object else an empty list.
        /// </summary>
        public IEnumerable<ResponseHeader> AddIfNotExisting(IEnumerable<ResponseHeader> headers)
        {
            var addedHeaders = new List<ResponseHeader>();

            foreach (var requestHeader in headers)
            {
                addedHeaders.Add(AddIfNotExisting(requestHeader));
            }

            return addedHeaders;
        }

        /// <summary>
        /// Ignores Case.
        /// </summary>
        public bool ContainsResponseHeader(string header, string headerValue)
        {
            var headerValueHash = HashUtils.Hash(headerValue.ToLower());
            return Entities.Any(j => j.Key.ToLower() == header.ToLower() && j.ValueHash == headerValueHash);
        }
    }
}