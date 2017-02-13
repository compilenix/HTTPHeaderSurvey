using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using Integration.DataAccess.Entitys;

namespace Implementation.DataAccess.EntityConfigurations
{
    public class ApplicationLogConfiguration : EntityTypeConfiguration<ApplicationLog>
    {
        public ApplicationLogConfiguration()
        {
            HasKey(p => p.Id);
            Property(x => x.Id)?.HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute()));

            Property(x => x.HostName)?.IsRequired()?.HasMaxLength(100);

            Property(x => x.Level)?.IsRequired();

            Property(x => x.Logger)?.IsRequired();

            Property(x => x.Message)?.IsRequired();

            Property(x => x.Method)?.IsRequired();

            Property(x => x.Thread)?.IsRequired();
        }
    }
}