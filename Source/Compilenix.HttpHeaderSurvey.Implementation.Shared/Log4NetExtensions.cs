using System;
using System.Diagnostics;
using JetBrains.Annotations;
using log4net;
using log4net.Config;

namespace Compilenix.HttpHeaderSurvey.Implementation.Shared
{
    [DebuggerStepThrough]
    public static class Log4NetExtensions
    {
        private static bool _isLoggerInitialized;

        [NotNull]
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

        [NotNull]
        public static ILog Log(this object obj) => Log(obj?.GetType());
    }
}