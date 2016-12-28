using log4net;
using log4net.Config;
using System;

namespace Implementation.Shared
{
    public static class Log4NetExtensions
    {
        private static bool _isLoggerInitialized;
        public static ILog Log(this Type obj)
        {

            if (!_isLoggerInitialized)
            {
                _isLoggerInitialized = true;
                XmlConfigurator.Configure();
            }

            return LogManager.GetLogger(obj);
        }
    }
}