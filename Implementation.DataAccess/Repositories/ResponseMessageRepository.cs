using Integration.DataAccess.Entitys;
using Integration.DataAccess.Repositories;

namespace Implementation.DataAccess.Repositories
{
    public class ResponseMessageRepository : Repository<ResponseMessage>, IResponseMessageRepository
    {
        private DataAccessContext DataAccessContext => Context as DataAccessContext;

        public ResponseMessageRepository(DataAccessContext context) : base(context)
        {
        }
    }
}