using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Compilenix.HttpHeaderSurvey.Integration.Domain
{
    [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
    public interface IRequestJobWorker : IDisposable
    {
        [NotNull]
        Task Completion { get; }

        [NotNull]
        Task StartAsync(int countOfJobsToProcess);

        [NotNull]
        Task StopAsync();
    }
}