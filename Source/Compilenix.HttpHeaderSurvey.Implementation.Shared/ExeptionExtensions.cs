using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Compilenix.HttpHeaderSurvey.Implementation.Shared
{
    [DebuggerStepThrough]
    public static class ExeptionExtensions
    {
        public static IEnumerable<string> GetAllMessages(this Exception exception)
        {
            var messages = new List<string>();
            var currentException = exception;

            if (currentException == null)
            {
                return messages;
            }

            do
            {
                messages.Add(currentException.Message);
                currentException = currentException.InnerException;
            }
            while (currentException != null);

            return messages.Where(x => x != null);
        }
    }
}