using System.Collections.Generic;

namespace Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys
{
    public class ResponseMessage : BaseEntity
    {
        public int StatusCode { get; set; }

        public string ProtocolVersion { get; set; }

        public RequestJob RequestJob { get; set; }

        public ICollection<ResponseHeader> ResponseHeaders { get; set; }

        public ResponseMessage()
        {
            ResponseHeaders = new HashSet<ResponseHeader>();
        }
    }
}