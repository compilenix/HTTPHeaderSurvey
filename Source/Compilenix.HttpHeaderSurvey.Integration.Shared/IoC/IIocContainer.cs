using System;
using System.Reflection;

namespace Compilenix.HttpHeaderSurvey.Integration.Shared.IoC
{
    public interface IIocContainer
    {
        IIoCScope BeginScope();
        void Build();
        void Register(Assembly assembly, Func<Type, bool> whereFunc);
        void Register(Assembly assembly, Func<Type, bool> whereFunc, InstanceLifetimeType lifetime);
        IIocContainer Register(Type from, Type to);
        IIocContainer Register(Type from, Type to, InstanceLifetimeType lifetime);
        IIocContainer Register<TInterface>(TInterface instance) where TInterface : class;
        IIocContainer Register<TInterface>(TInterface instance, InstanceLifetimeType lifetime) where TInterface : class;
        IIocContainer Register<T>() where T : class;
        IIocContainer Register<T>(InstanceLifetimeType lifetime) where T : class;
        IIocContainer Register<TFrom, TTo>() where TFrom : class where TTo : class, TFrom;
        IIocContainer Register<TFrom, TTo>(InstanceLifetimeType lifetime) where TFrom : class where TTo : class, TFrom;
        T Resolve<T>() where T : class;
    }
}