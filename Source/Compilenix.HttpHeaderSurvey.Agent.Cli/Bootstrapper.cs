using Compilenix.HttpHeaderSurvey.Implementation.Domain;
using Compilenix.HttpHeaderSurvey.Implementation.Shared.IoC;

namespace Compilenix.HttpHeaderSurvey.Agent.Cli
{
    internal static class Bootstrapper
    {
        public static void Initialize()
        {
            DomainBootstrapper.Initialize();
            RegisterDependencies();
        }

        private static void RegisterDependencies()
        {
            var container = IoC.CurrentContainer;

            //builder.Register<ISomeModule, SomeModule>(InstanceLifetimeType.SingleInstance);

            container.Build();
        }
    }
}