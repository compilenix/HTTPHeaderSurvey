using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;

namespace Compilenix.HttpHeaderSurvey.Implementation.DataAccess.EntityConfigurations
{
    public class ErrorCodeConfiguration : BaseEntityConfiguration<ErrorCode>
    {
        public ErrorCodeConfiguration()
        {
            Property(x => x.Message)?.IsRequired();

            Property(x => x.Code)?.IsRequired()?.HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute()));

            HasMany(x => x.ResponseErrors);
        }
    }
}