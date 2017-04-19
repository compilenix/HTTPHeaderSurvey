using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys
{
    [DebuggerStepThrough]
    [SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
    public class ResponseMessage : BaseEntity
    {
        public int? StatusCode { get; set; }

        public string ProtocolVersion { get; set; }

        public virtual RequestJob RequestJob { get; set; }

        public virtual ICollection<ResponseHeader> ResponseHeaders { get; set; }

        public virtual ICollection<ResponseError> ResponseErrors { get; set; }

        public ResponseMessage()
        {
            ResponseHeaders = new HashSet<ResponseHeader>();
            ResponseErrors = new HashSet<ResponseError>();
        }
    }
}