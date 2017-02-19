using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Implementation.Shared;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories;

namespace Compilenix.HttpHeaderSurvey.Implementation.DataAccess.Repositories
{
    public class ResponseHeaderRepository : Repository<ResponseHeader>, IResponseHeaderRepository
    {
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
        public async Task<IEnumerable<ResponseHeader>> GetByHeaderAsync(string header)
        {
            header = header.ToLower();
            return await Entities?.Where(h => h.Key == header).ToListAsync();
        }

        /// <summary>
        /// Ignores Case
        /// </summary>
        public async Task<ResponseHeader> GetByHeaderAndValueAsync(string header, string value)
        {
            var hash = HashUtils.Hash(value.ToLower());
            header = header.ToLower();
            return await Entities?.SingleOrDefaultAsync(h => h.Key == header && h.ValueHash == hash);
        }

        /// <summary>
        /// If succeded the added object else null.
        /// </summary>
        public async Task<ResponseHeader> AddIfNotExistingAsync(ResponseHeader header)
        {
            return !await ContainsResponseHeaderAsync(header.Key, header.Value) ? Add(header) : null;
        }

        /// <summary>
        /// Ignores Case.
        /// </summary>
        public async Task<bool> ContainsResponseHeaderAsync(string header, string headerValue)
        {
            header = header.ToLower();
            headerValue = headerValue.ToLower();

            var headerValueHash = HashUtils.Hash(headerValue);
            var containsHeader = Entities.Local.Any(j => j.Key == header && j.ValueHash == headerValueHash);

            return containsHeader ? containsHeader : await Entities.AnyAsync(j => j.Key == header && j.ValueHash == headerValueHash);
        }
    }
}