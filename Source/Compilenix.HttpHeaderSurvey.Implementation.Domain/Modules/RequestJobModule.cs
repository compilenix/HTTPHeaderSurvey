using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Compilenix.HttpHeaderSurvey.Implementation.Shared;
using Compilenix.HttpHeaderSurvey.Implementation.Shared.IoC;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories;
using Compilenix.HttpHeaderSurvey.Integration.Domain.Modules;

namespace Compilenix.HttpHeaderSurvey.Implementation.Domain.Modules
{
    public class RequestJobModule : BaseModule<IRequestJobRepository, RequestJob>, IRequestJobModule
    {
        private BufferBlock<RequestJob> _inputBufferBlock;
        private int _itemsGot;
        private ActionBlock<RequestJob> _jobCompletedBlock;
        private IEnumerable<RequestJob> _jobs;
        private List<TransformBlock<RequestJob, RequestJob>> _processJobBlocks;

        public RequestJobModule(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task ImportFromCsv(string filePath, char delimiter = ',')
        {
            await ImportFromCsv(filePath, GetDefaultRequestHeaders(), delimiter);
        }

        public async Task ImportFromCsv(string filePath, IEnumerable<RequestHeader> requestHeaders, char delimiter = ',')
        {
            var jobsFromCsv = await new DataTransferObjectConverter().RequestJobsFromCsv(filePath, delimiter);

            Parallel.ForEach(
                jobsFromCsv,
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 },
                async job => { await ProcessBatchImport(job, requestHeaders); });

            GarbageCollectionUtils.CollectNow();
        }

        public async Task ProcessPendingJobs(int countOfJobsToProcess)
        {
            // TODO Remove temporary task
            var keepTaskRunning = true;
            var task = new Task(
                () =>
                    {
                        while (keepTaskRunning)
                        {
                            try
                            {
                                var processJobBlocksInputCount = string.Empty;
                                try
                                {
                                    var stringBuilder = new StringBuilder();
                                    foreach (var jobBlock in _processJobBlocks)
                                    {
                                        stringBuilder.Append(jobBlock.InputCount + ", ");
                                    }
                                    processJobBlocksInputCount = stringBuilder.ToString(0, stringBuilder.Length - 2);
                                }
                                catch (Exception exception)
                                {
                                    // ignored
                                }

                                Console.WriteLine($"_processJobBlocks.InputCount: {processJobBlocksInputCount}");
                                Console.WriteLine($"_jobCompletedBlock.InputCount: {_jobCompletedBlock?.InputCount}");
                                Console.WriteLine($"_jobs.Count remaining: {1000 - _itemsGot}");
                            }
                            catch
                            {
                                // ignored
                            }
                            Thread.Sleep(1000);
                        }
                    });

            var parallelism = Environment.ProcessorCount * 2;
            var blockOptions = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = parallelism, BoundedCapacity = parallelism };

            _inputBufferBlock = new BufferBlock<RequestJob>(new DataflowBlockOptions { BoundedCapacity = (parallelism * parallelism) << 2 });
            _processJobBlocks = new List<TransformBlock<RequestJob, RequestJob>>();
            for (var i = 0; i < Environment.ProcessorCount; i++)
            {
                _processJobBlocks.Add(new TransformBlock<RequestJob, RequestJob>(job => ProcessRequestJob(job), blockOptions));
            }
            _jobCompletedBlock = new ActionBlock<RequestJob>(CompleteRequestJob, blockOptions);

            foreach (var jobBlock in _processJobBlocks)
            {
                _inputBufferBlock.LinkTo(jobBlock);
                jobBlock.LinkTo(_jobCompletedBlock);

                _inputBufferBlock.Completion.ContinueWith(t => { jobBlock.Complete(); });
            }

            // TODO Remove temporary task
            task.Start();

            await FillConsumer(countOfJobsToProcess, _inputBufferBlock);

            await _inputBufferBlock.Completion;
            var processJobBlockTasks = _processJobBlocks.Select(x => x.Completion);
            Task.WaitAll(processJobBlockTasks.ToArray());
            _jobCompletedBlock.Complete();
            await _jobCompletedBlock.Completion;

            // TODO Remove temporary task
            keepTaskRunning = false;
            task.Wait();
            task.Dispose();
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
                    job = unit.Repository<IRequestJobRepository>().Get(job.Id);

                    if (job == null)
                    {
                        return;
                    }

                    job.IsCurrentlyScheduled = false;
                    job.LastTimeProcessed = DateTime.Now;

                    await SaveUnitOfWork(unit);

