using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories;


namespace Compilenix.HttpHeaderSurvey.Implementation.DataAccess.Repositories
{
    
    public class ResponseMessageRepository : Repository<ResponseMessage>, IResponseMessageRepository
    {
        public ResponseMessageRepository( DataAccessContext context) : base(context)
        {
        }
    }
}