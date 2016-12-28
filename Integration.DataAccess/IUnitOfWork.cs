using Integration.DataAccess.Repositories;
using System;

namespace Integration.DataAccess
{
    public interface IUnitOfWork : IDisposable
    {
        IRequestJobRepository RequestJobs { get; }

        int Complete();
    }
}