                    Console.WriteLine($"completed job -> {job.Uri}");
                }
            }
            catch (Exception exception)
            {
                this.Log()?.Error(string.Empty, exception);
            }
        }

        private async Task FillConsumer(int countOfJobsToProcess, ITargetBlock<RequestJob> targetBlock)
        {
            do
            {
                using (var unit = IoC.Resolve<IUnitOfWork>())
                {
                    var repo = unit.Repository<IRequestJobRepository>();

                    _itemsGot = 0;
                    var batchSize = countOfJobsToProcess < 1000 ? countOfJobsToProcess : 1000;
                    _jobs = repo.GetRequestJobsTodoAndNotScheduled(withRequestHeaders: true, count: batchSize, checkout: true);

                    foreach (var job in _jobs)
                    {
                        if ((targetBlock.Completion?.IsCanceled ?? true) || targetBlock.Completion.IsFaulted)
                        {
                            break;
                        }

                        _itemsGot++;
                        await targetBlock.SendAsync(job);
                    }

                    countOfJobsToProcess -= _itemsGot;
                }
            }
            while (countOfJobsToProcess > 0 && _itemsGot > 0);

            targetBlock.Complete();
        }

        private IEnumerable<RequestHeader> GetDefaultRequestHeaders()
        {
            var headers = new List<RequestHeader>();
            var headerRepository = UnitOfWork.Repository<IRequestHeaderRepository>();

            headers.Add(headerRepository.GetByHeader("User-Agent").First());
            headers.Add(headerRepository.GetByHeader("accept-encoding").First());
            headers.Add(headerRepository.GetByHeader("accept-language").First());
            headers.Add(headerRepository.GetByHeader("upgrade-insecure-requests").First());

            return headers;
        }

        private List<ResponseHeader> GetResponseHeadersFromList(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headerList, IUnitOfWork unit)
        {
            var headersFromResponse = new List<ResponseHeader>();
            var addedHeaders = new List<ResponseHeader>();
            var headers = new List<ResponseHeader>();

            foreach (var header in headerList)
            {
                // "fixing" bug in .net httpclient lib, where server values get split into multiple strings by whitespaces.
                var headerValues = new List<string>();
                if (header.Key.ToLower() == "server")
                {
                    headerValues.Add(string.Join(" ", header.Value));
                }
                else
                {
                    headerValues.AddRange(header.Value);
                }

                foreach (var headerValue in headerValues)
                {
                    var tmpHeader = new ResponseHeader { Key = header.Key, Value = headerValue };
                    headersFromResponse.Add(tmpHeader);
                    addedHeaders.Add(unit.Repository<IResponseHeaderRepository>().AddIfNotExisting(tmpHeader));
                }
            }

            addedHeaders = addedHeaders.Where(h => h != null).ToList();
            headersFromResponse.RemoveAll(header => addedHeaders.Contains(header));

            foreach (var responseHeader in headersFromResponse)
            {
                if (responseHeader != null)
                {
                    headers.Add(unit.Repository<IResponseHeaderRepository>().GetByHeaderAndValue(responseHeader.Key, responseHeader.Value));
                }
            }

            headers.AddRange(addedHeaders);
            headers = headers.Where(h => h != null).Distinct().ToList();
            return headers;
        }

        private async Task ProcessBatchImport(RequestJob requestJob, IEnumerable<RequestHeader> headers)
        {
            using (var unit = IoC.Resolve<IUnitOfWork>())
            {
                var headerRepository = unit.Repository<IRequestHeaderRepository>();

                var headersToAdd = new List<RequestHeader>();
                foreach (var header in headers)
                {
                    headersToAdd.Add(headerRepository.AddIfNotExisting(header));
                }
                headersToAdd = headersToAdd.Where(x => x != null).ToList();

                requestJob.Headers = headersToAdd;
                unit.Repository<IRequestJobRepository>().AddIfNotExisting(requestJob);

                await SaveUnitOfWork(unit);
            }
        }

        private async Task<RequestJob> ProcessRequestJob(RequestJob requestJob)
        {
            HttpResponseMessage jobResult = null;
            try
            {
                using (var unit = IoC.Resolve<IUnitOfWork>())
                {
                    requestJob = unit.Repository<IRequestJobRepository>().GetWithRequestHeaders(requestJob.Id);

                    var httpClientOptions = new HttpClientRequestOptions
                        {
                            Headers = requestJob.Headers,
                            HttpVersion = requestJob.HttpVersion,
                            Method = requestJob.Method,
                            Uri = new Uri(requestJob.Uri),
                            HeadersOnly = true,
                            CancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(60)).Token
                        };
                    HttpClientUtils.DefaultTimeout = TimeSpan.FromSeconds(60);

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

                    var headers = GetResponseHeadersFromList(jobResult.Headers, unit);
                    headers.AddRange(GetResponseHeadersFromList(jobResult.Content.Headers, unit));

                    var message =
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
                        await SaveUnitOfWork(unit);
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