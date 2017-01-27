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

            IoC.CurrentContainer = container;
        }
    }
}
