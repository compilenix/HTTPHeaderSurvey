using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;

namespace Compilenix.HttpHeaderSurvey.Implementation.Shared
{
    [DebuggerStepThrough]
    public class HttpClientRequestOptions
    {
        public bool HeadersOnly { get; set; }
        public Uri Uri { get; set; }
        public string Method { get; set; }
        public string HttpVersion { get; set; }
        public ICollection<RequestHeader> Headers { get; set; }
        public CancellationToken CancellationToken { get; set; }

        public HttpClientRequestOptions()
        {
            Method = string.Empty;
            HttpVersion = string.Empty;
            Headers = new List<RequestHeader>();
        }
    }
}