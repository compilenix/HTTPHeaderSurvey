﻿using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;

namespace Compilenix.HttpHeaderSurvey.Implementation.DataAccess.EntityConfigurations
{
    public class RequestHeaderConfiguration : EntityTypeConfiguration<RequestHeader>
    {
        public RequestHeaderConfiguration()
        {
            var uniqueIndexKeyValue = $"IX_{nameof(RequestHeader)}_{nameof(RequestHeader.Key)}_{nameof(RequestHeader.ValueHash)}";

            HasKey(p => p.Id);

            Property(p => p.Id)?.HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute()));

            Property(p => p.Key)?.IsRequired()?.HasMaxLength(64)?.HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute(uniqueIndexKeyValue) { IsUnique = true, Order = 1 }));

            Property(p => p.Value)?.IsRequired();

            Property(p => p.ValueHash)?.IsRequired()?.HasMaxLength(64)?.HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute(uniqueIndexKeyValue) { IsUnique = true, Order = 2 }));

            Property(p => p.DateCreated)?.IsRequired();
        }
    }
}