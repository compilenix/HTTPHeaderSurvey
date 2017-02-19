using System.Reflection;
using Compilenix.HttpHeaderSurvey.Implementation.DataAccess;
using Compilenix.HttpHeaderSurvey.Implementation.Shared.IoC;
using Compilenix.HttpHeaderSurvey.Integration.Domain;
using Compilenix.HttpHeaderSurvey.Integration.Shared.IoC;

namespace Compilenix.HttpHeaderSurvey.Implementation.Domain
{
    public static class DomainBootstrapper
    {
        public static void Initialize()
        {
            DataAccessBootstrapper.Initialize();
            MappingUtils.InitializeMapper();
            RegisterDependencies();
        }

        private static void RegisterDependencies()
        {
            var container = IoC.CurrentContainer;

            container.Register<IIoCScope, IoCScope>(InstanceLifetimeType.Scoped);

            var domain = Assembly.GetExecutingAssembly();
            container.Register(domain, type => type.Name.EndsWith("Module"), InstanceLifetimeType.Transient);
            container.Register<IApplicationConfigurationCollection, ApplicationConfigurationCollection>(InstanceLifetimeType.SingleInstance);
            container.Register<IRequestJobWorker, RequestJobWorker>(InstanceLifetimeType.SingleInstance);

            IoC.CurrentContainer = container;
        }
    }
}