using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.SqlServer;
using Implementation.DataAccess.EntityConfigurations;
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
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();

            modelBuilder.Conventions.Add(new ForeignKeyNamingConvention());

            modelBuilder.Configurations.Add(new RequestJobConfiguration());
            modelBuilder.Configurations.Add(new RequestHeaderConfiguration());
            modelBuilder.Configurations.Add(new ResponseMessageConfiguration());
            modelBuilder.Configurations.Add(new ResponseHeaderConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}