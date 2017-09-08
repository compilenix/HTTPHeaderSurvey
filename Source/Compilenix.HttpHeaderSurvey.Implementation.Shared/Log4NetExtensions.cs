using System;
using System.Diagnostics;
using log4net;
using log4net.Config;

namespace Compilenix.HttpHeaderSurvey.Implementation.Shared
{
    [DebuggerStepThrough]
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

            // ReSharper disable once AssignNullToNotNullAttribute
            return LogManager.GetLogger(obj);
        }

        public static ILog Log(this object obj) => Log(obj?.GetType());
    }
}