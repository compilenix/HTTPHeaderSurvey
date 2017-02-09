using Integration.DataAccess.Entitys;
using Integration.DataAccess.Repositories;

namespace Integration.Domain.Modules
{
    public interface IRequestHeaderModule : IBaseModule<IRequestHeaderRepository, RequestHeader>
    {
    }
}
