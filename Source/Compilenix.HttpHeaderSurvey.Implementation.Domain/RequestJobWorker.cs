﻿using System;
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
using JetBrains.Annotations;

namespace Compilenix.HttpHeaderSurvey.Implementation.Domain
{
    [UsedImplicitly]
    public class RequestJobWorker : IRequestJobWorker
    {
        private readonly TimeSpan _httpClientTimeout;
        private CancellationTokenSource _cancellationTokenSource;
        private BufferBlock<RequestJob> _inputBufferBlock;
        private ActionBlock<Tuple<RequestJob, bool>> _jobCompletedBlock;
        private List<TransformBlock<RequestJob, Tuple<RequestJob, bool>>> _processJobBlocks;

        // ReSharper disable once AssignNullToNotNullAttribute
        public Task Completion => _jobCompletedBlock?.Completion;

        // TODO add IUnitOfWork
        public RequestJobWorker()
        {
            _httpClientTimeout = TimeSpan.FromSeconds(int.Parse(IoC.Resolve<IApplicationConfigurationCollection>().Get("HttpClientTimeoutSeconds") ?? "60"));
            HttpClientUtils.DefaultTimeout = _httpClientTimeout;
        }

        private static async Task FillConsumerAsync(int countOfJobsToProcess, [NotNull] ITargetBlock<RequestJob> targetBlock, CancellationToken token)
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
                        if ((targetBlock.Completion?.IsCanceled ?? true) || targetBlock.Completion.IsFaulted || token.IsCancellationRequested)
                        {
                            break;
                        }

                        itemsGot++;

                        // ReSharper disable once MethodSupportsCancellation
                        var sendAsync = targetBlock.SendAsync(job);
                        if (sendAsync != null)
                        {
                            await sendAsync;
                        }
                    }

                    countOfJobsToProcess -= itemsGot;
                }
            }
            while (countOfJobsToProcess > 0 && itemsGot > 0);

            targetBlock.Complete();
        }

        public async Task StopAsync()
        {
            _cancellationTokenSource?.Cancel();
            if (_jobCompletedBlock?.Completion != null)
            {
                await _jobCompletedBlock?.Completion;
            }
        }

        public async Task StartAsync(int countOfJobsToProcess)
        {
            if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource = new CancellationTokenSource();
                InitDataflow();

                // ReSharper disable once PossibleNullReferenceException
                await ProcessPendingJobs(countOfJobsToProcess, _cancellationTokenSource.Token);
            }
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            StopAsync().Wait();
            _cancellationTokenSource?.Dispose();
        }

        private async Task CompleteRequestJob(Tuple<RequestJob, bool> data)
        {
            if (data == null)
            {
                return;
            }

            var job = data.Item1;
            var saveCompleted = data.Item2;

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

                    if (saveCompleted)
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
                    MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded,
                    BoundedCapacity = 3
                };

            _inputBufferBlock = new BufferBlock<RequestJob>(new DataflowBlockOptions { BoundedCapacity = (Environment.ProcessorCount * Environment.ProcessorCount) << 3 });
            _processJobBlocks = new List<TransformBlock<RequestJob, Tuple<RequestJob, bool>>>();
            for (var i = 0; i < Environment.ProcessorCount << 3; i++)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                _processJobBlocks.Add(new TransformBlock<RequestJob, Tuple<RequestJob, bool>>(job => ProcessRequestJob(job), blockOptions));
            }
            _jobCompletedBlock = new ActionBlock<Tuple<RequestJob, bool>>(CompleteRequestJob, blockOptions);

            foreach (var jobBlock in _processJobBlocks)
            {
                _inputBufferBlock.LinkTo(jobBlock);
                jobBlock.LinkTo(_jobCompletedBlock);

                // ReSharper disable once MethodSupportsCancellation
                _inputBufferBlock.Completion?.ContinueWith(t => { jobBlock.Complete(); });
            }
        }

        private async Task ProcessPendingJobs(int countOfJobsToProcess, CancellationToken token)
        {
            if (_inputBufferBlock != null)
            {
                await FillConsumerAsync(countOfJobsToProcess, _inputBufferBlock, token);

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

        private async Task<Tuple<RequestJob, bool>> ProcessRequestJob([NotNull] RequestJob requestJob)
        {
            HttpResponseMessage jobResult = null;
            var saveCompleted = true;
            try
            {
                using (var unit = IoC.Resolve<IUnitOfWork>())
                {
                    requestJob = await unit.Resolve<IRequestJobRepository>().GetWithRequestHeadersAsync(requestJob.Id);

                    if (requestJob?.Uri == null)
                    {
                        return new Tuple<RequestJob, bool>(null, false);
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
                        if (await unit.Resolve<IResponseErrorModule>().AddAsync(responseMessage, exception, unit))
                        {
                            unit.Resolve<IResponseMessageRepository>().Add(responseMessage);
                            // TODO remove
                            this.Log().Debug($"Known Error on: {requestJob.Uri}");
                            return new Tuple<RequestJob, bool>(requestJob, true);
                        }

                        // TODO give admin possibillity to add error to list of known errors
                        this.Log().Error($"Error on: {requestJob.Uri}", exception);
                        return new Tuple<RequestJob, bool>(requestJob, true);
                    }

                    if (jobResult.Headers == null)
                    {
                        throw new AggregateException("http result has no response headers");
                    }

                    if (jobResult.Content?.Headers == null)
                    {
                        throw new AggregateException("http result has no response content headers");
                    }

                    var responseHeaderModule = unit.Resolve<IResponseHeaderModule>();
                    var headers = await responseHeaderModule.GetResponseHeadersFromListAsync(jobResult.Headers, unit);
                    headers.AddRange(await responseHeaderModule.GetResponseHeadersFromListAsync(jobResult.Content.Headers, unit));

                    responseMessage.ResponseHeaders = headers;
                    responseMessage.ProtocolVersion = jobResult.Version?.ToString();
                    responseMessage.StatusCode = (int)jobResult.StatusCode;

                    unit.Resolve<IResponseMessageRepository>().Add(responseMessage);

                    try
                    {
                        await unit.CompleteAsync();
                    }
                    catch (Exception exception)
                    {
                        saveCompleted = false;
                        if (!exception.GetAllMessages().Any(e => e?.Contains($"Cannot insert duplicate key row in object 'dbo.{nameof(ResponseHeader)}s'") ?? false))
                        {
                            throw;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                saveCompleted = false;
                this.Log().Fatal(string.Empty, exception);
            }
            finally
            {
                jobResult?.Dispose();
            }

            return new Tuple<RequestJob, bool>(requestJob, saveCompleted);
        }
    }
}