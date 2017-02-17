﻿using System;
using System.Collections.Generic;

namespace Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys
{
    public class RequestJob : BaseEntity
    {
        public string Method { get; set; }

        public string Uri { get; set; }

        public string UriHash { get; set; }

        public string HttpVersion { get; set; }

        public bool IsCurrentlyScheduled { get; set; }

        public bool IsRunOnce { get; set; }

        public DateTime LastTimeProcessed { get; set; }

        public ICollection<RequestHeader> Headers { get; set; }

        public ICollection<ResponseMessage> ResponseMessages { get; set; }

        public RequestJob()
        {
            Headers = new HashSet<RequestHeader>();
            ResponseMessages = new HashSet<ResponseMessage>();
        }
    }
}