using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Compilenix.HttpHeaderSurvey.Implementation.Shared;
using Compilenix.HttpHeaderSurvey.Implementation.Shared.IoC;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories;
using Compilenix.HttpHeaderSurvey.Integration.Domain;
using Compilenix.HttpHeaderSurvey.Integration.Domain.Modules;

namespace Compilenix.HttpHeaderSurvey.Implementation.Domain
{
    public class RequestJobWorker : IRequestJobWorker
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly TimeSpan _httpClientTimeout;
        private BufferBlock<RequestJob> _inputBufferBlock;
        private bool _isThrottling;
        private ActionBlock<(RequestJob job, bool isCompleted)> _jobCompletedBlock;
        private List<TransformBlock<RequestJob, (RequestJob job, bool isCompleted)>> _processJobBlocks;

        // ReSharper disable once AssignNullToNotNullAttribute
        public Task Completion => _jobCompletedBlock?.Completion;

        public bool IsThrottling
        {
            get => _isThrottling;
            set
            {
                _isThrottling = value;

                // ReSharper disable once InvertIf
                if (value)
                {
                    InitThrottlingTask(IoC.Resolve<IApplicationConfigurationCollection>());

                    if (!IsThrottlingTaskStarted)
                    {
                        IsThrottlingTaskStarted = true;
                        ThrottlingTask?.Start();
                    }
                }
            }
        }

        public uint ThrottlingItemsPerSecond { get; set; }

        public uint CurrentItemsPerSecond { get; private set; }

        private Task ThrottlingTask { get; set; }

        private bool IsThrottlingTaskStarted { get; set; }

        public RequestJobWorker()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            IsThrottlingTaskStarted = false;

            var config = IoC.Resolve<IApplicationConfigurationCollection>();
            _httpClientTimeout = TimeSpan.FromSeconds(int.Parse(config.Get("HttpClientTimeoutSeconds") ?? "60"));
            HttpClientUtils.DefaultTimeout = _httpClientTimeout;

