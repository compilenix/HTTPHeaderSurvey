using System;
using System.Collections.Generic;
using System.Threading;
using Integration.DataAccess.Entitys;

namespace Implementation.Shared
{
    public class HttpClientRequestOptions
    {
        public Uri Uri { get; set; }
        public string Method { get; set; }
        public string HttpVersion { get; set; }
        public ICollection<RequestHeader> Headers { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public bool HeadersOnly = false;
    }
}
