using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Implementation.Shared;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories;
using JetBrains.Annotations;

namespace Compilenix.HttpHeaderSurvey.Implementation.DataAccess.Repositories
{
    [UsedImplicitly]
    public class ResponseHeaderRepository : Repository<ResponseHeader>, IResponseHeaderRepository
    {
        public ResponseHeaderRepository([NotNull] DataAccessContext context) : base(context)
        {
        }

        private static void ValidateResponseHeader(ResponseHeader header)
        {
            if (header == null)
            {
                throw new ArgumentNullException(nameof(header));
            }

            if (header.Key == null)
            {
                throw new ArgumentNullException(nameof(header.Key));
            }

            if (header.Value == null)
            {
                throw new ArgumentNullException(nameof(header.Value));
            }
        }

        public override ResponseHeader Add(ResponseHeader header)
        {
            ValidateResponseHeader(header);

            // ReSharper disable once PossibleNullReferenceException
            header.Key = header.Key.ToLower();

            switch (header.Key)
            {
                case "date":
                    return null;
            }

            // ReSharper disable once PossibleNullReferenceException
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
            // ReSharper disable once PossibleNullReferenceException
            return await Entities.Where(h => h.Key == header).ToListAsync();
        }

        /// <summary>
        /// Ignores Case
        /// </summary>
        public async Task<ResponseHeader> GetByHeaderAndValueAsync(string header, string value)
        {
            var hash = HashUtils.Hash(value.ToLower());
            header = header.ToLower();
            // ReSharper disable once PossibleNullReferenceException
            return await Entities.SingleOrDefaultAsync(h => h.Key == header && h.ValueHash == hash);
        }

        /// <summary>
        /// If succeded the added object else null.
        /// </summary>
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public async Task<ResponseHeader> AddIfNotExistingAsync(ResponseHeader header)
        {
            ValidateResponseHeader(header);
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
            var containsHeader = Entities.Local?.Any(j => j?.Key == header && j?.ValueHash == headerValueHash) ?? false;

            // ReSharper disable once PossibleNullReferenceException
            return containsHeader || await Entities.AnyAsync(j => j.Key == header && j.ValueHash == headerValueHash);
        }
    }
}