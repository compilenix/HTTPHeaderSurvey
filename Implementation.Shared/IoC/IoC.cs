using Integration.Shared.IoC;

namespace Implementation.Shared.IoC
{
    public class IoC
    {
        private static IIocContainer _currentContainer;

        public static IIoCScope BeginLifetimeScope()
        {
            return CurrentContainer.BeginScope();
        }

        public static IIocContainer CurrentContainer
        {
            get { return _currentContainer ?? (_currentContainer = new IoCContainerWrapper()); }
            set { _currentContainer = value; }
        }

        public static T Resolve<T>() where T : class
        {
            return CurrentContainer.Resolve<T>();
        }
    }
}
