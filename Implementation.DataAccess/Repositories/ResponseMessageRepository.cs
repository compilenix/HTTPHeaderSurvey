using System;
using System.Collections.Generic;
using System.Data.Entity;
using Integration.DataAccess.Entitys;
using Integration.DataAccess.Repositories;

namespace Implementation.DataAccess.Repositories
{
    public class ResponseMessageRepository : Repository<ResponseMessage>, IResponseMessageRepository
    {
        public ResponseMessageRepository(DataAccessContext context) : base(context)
        {
        }
    }
}
