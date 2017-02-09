using System;
using System.Linq;
using System.Reflection;
using Integration.Shared.IoC;
using SimpleInjector;
using SimpleInjector.Diagnostics;
using SimpleInjector.Extensions.LifetimeScoping;

namespace Implementation.Shared.IoC
{
    public class IoCContainerWrapper : IIocContainer
    {
        private readonly Container _container;
        private bool _containerFinnishedRegistrations;
        private Action _onRegistrationsFinnished;

        public IoCContainerWrapper()
        {
            _containerFinnishedRegistrations = false;
            _container = new Container();
            _container.Options.DefaultScopedLifestyle = new LifetimeScopeLifestyle();
            _container.Options.DefaultLifestyle = Lifestyle.Transient;
        }

        public IIocContainer Register<TFrom, TTo>() where TFrom : class where TTo : class, TFrom
        {
            return Register<TFrom, TTo>(InstanceLifetimeTypes.Transient);
        }

        public IIocContainer Register<TFrom, TTo>(InstanceLifetimeTypes lifetime) where TFrom : class where TTo : class, TFrom
        {
            _container.Register<TFrom, TTo>(ConvertLifetimeType(lifetime));
            _onRegistrationsFinnished += () => DiagnosticWarningDisposableTransientHandler(typeof(TFrom));
            return this;
        }

        public IIocContainer Register(Type from, Type to)
        {
            return Register(from, to, InstanceLifetimeTypes.Transient);
        }

        public IIocContainer Register(Type from, Type to, InstanceLifetimeTypes lifetime)
        {
            _container.Register(from, to, ConvertLifetimeType(lifetime));
            return this;
        }

        public IIocContainer Register<TInterface>(TInterface instance) where TInterface : class
        {
            return Register(instance, InstanceLifetimeTypes.SingleInstance);
        }

        public IIocContainer Register<TInterface>(TInterface instance, InstanceLifetimeTypes lifetime) where TInterface : class
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
            Register(assembly, whereFunc, InstanceLifetimeTypes.Transient);
        }

        public T Resolve<T>() where T : class
        {
            return _container.GetInstance<T>();
        }

        public IIocContainer Register<T>() where T : class
        {
            return Register<T>(InstanceLifetimeTypes.Transient);
        }

        public IIocContainer Register<T>(InstanceLifetimeTypes lifetime) where T : class
        {
            _container.Register<T>(ConvertLifetimeType(lifetime));
            _onRegistrationsFinnished += () => DiagnosticWarningDisposableTransientHandler(typeof(T));
            return this;
        }

        public void Register(Assembly assembly, Func<Type, bool> whereFunc, InstanceLifetimeTypes lifetime)
        {
            var registrations =
                assembly.GetExportedTypes()
                    .Where(type => type.GetInterfaces().Any())
                    .Where(whereFunc)
                    .Select(type => new { Service = type.GetInterfaces().Last(), Implementation = type });

            foreach (var reg in registrations)
            {
                _container.Register(reg.Service, reg.Implementation, ConvertLifetimeType(lifetime));
                _onRegistrationsFinnished += () => DiagnosticWarningDisposableTransientHandler(reg.Service);
            }
        }

        private Lifestyle ConvertLifetimeType(InstanceLifetimeTypes lifetime)
        {
            switch (lifetime)
            {
                case InstanceLifetimeTypes.Scoped:
                    return Lifestyle.Scoped;
                case InstanceLifetimeTypes.SingleInstance:
                    return Lifestyle.Singleton;
                case InstanceLifetimeTypes.Transient:
                    return Lifestyle.Transient;
                default:
                    throw new NotSupportedException("LifetimeScope not found: " + lifetime);
            }
        }

        private void DiagnosticWarningDisposableTransientHandler(Type type)
        {
            _container.GetRegistration(type)
                .Registration.SuppressDiagnosticWarning(DiagnosticType.DisposableTransientComponent, "Disposal is handled by application code.");
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