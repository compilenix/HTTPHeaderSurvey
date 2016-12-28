using Implementation.DataAccess;
using Implementation.Shared;
using Integration.DataAccess.Entitys;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace HTTPHeaderSurvey
{
    internal static class Program
    {
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "args")]
        // ReSharper disable once UnusedParameter.Local
        private static void Main(string[] args)
        {
            using (var context = new HttpHeaderDbContext())
            {
                context.Database.Initialize(false);
            }

            var requestHeader = new RequestHeader { Key = "User-Agent", Value = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36" };

            using (var unitOfWork = new UnitOfWork(new HttpHeaderDbContext()))
            {
                requestHeader = unitOfWork.RequestHeaders.Add(requestHeader);
                unitOfWork.Complete();
            }

            using (var unitOfWork = new UnitOfWork(new HttpHeaderDbContext()))
            {
                requestHeader = unitOfWork.RequestHeaders.Get(requestHeader.Id);
                var jobs = new[]{
                new RequestJob {
                    Headers = new List<RequestHeader> { requestHeader },
                    HttpVersion = HttpVersion.Version11.ToString(),
                    IsCurrentlyScheduled = false,
                    IsRunOnce = false,
                    LastCompletedDateTime = null,
                    Method = "GET",
                    Uri = "https://cdn.compilenix.org/"},
                new RequestJob {
                    Headers = new List<RequestHeader> { requestHeader },
                    HttpVersion = HttpVersion.Version11.ToString(),
                    IsCurrentlyScheduled = false,
                    IsRunOnce = false,
                    LastCompletedDateTime = null,
                    Method = "GET",
                    Uri = "https://git.compilenix.org/"},
                new RequestJob {
                    Headers = new List<RequestHeader> { requestHeader },
                    HttpVersion = HttpVersion.Version11.ToString(),
                    IsCurrentlyScheduled = false,
                    IsRunOnce = false,
                    LastCompletedDateTime = null,
                    Method = "GET",
                    Uri = "https://github.com/"},
                new RequestJob {
                    Headers = new List<RequestHeader> { requestHeader },
                    HttpVersion = HttpVersion.Version11.ToString(),
                    IsCurrentlyScheduled = false,
                    IsRunOnce = false,
                    LastCompletedDateTime = null,
                    Method = "GET",
                    Uri = "https://www.plan.de/"}};

                typeof(Program).Log()?.Debug("Adding new Jobs");
                unitOfWork.RequestJobs.AddRange(jobs);
                unitOfWork.Complete();
                typeof(Program).Log()?.Debug("New Jobs has been added");
            }

            //            var domains = new List<string>();
            //            domains.AddRange(new[] { "https://www.paypal.com" });
            //
            //            var requestPath = "de/webapps/mpp/home";
            //            var requestQuery = string.Empty;
            //            var maxRequestContentBufferSize = (1 << 20) * 10;
            //            var timeout = TimeSpan.FromSeconds(30);
            //            var httpVersion = new Version("1.1");
            //            var requestHeaders = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.116 Safari/537.36") };
            //
            //            foreach (var domain in domains)
            //            {
            //                try
            //                {
            //                    var method = HttpMethod.Get;
            //                    var uriString = $"{domain}/{requestPath}/{requestQuery}";
            //                    var uri = new Uri(uriString);
            //                    var httpTimeout = TimeSpan.FromSeconds(30);
            //
            //                    using (var requestMessage = HttpClientUtils.NewHttpRequestMessage(method, uri, requestHeaders, httpVersion))
            //                    using (var responseMessage = HttpClientUtils.NewHttpClientRequest(requestMessage, HttpClientUtils.NewHttpClient(HttpClientUtils.NewWebRequestHandler(maxRequestContentBufferSize), httpTimeout)))
            //                    {
            //                        var result = responseMessage?.Result;
            //                        if (result?.RequestMessage != null)
            //                        {
            //                            Console.WriteLine(result.RequestMessage.RequestUri);
            //                            Console.Write(result.Headers);
            //                            Console.WriteLine($"{(int)result.StatusCode} {result.StatusCode}\n");
            //                        }
            //                    }
            //                }
            //                catch (AggregateException exception)
            //                {
            //                    "Main".Log()?.Fatal("Something went wrong...", exception);
            //                }
            //            }
        }
    }
}