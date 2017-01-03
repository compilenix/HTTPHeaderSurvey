using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.SqlServer;
using Implementation.DataAccess.EntityConfigurations;
using Integration.DataAccess.Entitys;

namespace Implementation.DataAccess
{
    public class DataAccessContext : DbContext
    {
        private SqlProviderServices _sqlProviderServices;
        public DbSet<RequestJob> RequestJobs { get; set; }
        public DbSet<RequestHeader> RequestHeaders { get; set; }
        public DbSet<ApplicationLog> ApplicationLogs { get; set; }

        public DataAccessContext() : base("name=DataAccessContext")
        {
            _sqlProviderServices = SqlProviderServices.Instance;
            Configuration.LazyLoadingEnabled = false;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();

            modelBuilder.Configurations.Add(new RequestJobConfiguration());
            modelBuilder.Configurations.Add(new RequestHeaderConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}