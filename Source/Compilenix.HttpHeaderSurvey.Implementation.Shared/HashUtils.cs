using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Compilenix.HttpHeaderSurvey.Implementation.Shared
{
    public static class HashUtils
    {
        public static string Hash(string data)
        {
            using (var alg = SHA256.Create())
            {
                return alg?.ComputeHash(Encoding.UTF8.GetBytes(data)).Aggregate(string.Empty, (current, b) => current + b.ToString("x2"))?.ToLower();
            }
        }
    }
}