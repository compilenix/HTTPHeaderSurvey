using System.Reflection;
using Compilenix.HttpHeaderSurvey.Implementation.Shared.IoC;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess;
using Compilenix.HttpHeaderSurvey.Integration.Shared.IoC;

namespace Compilenix.HttpHeaderSurvey.Implementation.DataAccess
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

            container.Register<DataAccessContext>(InstanceLifetimeType.Scoped);
            container.Register<IUnitOfWork, UnitOfWork>(InstanceLifetimeType.Transient);

            var dataAccess = Assembly.GetExecutingAssembly();
            container.Register(dataAccess, type => type.Name.EndsWith("Repository"), InstanceLifetimeType.Scoped);

            IoC.CurrentContainer = container;
        }
    }
}