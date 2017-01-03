using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using Integration.DataAccess.Entitys;

namespace Implementation.DataAccess.EntityConfigurations
{
    public class RequestJobConfiguration : EntityTypeConfiguration<RequestJob>
    {
        public RequestJobConfiguration()
        {
            HasKey(p => p.Id);

            Property(p => p.Id)?.HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute()));

            Property(p => p.HttpVersion)?.IsRequired()?.HasMaxLength(4);

            Property(p => p.IsCurrentlyScheduled)?.IsRequired();

            Property(p => p.IsRunOnce)?.IsRequired();

            Property(p => p.LastCompletedDateTime)?.IsOptional();

            Property(p => p.Method)?
                .IsRequired()?.HasMaxLength(16)?.HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute()));

            Property(p => p.Uri)?.IsRequired();

            Property(p => p.UriHash)?
                .IsRequired()?.HasMaxLength(64)?.HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute()));

            HasMany(j => j.Headers)?.WithMany(h => h.RequestJobs)?.Map(
                m =>
                    {
                        m?.ToTable($"Linked{nameof(RequestJob)}{nameof(RequestHeader)}s");
                        m?.MapLeftKey($"{nameof(RequestJob)}Id");
                        m?.MapRightKey($"{nameof(RequestHeader)}Id");
                    });

            Property(p => p.DateCreated)?.IsRequired();
        }
    }
}