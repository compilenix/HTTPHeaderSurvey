using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using Integration.DataAccess.Entitys;

namespace Implementation.DataAccess.EntityConfigurations
{
    public class ResponseMessageConfiguration : EntityTypeConfiguration<ResponseMessage>
    {
        public ResponseMessageConfiguration()
        {
            HasKey(p => p.Id);

            Property(p => p.Id)?.HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute()));

            Property(p => p.StatusCode)?.IsRequired();

            Property(p => p.ProtocolVersion)?.IsRequired()?.HasMaxLength(4);

            HasRequired(p => p.RequestJob);

            HasMany(j => j.ResponseHeaders)?.WithMany(h => h.ResponseMessages)?.Map(
                m =>
                    {
                        m?.ToTable($"Linked{nameof(ResponseMessage)}{nameof(ResponseHeader)}s");
                        m?.MapLeftKey($"{nameof(ResponseMessage)}Id");
                        m?.MapRightKey($"{nameof(ResponseHeader)}Id");
                    });

            Property(p => p.DateCreated)?.IsRequired();
        }
    }
}