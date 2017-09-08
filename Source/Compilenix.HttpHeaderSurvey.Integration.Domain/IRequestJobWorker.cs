using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Compilenix.HttpHeaderSurvey.Integration.Domain
{
    [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
    public interface IRequestJobWorker : IDisposable
    {
        
        Task Completion { get; }

        bool IsThrottling { get; set; }

        uint ThrottlingItemsPerSecond { get; set; }

        uint CurrentItemsPerSecond { get; }

        
        Task StartAsync(int countOfJobsToProcess);

        
        Task StopAsync();
    }
}