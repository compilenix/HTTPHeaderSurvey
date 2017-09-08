using System.Diagnostics;
using Compilenix.HttpHeaderSurvey.Integration.Shared.IoC;

namespace Compilenix.HttpHeaderSurvey.Implementation.Shared.IoC
{
    [DebuggerStepThrough]
    public class IoC
    {
        private static IIocContainer _currentContainer;

        public static IIocContainer CurrentContainer
        {
            get { return _currentContainer ?? (_currentContainer = new IoCContainerWrapper()); }
            set { _currentContainer = value; }
        }

        public static IIoCScope BeginLifetimeScope()
        {
            return CurrentContainer.BeginScope();
        }

        public static T Resolve<T>()
            where T : class => CurrentContainer.Resolve<T>();
    }
}