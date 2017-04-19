using System.Diagnostics;
using Compilenix.HttpHeaderSurvey.Integration.Shared.IoC;
using JetBrains.Annotations;

namespace Compilenix.HttpHeaderSurvey.Implementation.Shared.IoC
{
    [DebuggerStepThrough]
    [UsedImplicitly]
    public class IoC
    {
        [CanBeNull]
        private static IIocContainer _currentContainer;

        [NotNull]
        public static IIocContainer CurrentContainer
        {
            get { return _currentContainer ?? (_currentContainer = new IoCContainerWrapper()); }
            set { _currentContainer = value; }
        }

        [NotNull]
        public static IIoCScope BeginLifetimeScope()
        {
            return CurrentContainer.BeginScope();
        }

        [NotNull]
        public static T Resolve<T>() where T : class => CurrentContainer.Resolve<T>();
    }
}