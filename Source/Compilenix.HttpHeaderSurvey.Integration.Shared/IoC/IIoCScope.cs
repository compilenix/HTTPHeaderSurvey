using System;


namespace Compilenix.HttpHeaderSurvey.Integration.Shared.IoC
{
    public interface IIoCScope : IDisposable
    {
        
        IIoCScope BeginLifetimeScope();

        
        T Resolve<T>()
            where T : class;
    }
}