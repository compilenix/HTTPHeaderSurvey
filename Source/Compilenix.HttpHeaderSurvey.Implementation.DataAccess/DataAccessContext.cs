using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.SqlServer;

namespace Compilenix.HttpHeaderSurvey.Implementation.DataAccess
{
    public class DataAccessContext : DbContext
    {
        // ReSharper disable once UnusedMember.Local
        private static string Name => typeof(SqlFunctions).Name;

        public DataAccessContext() : base("name=DataAccessContext")
        {
            // ReSharper disable once PossibleNullReferenceException
            Configuration.LazyLoadingEnabled = true;
            Configuration.ProxyCreationEnabled = true;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // ReSharper disable PossibleNullReferenceException
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Add<ForeignKeyNamingConvention>();

            modelBuilder.Configurations.AddFromAssembly(typeof(DataAccessContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}