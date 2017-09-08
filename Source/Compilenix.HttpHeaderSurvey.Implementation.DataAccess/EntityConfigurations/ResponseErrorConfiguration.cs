using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;

namespace Compilenix.HttpHeaderSurvey.Implementation.DataAccess.EntityConfigurations
{
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