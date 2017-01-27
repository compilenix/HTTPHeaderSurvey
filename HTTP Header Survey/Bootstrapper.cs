using Implementation.Domain;
using Implementation.Shared.IoC;

namespace HTTPHeaderSurvey
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