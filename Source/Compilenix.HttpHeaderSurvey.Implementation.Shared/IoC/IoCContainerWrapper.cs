using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Compilenix.HttpHeaderSurvey.Integration.Shared.IoC;
using SimpleInjector;
using SimpleInjector.Diagnostics;
using SimpleInjector.Extensions.LifetimeScoping;

namespace Compilenix.HttpHeaderSurvey.Implementation.Shared.IoC
{
    [DebuggerStepThrough]
    public class IoCContainerWrapper : IIocContainer
    {
        private readonly Container _container;

        private bool _containerFinnishedRegistrations;
        private Action _onRegistrationsFinnished;

        public IoCContainerWrapper()
        {
            _containerFinnishedRegistrations = false;
            _container = new Container { Options = { DefaultScopedLifestyle = new LifetimeScopeLifestyle(), DefaultLifestyle = Lifestyle.Transient } };
        }

        private static Lifestyle ConvertLifetimeType(InstanceLifetimeType lifetime)
        {
            switch (lifetime)
            {
                case InstanceLifetimeType.Scoped:
                    return Lifestyle.Scoped;
                case InstanceLifetimeType.SingleInstance:
                    return Lifestyle.Singleton;
                case InstanceLifetimeType.Transient:
                    return Lifestyle.Transient;
                default:
                    throw new NotSupportedException("LifetimeScope not found: " + lifetime);
            }
        }

        public IIocContainer Register<TFrom, TTo>()
            where TFrom : class where TTo : class, TFrom
        {
            return Register<TFrom, TTo>(InstanceLifetimeType.Transient);
        }

        public IIocContainer Register<TFrom, TTo>(InstanceLifetimeType lifetime)
            where TFrom : class where TTo : class, TFrom
        {
            _container.Register<TFrom, TTo>(ConvertLifetimeType(lifetime));
            _onRegistrationsFinnished += () => DiagnosticWarningDisposableTransientHandler(typeof(TFrom));
            return this;
        }

        public IIocContainer Register(Type from, Type to)
        {
            return Register(from, to, InstanceLifetimeType.Transient);
        }

        public IIocContainer Register(Type from, Type to, InstanceLifetimeType lifetime)
        {
            _container.Register(from, to, ConvertLifetimeType(lifetime));
            return this;
        }

        public IIocContainer Register<TInterface>(TInterface instance)
            where TInterface : class
        {
            return Register(instance, InstanceLifetimeType.SingleInstance);
        }

        public IIocContainer Register<TInterface>(TInterface instance, InstanceLifetimeType lifetime)
            where TInterface : class
        {
            _container.Register(() => instance);
            return this;
        }

        public void Build()
        {
            InvokeOnRegistrationsFinnished();
            _container.Verify();
        }

        public IIoCScope BeginScope()
        {
            return new IoCScope(_container.BeginLifetimeScope());
        }

        public void Register(Assembly assembly, Func<Type, bool> whereFunc)
        {
            Register(assembly, whereFunc, InstanceLifetimeType.Transient);
        }

        public T Resolve<T>()
            where T : class
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            return _container.GetInstance<T>();
        }

        public IIocContainer Register<T>()
            where T : class
        {
            return Register<T>(InstanceLifetimeType.Transient);
        }

        public IIocContainer Register<T>(InstanceLifetimeType lifetime)
            where T : class
        {
            _container.Register<T>(ConvertLifetimeType(lifetime));
            _onRegistrationsFinnished += () => DiagnosticWarningDisposableTransientHandler(typeof(T));
            return this;
        }

        public void Register(Assembly assembly, Func<Type, bool> whereFunc, InstanceLifetimeType lifetime)
        {
            var registrations = assembly.GetExportedTypes().Where(type => type?.GetInterfaces().Any() ?? false).Where(whereFunc).Select(type => new { Service = type?.GetInterfaces().Last(), Implementation = type });

            foreach (var reg in registrations)
            {
                _container.Register(reg.Service, reg.Implementation, ConvertLifetimeType(lifetime));
                _onRegistrationsFinnished += () => DiagnosticWarningDisposableTransientHandler(reg.Service);
            }
        }

        private void DiagnosticWarningDisposableTransientHandler(Type type)
        {
            _container.GetRegistration(type)?.Registration?.SuppressDiagnosticWarning(DiagnosticType.DisposableTransientComponent, "Disposal is handled by application code.");
        }

        private void InvokeOnRegistrationsFinnished()
        {
            if (_containerFinnishedRegistrations)
            {
                return;
            }

            _onRegistrationsFinnished?.Invoke();

            _containerFinnishedRegistrations = true;
        }
    }
}