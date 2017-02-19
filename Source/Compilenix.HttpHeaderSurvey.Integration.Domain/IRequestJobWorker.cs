using System;
using System.Threading.Tasks;

namespace Compilenix.HttpHeaderSurvey.Integration.Domain
{
    public interface IRequestJobWorker : IDisposable
    {
        Task Completion { get; }
        Task StartAsync(int countOfJobsToProcess);
        Task StopAsync();
    }
}