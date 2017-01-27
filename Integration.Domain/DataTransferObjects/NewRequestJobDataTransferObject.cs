namespace Integration.Domain.DataTransferObjects
{
    public class NewRequestJobDataTransferObject
    {
        public string Method { get; set; }

        public string Uri { get; set; }

        public string HttpVersion { get; set; }

        public bool IsRunOnce { get; set; }
    }
}