using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using JetBrains.Annotations;

namespace Compilenix.HttpHeaderSurvey.Implementation.DataAccess.EntityConfigurations
{
    [UsedImplicitly]
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