using System;

namespace Integration.DataAccess.Entitys
{
    public class BaseEntity<T>
    {
        public T Id { get; set; }
        public DateTime DateCreated { get; set; }
    }
}