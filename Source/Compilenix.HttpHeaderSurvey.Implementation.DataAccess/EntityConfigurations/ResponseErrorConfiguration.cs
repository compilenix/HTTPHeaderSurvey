using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using JetBrains.Annotations;

namespace Compilenix.HttpHeaderSurvey.Implementation.DataAccess.EntityConfigurations
{
    [UsedImplicitly]
    public class ResponseErrorConfiguration : BaseEntityConfiguration<ResponseError>
    {
        public ResponseErrorConfiguration()
        {
            Property(x => x.Message);

            Property(x => x.OriginalMessage);

            Property(x => x.StackTrace);

            HasOptional(x => x.ErrorCode);

            HasRequired(x => x.ResponseMessage);
        }
    }
}