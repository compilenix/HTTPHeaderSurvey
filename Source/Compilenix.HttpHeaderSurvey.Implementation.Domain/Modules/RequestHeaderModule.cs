using Compilenix.HttpHeaderSurvey.Integration.DataAccess;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories;
using Compilenix.HttpHeaderSurvey.Integration.Domain.Modules;

namespace Compilenix.HttpHeaderSurvey.Implementation.Domain.Modules
{
    public class RequestHeaderModule : BaseModule<IRequestHeaderRepository, RequestHeader>, IRequestHeaderModule
    {
        public RequestHeaderModule(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        /// <summary>Add item or update existing</summary>
        public override RequestHeader AddOrUpdate(RequestHeader item)
        {
            return UnitOfWork.Repository<IRequestHeaderRepository>().AddIfNotExisting(item);
        }
    }
}