using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories;
using JetBrains.Annotations;

namespace Compilenix.HttpHeaderSurvey.Implementation.DataAccess.Repositories
{
    [UsedImplicitly]
    public class ResponseErrorRepository : Repository<ResponseError>, IResponseErrorRepository
    {
        public ResponseErrorRepository([NotNull] DataAccessContext context) : base(context)
        {
        }
    }
}