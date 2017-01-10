using System;
using Integration.DataAccess.Repositories;

namespace Integration.DataAccess
{
    public interface IUnitOfWork : IDisposable
    {
        IRequestJobRepository RequestJobs { get; }
        IRequestHeaderRepository RequestHeaders { get; }
        IResponseMessageRepository ResponseMessages { get; }
        IResponseHeaderRepository ResponseHeaders { get; }

        int Complete();
    }
}