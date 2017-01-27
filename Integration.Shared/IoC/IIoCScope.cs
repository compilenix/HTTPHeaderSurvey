using System;

namespace Integration.Shared.IoC
{
    public interface IIoCScope : IDisposable
    {
        T Resolve<T>() where T : class;
        IIoCScope BeginLifetimeScope();
    }
}
