using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Implementation.Shared;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories;

namespace Compilenix.HttpHeaderSurvey.Implementation.DataAccess.Repositories
{
    public class RequestHeaderRepository : Repository<RequestHeader>, IRequestHeaderRepository
    {
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

        public async Task<IEnumerable<RequestHeader>> GetByHeaderAsync(string header)
        {
            header = header.ToLower();
            return await Entities?.Where(h => h.Key == header).ToListAsync();
        }

        public async Task<bool> ContainsRequestHeaderAsync(string header, string headerValue)
        {
            header = header.ToLower();
            headerValue = headerValue.ToLower();

            var headerValueHash = HashUtils.Hash(headerValue);
            return await Entities.AnyAsync(j => j.Key == header && j.ValueHash == headerValueHash);
        }

        public async Task<RequestHeader> AddIfNotExistingAsync(RequestHeader header)
        {
            return !await ContainsRequestHeaderAsync(header.Key, header.Value) ? Add(header) : null;
        }
    }
}