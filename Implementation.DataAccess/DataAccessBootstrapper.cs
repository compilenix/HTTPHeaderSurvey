using System.Reflection;
using Implementation.Shared.IoC;
using Integration.DataAccess;
using Integration.Shared.IoC;

namespace Implementation.DataAccess
{
    public static class DataAccessBootstrapper
    {
        public static void Initialize()
        {
            RegisterDependencies();
        }

        private static void RegisterDependencies()
        {
            var container = IoC.CurrentContainer;

            container.Register<DataAccessContext>(InstanceLifetimeTypes.Scoped);
            container.Register<IUnitOfWork, UnitOfWork>(InstanceLifetimeTypes.Transient);

            var dataAccess = Assembly.GetExecutingAssembly();
            container.Register(dataAccess, type => type.Name.EndsWith("Repository"), InstanceLifetimeTypes.Scoped);

            IoC.CurrentContainer = container;
        }
    }
}