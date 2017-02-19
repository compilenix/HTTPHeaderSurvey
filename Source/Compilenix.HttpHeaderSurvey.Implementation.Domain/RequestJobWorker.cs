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
        private readonly TimeSpan _httpClientTimeout;
        private CancellationTokenSource _cancellationTokenSource;
        private BufferBlock<RequestJob> _inputBufferBlock;
        private ActionBlock<RequestJob> _jobCompletedBlock;
        private List<TransformBlock<RequestJob, RequestJob>> _processJobBlocks;

        public Task Completion => _jobCompletedBlock?.Completion;

        public RequestJobWorker()
        {
            _httpClientTimeout = TimeSpan.FromSeconds(int.Parse(IoC.Resolve<IApplicationConfigurationCollection>().Get("HttpClientTimeoutSeconds")));
            HttpClientUtils.DefaultTimeout = _httpClientTimeout;
        }

        private static async Task FillConsumer(int countOfJobsToProcess, ITargetBlock<RequestJob> targetBlock, CancellationToken token)
        {
            int itemsGot;
            do
            {
                using (var unit = IoC.Resolve<IUnitOfWork>())
                {
                    var repo = unit.Repository<IRequestJobRepository>();

                    itemsGot = 0;
                    var batchSize = countOfJobsToProcess < 1000 ? countOfJobsToProcess : 1000;
                    var _jobs = repo.GetRequestJobsTodoAndNotScheduled(withRequestHeaders: true, count: batchSize, checkout: true);

                    foreach (var job in _jobs)
                    {
                        if ((targetBlock.Completion?.IsCanceled ?? true) || targetBlock.Completion.IsFaulted || token.IsCancellationRequested)
                        {
                            break;
                        }

                        itemsGot++;
                        await targetBlock.SendAsync(job);
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
            await _jobCompletedBlock?.Completion;
        }

        public async Task StartAsync(int countOfJobsToProcess)
        {
            if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource = new CancellationTokenSource();
                InitDataflow();
                await ProcessPendingJobs(countOfJobsToProcess, _cancellationTokenSource.Token);
            }
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            StopAsync().Wait();
            _cancellationTokenSource?.Dispose();
        }

        private async Task CompleteRequestJob(RequestJob job)
        {
            if (job == null)
            {
                return;
            }

            try
            {
                using (var unit = IoC.Resolve<IUnitOfWork>())
                {
                    job = await unit.Repository<IRequestJobRepository>().GetAsync(job.Id);

                    if (job == null)
                    {
                        return;
                    }

                    job.IsCurrentlyScheduled = false;
                    job.LastTimeProcessed = DateTime.Now;

                    await unit.CompleteAsync();

                    // TODO remove
                    Console.WriteLine($"completed job -> {job.Uri}");
                }
            }
            catch (Exception exception)
            {
                this.Log()?.Error(string.Empty, exception);
            }
        }

        private void InitDataflow()
        {
            var parallelism = Environment.ProcessorCount * 2;
            var blockOptions = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = parallelism, BoundedCapacity = parallelism };

            _inputBufferBlock = new BufferBlock<RequestJob>(new DataflowBlockOptions { BoundedCapacity = (parallelism * parallelism) << 4 });
            _processJobBlocks = new List<TransformBlock<RequestJob, RequestJob>>();
            for (var i = 0; i < Environment.ProcessorCount << 4; i++)
            {
                _processJobBlocks.Add(new TransformBlock<RequestJob, RequestJob>(job => ProcessRequestJob(job), blockOptions));
            }
            _jobCompletedBlock = new ActionBlock<RequestJob>(CompleteRequestJob, blockOptions);

            foreach (var jobBlock in _processJobBlocks)
            {
                _inputBufferBlock.LinkTo(jobBlock);
                jobBlock.LinkTo(_jobCompletedBlock);

                // ReSharper disable once MethodSupportsCancellation
                _inputBufferBlock.Completion.ContinueWith(t => { jobBlock.Complete(); });
            }
        }

        private async Task ProcessPendingJobs(int countOfJobsToProcess, CancellationToken token)
        {
            await FillConsumer(countOfJobsToProcess, _inputBufferBlock, token);

            await _inputBufferBlock.Completion;
            var processJobBlockTasks = _processJobBlocks.Select(x => x.Completion);
            Task.WaitAll(processJobBlockTasks.ToArray());
            _jobCompletedBlock.Complete();
            await _jobCompletedBlock.Completion;
        }

        private async Task<RequestJob> ProcessRequestJob(RequestJob requestJob)
        {
            HttpResponseMessage jobResult = null;
            try
            {
                using (var unit = IoC.Resolve<IUnitOfWork>())
                {
                    requestJob = await unit.Repository<IRequestJobRepository>().GetWithRequestHeadersAsync(requestJob.Id);

                    var httpClientOptions = new HttpClientRequestOptions
                        {
                            Headers = requestJob.Headers,
                            HttpVersion = requestJob.HttpVersion,
                            Method = requestJob.Method,
                            Uri = new Uri(requestJob.Uri),
                            HeadersOnly = true,
                            CancellationToken = new CancellationTokenSource(_httpClientTimeout).Token
                        };

                    try
                    {
                        // TODO Read header as stream
                        jobResult = await HttpClientUtils.InvokeWebRequestAsync(httpClientOptions);
                    }
                    catch (Exception exception)
                    {
                        // TODO extend response message to include reference of exception
                        //if (exception.GetAllMessages().Any(m => m.Contains("The remote name could not be resolved"))
                        //    || exception.GetAllMessages()
                        //        .Any(
                        //            m =>
                        //                m.Contains(
                        //                    "A connection attempt failed because the connected party did not properly respond after a period of time"))
                        //    || exception.GetAllMessages()
                        //        .Any(m => m.Contains("No connection could be made because the target machine actively refused"))
                        //    || exception.GetAllMessages().Any(m => m.Contains("was forcibly closed by the remote host"))
                        //    || exception.GetAllMessages().Any(m => m.Contains("The connection was closed unexpectedly"))
                        //    || exception.GetAllMessages().Any(m => m.Contains("Unable to read data from the transport connection")))
                        //{
                        //    this.Log()?.Error($"Error on: {requestJob.Uri}", exception);
                        //    return requestJob;
                        //}

                        this.Log()?.Error($"Error on: {requestJob.Uri}", exception);
                        return requestJob;
                    }

                    List<ResponseHeader> headers;
                    using (var module = IoC.Resolve<IResponseHeaderModule>())
                    {
                        headers = await module.GetResponseHeadersFromListAsync(jobResult.Headers, unit);
                        headers.AddRange(await module.GetResponseHeadersFromListAsync(jobResult.Content.Headers, unit));
                    }

                    unit.Repository<IResponseMessageRepository>()
                        .Add(
                            new ResponseMessage
                                {
                                    RequestJob = requestJob,
                                    ResponseHeaders = headers,
                                    ProtocolVersion = jobResult.Version.ToString(),
                                    StatusCode = (int)jobResult.StatusCode
                                });

                    try
                    {
                        await unit.CompleteAsync();
                    }
                    catch (Exception exception)
                    {
                        if (
                            !exception.GetAllMessages()?
                                .Any(e => e?.Contains($"Cannot insert duplicate key row in object 'dbo.{nameof(ResponseHeader)}s'") ?? true) ?? true)
                        {
                            throw;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                this.Log()?.Fatal(string.Empty, exception);
            }
            finally
            {
                jobResult?.Dispose();
            }

            return requestJob;
        }
    }
}