﻿using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories;
using JetBrains.Annotations;

namespace Compilenix.HttpHeaderSurvey.Implementation.DataAccess.Repositories
{
    [UsedImplicitly]
    public class ResponseMessageRepository : Repository<ResponseMessage>, IResponseMessageRepository
    {
        public ResponseMessageRepository([NotNull] DataAccessContext context) : base(context)
        {
        }
    }
}