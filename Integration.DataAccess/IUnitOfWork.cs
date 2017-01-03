using System;
using Integration.DataAccess.Repositories;

namespace Integration.DataAccess
{
    public interface IUnitOfWork : IDisposable
    {
        IRequestJobRepository RequestJobs { get; }

        int Complete();
    }
}