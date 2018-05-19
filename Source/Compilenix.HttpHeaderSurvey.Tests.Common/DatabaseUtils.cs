using System.Data.Entity;
using System.Diagnostics;
using Compilenix.HttpHeaderSurvey.Implementation.DataAccess;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Compilenix.HttpHeaderSurvey.Tests.Common
{
    public class DatabaseUtils
    {
        private static string _connectionString;

        public static bool CleanUpDatabaseOnSucess(string connectionString)
        {
            if (TestContext.CurrentContext?.Result?.Outcome?.Equals(ResultState.Success) ?? false)
            {
                RemoveDatabase(connectionString);
                return true;
            }

            return false;
        }

        public static string GetRandomConnectionString(string dataSource = "localhost")
        {
            var dbname = $"HttpHeaderSurvey_UnitTest_{new StackTrace().GetFrame(1)?.GetMethod()?.DeclaringType?.Name}_{TestContext.CurrentContext?.Test?.Name}_{TestContext.CurrentContext?.Random?.NextUInt(99)}";

            if (dbname.Length >= 128)
            {
                dbname = $"HttpHeaderSurvey_UnitTest_{TestContext.CurrentContext?.Test?.Name}_{TestContext.CurrentContext?.Random?.NextUInt(99)}";
            }

            if (dbname.Length >= 128)
            {
                dbname = $"HttpHeaderSurvey_UnitTest_{TestContext.CurrentContext?.Random?.NextUInt(9999)}";
            }

            return $"Data Source={dataSource};Initial Catalog={dbname};Integrated Security=True;Encrypt=True;TrustServerCertificate=True;multipleactiveresultsets=True;application name=EntityFramework 6 Unit Testing";
        }

        public static void RemoveDatabase(string connectionString)
        {
            Database.Delete(connectionString);
        }

        public static void RestoreDefaultConnectionString()
        {
            if (_connectionString == null) return;
            DataAccessContext.ConnectionString = _connectionString;
            _connectionString = null;
        }

        public static void SetCustomConnectionString(string value)
        {
            if (_connectionString == null)
            {
                _connectionString = DataAccessContext.ConnectionString;
            }

            DataAccessContext.ConnectionString = value;
        }
    }
}
