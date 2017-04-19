using System;
using System.Reflection;
using JetBrains.Annotations;

namespace Compilenix.HttpHeaderSurvey.Integration.Shared.IoC
{
    public interface IIocContainer
    {
        [NotNull]
        IIoCScope BeginScope();

        void Build();
        void Register([NotNull] Assembly assembly, [NotNull] Func<Type, bool> whereFunc);
        void Register([NotNull] Assembly assembly, [NotNull] Func<Type, bool> whereFunc, InstanceLifetimeType lifetime);

        [NotNull]
        IIocContainer Register([NotNull] Type from, [NotNull] Type to);

        [NotNull]
        IIocContainer Register([NotNull] Type from, [NotNull] Type to, InstanceLifetimeType lifetime);

        [NotNull]
        IIocContainer Register<TInterface>([NotNull] TInterface instance) where TInterface : class;

        [NotNull]
        IIocContainer Register<TInterface>([NotNull] TInterface instance, InstanceLifetimeType lifetime) where TInterface : class;

        [NotNull]
        IIocContainer Register<T>() where T : class;

        [NotNull]
        IIocContainer Register<T>(InstanceLifetimeType lifetime) where T : class;

        [NotNull]
        IIocContainer Register<TFrom, TTo>() where TFrom : class where TTo : class, TFrom;

        [NotNull]
        IIocContainer Register<TFrom, TTo>(InstanceLifetimeType lifetime) where TFrom : class where TTo : class, TFrom;

        [NotNull]
        T Resolve<T>() where T : class;
    }
}