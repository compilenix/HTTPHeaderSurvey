using System;
using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories;


namespace Compilenix.HttpHeaderSurvey.Integration.Domain.Modules
{
    public interface IResponseErrorModule : IBaseModule<IResponseErrorRepository, ResponseError>
    {
        /// <summary>
        /// Returns true if this was a known error.
        /// </summary>
        
        Task<(bool isKnownError, bool isPermanentError)> ProcessAsync( ResponseMessage messageWithError,  Exception error,  IUnitOfWork unit);
    }
}