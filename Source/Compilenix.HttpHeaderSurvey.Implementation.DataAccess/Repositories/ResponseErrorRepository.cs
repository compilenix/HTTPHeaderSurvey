using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories;


namespace Compilenix.HttpHeaderSurvey.Implementation.DataAccess.Repositories
{
    
    public class ResponseErrorRepository : Repository<ResponseError>, IResponseErrorRepository
    {
        public ResponseErrorRepository( DataAccessContext context) : base(context)
        {
        }
    }
}