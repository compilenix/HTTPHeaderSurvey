using System;
using System.Collections.Generic;

namespace Implementation.Shared
{
    public static class ExeptionExtensions
    {
        public static IEnumerable<string> GetAllMessages(this Exception exception)
        {
            var messages = new List<string>();
            var currentException = exception;

            do
            {
                messages.Add(currentException?.Message);
                currentException = currentException?.InnerException;
            }
            while (currentException != null);

            return messages;
        }
    }
}
