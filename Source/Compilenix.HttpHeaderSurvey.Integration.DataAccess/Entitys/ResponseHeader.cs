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
    public class ResponseHeader : BaseEntity
    {
        public string Key { get; set; }

        public string Value { get; set; }

        public string ValueHash { get; set; }

        public virtual ICollection<ResponseMessage> ResponseMessages { get; set; }

        public ResponseHeader()
        {
            ResponseMessages = new HashSet<ResponseMessage>();
        }
    }
}