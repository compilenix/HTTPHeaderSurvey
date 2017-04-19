﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories;
using JetBrains.Annotations;

namespace Compilenix.HttpHeaderSurvey.Integration.Domain.Modules
{
    public interface IResponseHeaderModule : IBaseModule<IResponseHeaderRepository, ResponseHeader>
    {
        [ItemNotNull]
        [NotNull]
        Task<List<ResponseHeader>> GetResponseHeadersFromListAsync([NotNull] IEnumerable<KeyValuePair<string, IEnumerable<string>>> headerList, [NotNull] IUnitOfWork unit);
    }
}