using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;

namespace Compilenix.HttpHeaderSurvey.Implementation.Shared
{
    [DebuggerStepThrough]
    public static class HashUtils
    {
        public static string Hash([NotNull] string data)
        {
            using (var alg = SHA256.Create())
            {
                return alg?.ComputeHash(Encoding.UTF8.GetBytes(data)).Aggregate(string.Empty, (current, b) => current + b.ToString("x2"))?.ToLower();
            }
        }
    }
}