            IsThrottling = bool.Parse(config.Get("RequestJobWorkerThrottlingEnabled") ?? "False");
        }

        private static async Task FillConsumerAsync(int countOfJobsToProcess, ITargetBlock<RequestJob> targetBlock, CancellationToken token)
        {
            int itemsGot;
            do
            {
                using (var unit = IoC.Resolve<IUnitOfWork>())
                {
                    var repo = unit.Resolve<IRequestJobRepository>();

                    itemsGot = 0;
                    var batchSize = countOfJobsToProcess < 1000 ? countOfJobsToProcess : 1000;
                    var jobs = repo.GetRequestJobsTodoAndNotScheduled(withRequestHeaders: true, count: batchSize, checkout: true);

                    foreach (var job in jobs)
                    {
                        if ((targetBlock.Completion?.IsCanceled ?? true) || targetBlock.Completion.IsFaulted || token.IsCancellationRequested) break;

                        itemsGot++;

                        await targetBlock.SendAsync(job, token);
                    }

                    countOfJobsToProcess -= itemsGot;
                }
            }
            while (countOfJobsToProcess > 0 && itemsGot > 0);

            targetBlock.Complete();
        }

        private static async Task ProcessHttpResponseAsync(HttpResponseMessage jobResult, IUnitOfWork unit, ResponseMessage responseMessage)
        {
            if (jobResult.Headers == null) throw new AggregateException("http result has no response headers");
            if (jobResult.Content?.Headers == null) throw new AggregateException("http result has no response content headers");

            var responseHeaderModule = unit.Resolve<IResponseHeaderModule>();
            var headers = await responseHeaderModule.GetResponseHeadersFromListAsync(jobResult.Headers, unit);
            headers.AddRange(await responseHeaderModule.GetResponseHeadersFromListAsync(jobResult.Content.Headers, unit));

            responseMessage.ResponseHeaders = headers;
            responseMessage.ProtocolVersion = jobResult.Version?.ToString();
            responseMessage.StatusCode = (int)jobResult.StatusCode;
        }

        public async Task StopAsync()
        {
            _cancellationTokenSource?.Cancel();

            if (_jobCompletedBlock?.Completion != null)
                await _jobCompletedBlock?.Completion;

            if (IsThrottling && ThrottlingTask != null)
                await ThrottlingTask;
        }

        public async Task StartAsync(int countOfJobsToProcess)
        {
            if (!_cancellationTokenSource?.IsCancellationRequested ?? false)
            {
                InitDataflow();
                await ProcessPendingJobs(countOfJobsToProcess);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            StopAsync().Wait();
            _cancellationTokenSource?.Dispose();
        }

        private async Task CompleteRequestJobAsync((RequestJob job, bool isCompleted) data)
        {
            var job = data.job;

            if (job == null)
            {
                return;
            }

            try
            {
                using (var unit = IoC.Resolve<IUnitOfWork>())
                {
                    job = await unit.Resolve<IRequestJobRepository>().GetAsync(job.Id);

                    if (job == null)
                    {
                        return;
                    }

                    job.IsCurrentlyScheduled = false;

                    if (data.isCompleted)
                    {
                        job.LastTimeProcessed = DateTime.Now;
                    }

                    await unit.CompleteAsync();

                    // TODO remove
                    // ReSharper disable once LocalizableElement
                    Console.WriteLine($"completed job -> {job.Uri}");
                }
            }
            catch (Exception exception)
            {
                this.Log().Error(string.Empty, exception);
            }
        }

        private void InitDataflow()
        {
            var blockOptions = new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount,
                    BoundedCapacity = 1,
                    TaskScheduler = new ConcurrentExclusiveSchedulerPair().ConcurrentScheduler
                };

            _inputBufferBlock = new BufferBlock<RequestJob>(new DataflowBlockOptions { BoundedCapacity = 500 });
            _processJobBlocks = new List<TransformBlock<RequestJob, (RequestJob job, bool isCompleted)>>();
            for (var i = 0; i < Environment.ProcessorCount << 3; i++)
            {
                _processJobBlocks.Add(new TransformBlock<RequestJob, (RequestJob job, bool isCompleted)>((Func<RequestJob, Task<(RequestJob job, bool isCompleted)>>)ProcessRequestJobAsync, blockOptions));
            }
            _jobCompletedBlock = new ActionBlock<(RequestJob job, bool isCompleted)>(CompleteRequestJobAsync, blockOptions);

            foreach (var jobBlock in _processJobBlocks)
            {
                _inputBufferBlock.LinkTo(jobBlock);
                jobBlock.LinkTo(_jobCompletedBlock);

                _inputBufferBlock.Completion?.ContinueWith(t => { jobBlock.Complete(); });
            }
        }

        private void InitThrottlingTask(IApplicationConfigurationCollection config)
        {
            ThrottlingItemsPerSecond = uint.Parse(config.Get("RequestJobWorkerThrottlingItemsPerSecond") ?? "10");

            if (ThrottlingItemsPerSecond == 0)
                ThrottlingItemsPerSecond = 10;

            CurrentItemsPerSecond = 0;

            void DecreaseItemsPerSecond()
            {
                if (CurrentItemsPerSecond != 0)
                {
                    if (CurrentItemsPerSecond < ThrottlingItemsPerSecond)
                        CurrentItemsPerSecond = 0;
                    else
                        CurrentItemsPerSecond -= ThrottlingItemsPerSecond;
                }

                Thread.Sleep(1_000);
            }

            void ItemsPerSecondAction()
            {
                while (!(_cancellationTokenSource?.IsCancellationRequested ?? true))
                {
                    CurrentItemsPerSecond.ThreadSafeAction(DecreaseItemsPerSecond);
                }
            }

            if (ThrottlingTask == null) ThrottlingTask = new Task(ItemsPerSecondAction);
        }

        private async Task ProcessPendingJobs(int countOfJobsToProcess)
        {
            if (_inputBufferBlock != null)
            {
                await FillConsumerAsync(countOfJobsToProcess, _inputBufferBlock, _cancellationTokenSource.Token);

                if (_inputBufferBlock?.Completion != null)
                {
                    await _inputBufferBlock?.Completion;
                }
            }

            var processJobBlockTasks = _processJobBlocks?.Select(x => x?.Completion);
            if (processJobBlockTasks != null)
            {
                Task.WaitAll(processJobBlockTasks.ToArray());
            }

            _jobCompletedBlock?.Complete();
            if (_jobCompletedBlock?.Completion != null)
            {
                await _jobCompletedBlock?.Completion;
            }
        }

        private async Task<(RequestJob job, bool isCompleted)> ProcessRequestJobAsync(RequestJob requestJob)
        {
            if (IsThrottling)
            {
                while (CurrentItemsPerSecond >= ThrottlingItemsPerSecond)
                {
                    Thread.Sleep(50);
                }

                CurrentItemsPerSecond.ThreadSafeAction(() => CurrentItemsPerSecond++);
            }

            HttpResponseMessage jobResult = null;
            var isCompleted = true;
            try
            {
                using (var unit = IoC.Resolve<IUnitOfWork>())
                {
                    unit.SaveChanges = false;
                    requestJob = await unit.Resolve<IRequestJobRepository>().GetWithRequestHeadersAsync(requestJob.Id);

                    if (requestJob?.Uri == null)
                    {
                        return (job: null, isCompleted: false);
                    }

                    var httpClientOptions = new HttpClientRequestOptions
                        {
                            Headers = requestJob.Headers,
                            HttpVersion = requestJob.HttpVersion,
                            Method = requestJob.Method,
                            Uri = new Uri(requestJob.Uri),
                            HeadersOnly = true,
                            CancellationToken = new CancellationTokenSource(_httpClientTimeout).Token
                        };
                    var responseMessage = new ResponseMessage { RequestJob = requestJob };

                    try
                    {
                        jobResult = await HttpClientUtils.InvokeWebRequestAsync(httpClientOptions);
                    }
                    catch (Exception exception)
                    {
                        var handledError = await unit.Resolve<IResponseErrorModule>().ProcessAsync(responseMessage, exception, unit);

                        isCompleted = !handledError.isPermanentError;

                        if (handledError.isKnownError)
                        {
                            unit.Resolve<IResponseMessageRepository>().Add(responseMessage);
                            // TODO remove
                            this.Log().Debug($"Known Error on: {requestJob.Uri}");
                            return (requestJob, isCompleted);
                        }

                        // TODO give admin possibillity to add error to list of known errors
                        this.Log().Error($"Error on: {requestJob.Uri}", exception);
                        return (requestJob, isCompleted);
                    }

                    await ProcessHttpResponseAsync(jobResult, unit, responseMessage);

                    unit.Resolve<IResponseMessageRepository>().Add(responseMessage);

                    try
                    {
                        await unit.CompleteAsync();
                    }
                    catch (Exception exception)
                    {
                        isCompleted = false;
                        if (!exception.GetAllMessages().Any(e => e?.Contains("Cannot insert duplicate key row in object") ?? false))
                        {
                            throw;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                isCompleted = false;
                this.Log().Fatal(string.Empty, exception);
            }
            finally
            {
                jobResult?.Dispose();
            }

            return (requestJob, isCompleted);
        }
    }
}