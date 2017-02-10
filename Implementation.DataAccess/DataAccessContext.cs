using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.SqlServer;
using Integration.DataAccess.Entitys;

namespace Implementation.DataAccess
{
    public class DataAccessContext : DbContext
    {
        public DbSet<RequestJob> RequestJobs { get; set; }
        public DbSet<RequestHeader> RequestHeaders { get; set; }
        public DbSet<ApplicationLog> ApplicationLogs { get; set; }
        public DbSet<ResponseHeader> ResponseHeaders { get; set; }
        public DbSet<ResponseMessage> ResponseMessages { get; set; }

        public DataAccessContext() : base("name=DataAccessContext")
        {
            // ReSharper disable once UnusedVariable
            var sqlfunctions = typeof(SqlFunctions);
            Configuration.LazyLoadingEnabled = false;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            if (modelBuilder.Configurations == null || modelBuilder.Conventions == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Add<ForeignKeyNamingConvention>();

            modelBuilder.Configurations?.AddFromAssembly(typeof(DataAccessContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}