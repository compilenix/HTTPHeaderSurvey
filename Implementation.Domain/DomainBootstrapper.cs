using System.Reflection;
using Implementation.DataAccess;
using Implementation.Shared.IoC;
using Integration.Shared.IoC;

namespace Implementation.Domain
{
    public static class DomainBootstrapper
    {
        public static void Initialize()
        {
            DataAccessBootstrapper.Initialize();
            RegisterDependencies();
        }

        private static void RegisterDependencies()
        {
            var container = IoC.CurrentContainer;

            container.Register<IIoCScope, IoCScope>(InstanceLifetimeTypes.Scoped);

            var domain = Assembly.GetExecutingAssembly();
            container.Register(domain, type => type.Name.EndsWith("Module"), InstanceLifetimeTypes.Transient);

            IoC.CurrentContainer = container;
        }
    }
}
