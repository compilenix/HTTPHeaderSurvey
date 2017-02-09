using Integration.DataAccess;
using Integration.DataAccess.Entitys;
using Integration.DataAccess.Repositories;
using Integration.Domain.Modules;

namespace Implementation.Domain.Modules
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
