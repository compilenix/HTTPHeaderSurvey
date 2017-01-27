using System;

namespace Integration.DataAccess
{
    public interface IUnitOfWork : IDisposable
    {
        TRepository Repository<TRepository>() where TRepository : class;
        int Complete();
    }
}