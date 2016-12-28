using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Implementation.Shared
{
    public static class HashUtils
    {
        public static string Hash(string data)
        {
            using (var alg = SHA256CryptoServiceProvider.Create())
            {
                return alg?.ComputeHash(Encoding.UTF8.GetBytes(data))
                    .Aggregate(string.Empty, (current, b) => current + b.ToString("x2"));
            }
        }
    }
}
