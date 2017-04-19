using System.Diagnostics.CodeAnalysis;

namespace Compilenix.HttpHeaderSurvey.Integration.Domain.DataTransferObjects
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class NewRequestJobDto
    {
        public string Method { get; set; }

        public string Uri { get; set; }

        public string HttpVersion { get; set; }

        public bool IsRunOnce { get; set; }
    }
}