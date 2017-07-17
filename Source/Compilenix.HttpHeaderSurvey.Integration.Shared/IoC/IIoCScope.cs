using System;
using JetBrains.Annotations;

namespace Compilenix.HttpHeaderSurvey.Integration.Shared.IoC
{
    public interface IIoCScope : IDisposable
    {
        [NotNull]
        IIoCScope BeginLifetimeScope();

        [NotNull]
        T Resolve<T>()
            where T : class;
    }
}