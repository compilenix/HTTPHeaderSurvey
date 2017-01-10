﻿using System.Collections.Generic;

namespace Integration.DataAccess.Entitys
{
    public class ResponseHeader : BaseEntity
    {
        public int Id { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }

        public string ValueHash { get; set; }

        public ICollection<ResponseMessage> ResponseMessages { get; set; }

        public ResponseHeader()
        {
            ResponseMessages = new HashSet<ResponseMessage>();
        }
    }
}