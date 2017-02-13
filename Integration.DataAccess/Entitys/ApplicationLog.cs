namespace Integration.DataAccess.Entitys
{
    public class ApplicationLog : BaseEntity
    {
        public new int Id { get; set; }

        public string HostName { get; set; }

        public string Level { get; set; }

        public string Logger { get; set; }

        public string Message { get; set; }

        public string Method { get; set; }

        public string Thread { get; set; }

        public string Exception { get; set; }
    }
}