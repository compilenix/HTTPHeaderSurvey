using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys
{
    [DebuggerStepThrough]
    [SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
    public class RequestJob : BaseEntity
    {
        public string Method { get; set; }

        public string Uri { get; set; }

        public string UriHash { get; set; }

        public string HttpVersion { get; set; }

        public bool IsCurrentlyScheduled { get; set; }

        public bool IsRunOnce { get; set; }

        public DateTime LastTimeProcessed { get; set; }

        public virtual ICollection<RequestHeader> Headers { get; set; }

        public virtual ICollection<ResponseMessage> ResponseMessages { get; set; }

        public RequestJob()
        {
            Headers = new HashSet<RequestHeader>();
            ResponseMessages = new HashSet<ResponseMessage>();
        }
    }
}