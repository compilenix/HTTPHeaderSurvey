using System;
using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories;
using JetBrains.Annotations;

namespace Compilenix.HttpHeaderSurvey.Integration.Domain.Modules
{
    public interface IResponseErrorModule : IBaseModule<IResponseErrorRepository, ResponseError>
    {
        /// <summary>
        /// Returns true if this was a known error.
        /// </summary>
        [NotNull]
        Task<bool> AddAsync([NotNull] ResponseMessage messageWithError, [NotNull] Exception error, [NotNull] IUnitOfWork unit);
    }
}