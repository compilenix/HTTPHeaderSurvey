using System;
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
            if (string.IsNullOrWhiteSpace(header.Key))
            {
                throw new AggregateException($"{typeof(RequestHeader).Name}.{nameof(header.Key)} must not be null");
            }

            if (string.IsNullOrWhiteSpace(header.Value))
            {
                throw new AggregateException($"{typeof(RequestHeader).Name}.{nameof(header.Key)} must not be null");
            }

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
            // ReSharper disable once PossibleNullReferenceException
            return await Entities.Where(h => h.Key == header).ToListAsync() ?? new List<RequestHeader>();
        }

        public async Task<bool> ContainsRequestHeaderAsync(string header, string headerValue)
        {
            header = header.ToLower();
            headerValue = headerValue.ToLower();

            var headerValueHash = HashUtils.Hash(headerValue);
            var anyAsync = Entities.AnyAsync(j => j.Key == header && j.ValueHash == headerValueHash);
            return anyAsync != null && await anyAsync;
        }

        public async Task<RequestHeader> AddIfNotExistingAsync(RequestHeader header)
        {
            if (header.Key == null)
            {
                return null;
            }

            if (header.Value == null)
            {
                return null;
            }

            return !await ContainsRequestHeaderAsync(header.Key, header.Value) ? Add(header) : null;
        }
    }
}