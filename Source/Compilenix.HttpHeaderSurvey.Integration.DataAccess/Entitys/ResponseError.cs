using System.Diagnostics.CodeAnalysis;

namespace Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys
{
    [SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
    public class ResponseError : BaseEntity
    {
        public string Message { get; set; }

        public string OriginalMessage { get; set; }

        public string StackTrace { get; set; }

        public virtual ErrorCode ErrorCode { get; set; }

        public virtual ResponseMessage ResponseMessage { get; set; }
    }
}