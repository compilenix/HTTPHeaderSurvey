using Compilenix.HttpHeaderSurvey.Implementation.Domain;
using Compilenix.HttpHeaderSurvey.Implementation.Shared.IoC;

namespace Compilenix.HttpHeaderSurvey.Agent.Cli
{
    internal static class Bootstrapper
    {
        public static void Initialize()
        {
            RegisterDependencies();
        }

        private static void RegisterDependencies()
        {
            DomainBootstrapper.Initialize();
            var container = IoC.CurrentContainer;

            //builder.Register<ISomeModule, SomeModule>(InstanceLifetimeTypes.SingleInstance);

            container.Build();
        }
    }
}