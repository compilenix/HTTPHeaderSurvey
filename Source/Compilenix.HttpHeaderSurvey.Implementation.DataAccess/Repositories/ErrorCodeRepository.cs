using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories;
using JetBrains.Annotations;

namespace Compilenix.HttpHeaderSurvey.Implementation.DataAccess.Repositories
{
    [UsedImplicitly]
    public class ErrorCodeRepository : Repository<ErrorCode>, IErrorCodeRepository
    {
        public ErrorCodeRepository([NotNull] DataAccessContext context) : base(context)
        {
        }

        public async Task<ErrorCode> AddIfNotExistingAsync(ErrorCode code)
        {
            if (string.IsNullOrWhiteSpace(code.Message))
            {
                throw new ArgumentException($"{nameof(code)}.{nameof(code.Message)} must not be null or empty");
            }

            var lowercaseCodeMessage = code.Message.ToLower();
            var listAsync = Entities.Where(x => x.Code == code.Code || x.Message.ToLower() == lowercaseCodeMessage).Take(1).ToListAsync();
            if (listAsync != null)
            {
                var exists = await listAsync;
                return exists?.Any() ?? false ? exists.First() : Add(code);
            }

            return Add(code);
        }
    }
}