using System;
using Integration.Shared.IoC;
using SimpleInjector;

namespace Implementation.Shared.IoC
{
    public class IoCScope : IIoCScope
    {
        private readonly Scope _scope;

        public IoCScope(Scope scope)
        {
            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            _scope = scope;
        }

        public IIoCScope BeginLifetimeScope()
        {
            throw new NotSupportedException();
        }

        public void Dispose()
        {
            _scope?.Dispose();
        }

        public T Resolve<T>() where T : class
        {
            return _scope.GetInstance<T>();
        }
    }
}
