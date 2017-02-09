using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Implementation.Shared;
using Implementation.Shared.IoC;
using Integration.DataAccess;
using Integration.DataAccess.Entitys;
using Integration.DataAccess.Repositories;
using Integration.Domain.Modules;

namespace Implementation.Domain.Modules
{
    public class RequestJobModule : BaseModule<IRequestJobRepository, RequestJob>, IRequestJobModule
    {
        private TransformBlock<RequestJob, RequestJob> _processJobBlock;
        private ActionBlock<RequestJob> _jobCompletedBlock;
        private IEnumerable<RequestJob> _jobs;
        private int _itemsGot;

        public RequestJobModule(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task ImportFromCsv(string filePath, char delimiter = ',')
        {
            await ImportFromCsv(filePath, GetDefaultRequestHeaders(), delimiter);
        }

        public async Task ImportFromCsv(string filePath, IEnumerable<RequestHeader> requestHeaders, char delimiter = ',')
        {
            var importBlock = new ActionBlock<RequestJob>(
                job => ProcessBatchImport(job, requestHeaders),
                new ExecutionDataflowBlockOptions
                    {
                        BoundedCapacity = Environment.ProcessorCount * 2,
                        MaxDegreeOfParallelism = Environment.ProcessorCount
                    });
            var jobsFromCsv = new DataTransferObjectConverter().RequestJobsFromCsv(filePath, delimiter);

            foreach (var job in jobsFromCsv)
            {
                await importBlock.SendAsync(job);
            }

            importBlock.Complete();
            await importBlock.Completion;
        }

        public async Task ProcessPendingJobs(int countOfJobsToProcess, int maxDegreeOfParallelism)
        {
            // TODO cleanup and creating multiple dataflow blocks which get's fed by a bufferblock
            var task = new Task(
                () =>
                    {
                        while (true)
                        {
                            try
                            {
                                Console.WriteLine($"_processJobBlock.InputCount: {_processJobBlock?.InputCount}");
                                Console.WriteLine($"_jobCompletedBlock.InputCount: {_jobCompletedBlock?.InputCount}");
                                Console.WriteLine($"_jobs.Count remaining: {1000 - _itemsGot}");
                            }
                            catch
                            {
                                // ignored
                            }
                            Thread.Sleep(100);
                        }
                    });
            var blockOptions = new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = maxDegreeOfParallelism,
                    BoundedCapacity = maxDegreeOfParallelism * 2
            };
            _jobCompletedBlock = new ActionBlock<RequestJob>(CompleteRequestJob, blockOptions);
            _processJobBlock = new TransformBlock<RequestJob, RequestJob>(job => ProcessRequestJob(job), blockOptions);
//            var getJobBlock = new BufferBlock<RequestJob>(new DataflowBlockOptions { BoundedCapacity = maxDegreeOfParallelism });

//            getJobBlock.LinkTo(processJobBlock);
            _processJobBlock.LinkTo(_jobCompletedBlock);

//            getJobBlock.Completion.ContinueWith(t => { processJobBlock.Complete(); });
            _processJobBlock.Completion.ContinueWith(t => { _jobCompletedBlock.Complete(); });

            task.Start();

            await FillConsumer(countOfJobsToProcess, _processJobBlock);

//            await getJobBlock.Completion;
            await _processJobBlock.Completion;
            await _jobCompletedBlock.Completion;

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

        private async Task FillConsumer(int countOfJobsToProcess, ITargetBlock<RequestJob> processJobBlock)
        {
            using (var unit = IoC.Resolve<IUnitOfWork>())
            {
                var repo = unit.Repository<IRequestJobRepository>();

                do
                {
                    _itemsGot = 0;
                    var batchSize = countOfJobsToProcess < 1000 ? countOfJobsToProcess : 1000;
                    _jobs = repo.GetRequestJobsTodoAndNotScheduled(withRequestHeaders: true, count: batchSize, checkout: true);

                    foreach (var job in _jobs)
                    {
                        if ((processJobBlock.Completion?.IsCanceled ?? true) || processJobBlock.Completion.IsFaulted)
                        {
                            break;
                        }

                        _itemsGot++;
                        await processJobBlock.SendAsync(job);
                    }

                    countOfJobsToProcess -= _itemsGot;
                }
                while (countOfJobsToProcess > 0 && _itemsGot > 0);
            }

            processJobBlock.Complete();
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
                            CancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token
                        };

                    try
                    {
                        jobResult = await HttpClientUtils.InvokeWebRequestAsync(httpClientOptions);
                    }
                    catch (Exception exception)
                    {
                        if (exception.GetAllMessages().Any(m => m.Contains("The remote name could not be resolved"))
                            || exception.GetAllMessages().Any(m => m.Contains("A task was canceled"))
                            || exception.GetAllMessages()
                                .Any(
                                    m =>
                                        m.Contains(
                                            "A connection attempt failed because the connected party did not properly respond after a period of time"))
                            || exception.GetAllMessages()
                                .Any(m => m.Contains("No connection could be made because the target machine actively refused"))
                            || exception.GetAllMessages().Any(m => m.Contains("was forcibly closed by the remote host"))
                            || exception.GetAllMessages().Any(m => m.Contains("The connection was closed unexpectedly"))
                            || exception.GetAllMessages().Any(m => m.Contains("Unable to read data from the transport connection")))
                        {
                            this.Log()?.Error($"Error on: {requestJob.Uri}");
                            return requestJob;
                        }

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
                this.Log()?.Error(string.Empty, exception);
                throw;
            }
            finally
            {
                jobResult?.Dispose();
                //await jobCompletedBlock.SendAsync(requestJob);
            }

            return requestJob;
        }
    }
}