using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using Integration.DataAccess.Entitys;

namespace Implementation.DataAccess.EntityConfigurations
{
    public class ResponseHeaderConfiguration : EntityTypeConfiguration<ResponseHeader>
    {
        public ResponseHeaderConfiguration()
        {
            var uniqueIndexKeyValue = $"IX_{nameof(ResponseHeader)}_{nameof(ResponseHeader.Key)}_{nameof(ResponseHeader.ValueHash)}";

            HasKey(p => p.Id);

            Property(p => p.Id)?.HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute()));

            Property(p => p.Key)?
                .IsRequired()?
                .HasMaxLength(64)?
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute(uniqueIndexKeyValue) { IsUnique = true, Order = 1 }));

            Property(p => p.Value)?.IsRequired();

            Property(p => p.ValueHash)?
                .IsRequired()?
                .HasMaxLength(64)?
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute(uniqueIndexKeyValue) { IsUnique = true, Order = 2 }));

            Property(p => p.DateCreated)?.IsRequired();
        }
    }
}