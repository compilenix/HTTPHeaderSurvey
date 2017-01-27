﻿using System.Collections.Generic;

namespace Integration.DataAccess.Entitys
{
    public class RequestHeader : BaseEntity<int>
    {
        public string Key { get; set; }

        public string Value { get; set; }

        public string ValueHash { get; set; }

        public ICollection<RequestJob> RequestJobs { get; set; }

        public RequestHeader()
        {
            RequestJobs = new HashSet<RequestJob>();
        }
    }
}