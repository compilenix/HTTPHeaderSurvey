using Implementation.DataAccess.EntityConfigurations;
using Integration.DataAccess.Entitys;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace Implementation.DataAccess
{
    public class HttpHeaderDbContext : DbContext
    {
        public HttpHeaderDbContext()
            : base("name=HttpHeaderDbContext")
        {
            Configuration.LazyLoadingEnabled = false;
        }

        //TODO DbSet's goes here
        public virtual DbSet<RequestJob> RequestJobs { get; set; }
        public virtual DbSet<RequestHeader> RequestHeaders { get; set; }
        public virtual DbSet<ApplicationLog> ApplicationLogs { get; set; }

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
