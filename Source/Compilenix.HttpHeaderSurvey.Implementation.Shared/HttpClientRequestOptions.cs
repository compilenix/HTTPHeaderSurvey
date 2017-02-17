using System;
using System.Collections.Generic;
using System.Threading;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;

namespace Compilenix.HttpHeaderSurvey.Implementation.Shared
{
    public class HttpClientRequestOptions
    {
        public bool HeadersOnly = false;
        public Uri Uri { get; set; }
        public string Method { get; set; }
        public string HttpVersion { get; set; }
        public ICollection<RequestHeader> Headers { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}