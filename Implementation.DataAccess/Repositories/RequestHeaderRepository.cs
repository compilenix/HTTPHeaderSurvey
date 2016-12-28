using Implementation.Shared;
using Integration.DataAccess.Entitys;
using Integration.DataAccess.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

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
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            entity.ValueHash = HashUtils.Hash(entity.Value);
            var dbEntity = Entities?.SingleOrDefault(x => x.Key == entity.Key && x.ValueHash == entity.ValueHash);

            return dbEntity ?? base.Add(entity);
        }

        public IEnumerable<RequestHeader> GetByHeader(string header, bool ignoreCase = true)
        {
            return Entities?.Where(h => string.Equals(h.Key, header, StringComparison.CurrentCultureIgnoreCase)).ToList();
        }
    }
}
