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
    public class ErrorCode : BaseEntity
    {
        public int Code { get; set; }

        public string Message { get; set; }

        public virtual ICollection<ResponseError> ResponseErrors { get; set; }

        public ErrorCode()
        {
            ResponseErrors = new HashSet<ResponseError>();
        }
    }
}