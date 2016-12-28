using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global

namespace Implementation.Shared
{
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Utils")]
    public static class HttpClientUtils
    {
        /// <summary>
        /// 30 Seconds
        /// </summary>
        public static TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// About 10 MB
        /// </summary>
        public static long DefaultMaxRequestContentBufferSize { get; set; } = (1 << 20) * 10;

        /// <summary>
        /// Trust all certificates and ignore "errors" like; chain issues or distinguished name does not match...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="sslPolicyErrors"></param>
        /// <returns>true</returns>
        public static bool ServerCertificateValidationCallbackHandler(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        /// <summary>
        /// Create a simple (awaitable) http web-request
        /// </summary>
        /// <param name="method">Use HttpMethod</param>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> MakeSimpleWebRequest(HttpMethod method, Uri uri)
        {
            return await MakeSimpleWebRequest(method, uri, DefaultTimeout, null, HttpVersion.Version11);
        }

        /// <summary>
        /// Create a simple (awaitable) http web-request
        /// </summary>
        /// <param name="method">Use HttpMethod</param>
        /// <param name="uri"></param>
        /// <param name="timeout">Defaults to HttpClientUtils.DefaultTimeout if null</param>
        /// <param name="headers"></param>
        /// <param name="httpVersion">Defaults to HttpVersion.Version11 if null</param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> MakeSimpleWebRequest(HttpMethod method, Uri uri, TimeSpan timeout, IReadOnlyCollection<KeyValuePair<string, string>> headers, Version httpVersion)
        {
            using (var requestHandler = NewWebRequestHandler(DefaultMaxRequestContentBufferSize))
            using (var httpClient = NewHttpClient(requestHandler, timeout))
            {
                var clientRequest = NewHttpClientRequest(NewHttpRequestMessage(method, uri, headers), httpClient);
                if (clientRequest == null)
                {
                    throw new ArgumentNullException();
                }

                return await clientRequest;
            }
        }

        /// <summary>
        /// Do a simple domain http request, with nullable and optional parameters
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="maxRequestContentBufferSize">Defaults to HttpClientUtils.DefaultMaxRequestContentBufferSize if null</param>
        /// <param name="timeout">Defaults to HttpClientUtils.DefaultTimeout if null</param>
        /// <param name="scheme">Defaults to "http://" if null</param>
        /// <param name="headers"></param>
        /// <param name="requestPathAndQueryString">I.e.: /posts/?q=asdf</param>
        /// <param name="httpVersion">Defaults to HttpVersion.Version11 if null</param>
        /// <returns>Awaitable task</returns>
        public static async Task<HttpResponseMessage> MakeSimpleWebRequest(string domain,
                                                                           long? maxRequestContentBufferSize,
                                                                           TimeSpan? timeout,
                                                                           string scheme = "http",
                                                                           IReadOnlyCollection<KeyValuePair<string, string>> headers = null,
                                                                           string requestPathAndQueryString = null,
                                                                           Version httpVersion = null)
        {
            using (var requestHandler = NewWebRequestHandler(maxRequestContentBufferSize))
            using (var httpClient = NewHttpClient(requestHandler, timeout))
            {
                var clientRequest = NewHttpClientRequest(NewHttpRequestMessage(method: HttpMethod.Get, domain: domain, requestPathAndQueryString: requestPathAndQueryString, headers: headers, scheme: scheme, httpVersion: httpVersion), httpClient);

                if (clientRequest == null)
                {
                    throw new ArgumentNullException();
                }

                return await clientRequest;
            }
        }

        /// <summary>
        /// Do a simple http-webrequest, with nullable and optional parameters
        /// </summary>
        /// <param name="method">Use HttpMethod</param>
        /// <param name="uri"></param>
        /// <param name="headers"></param>
        /// <param name="httpVersion">Defaults to HttpVersion.Version11 if null</param>
        /// <returns></returns>
        public static HttpRequestMessage NewHttpRequestMessage(HttpMethod method, Uri uri, IReadOnlyCollection<KeyValuePair<string, string>> headers = null, Version httpVersion = null)
        {
            if (httpVersion == null)
            {
                httpVersion = HttpVersion.Version11;
            }

            var request = new HttpRequestMessage { Method = method, Version = httpVersion, RequestUri = uri };

            if (headers != null && request.Headers != null && headers.Count > 0)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            return request;
        }

        /// <summary>
        /// Sends a (async) http request if you already have a HttpRequestMessage and a httpclientHttpClient.
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <param name="httpClient"></param>
        /// <returns>Awaitable task</returns>
        public static Task<HttpResponseMessage> NewHttpClientRequest(HttpRequestMessage requestMessage, HttpClient httpClient)
        {
            if (httpClient == null)
            {
                throw new ArgumentNullException(nameof(httpClient));
            }

            if (requestMessage == null)
            {
                throw new ArgumentNullException(nameof(requestMessage));
            }

            return httpClient.SendAsync(requestMessage);
        }

        /// <summary>
        /// Create a new WebRequestHandler, which will NOT allow autoredirection and ignores all server certificate validation errors.
        /// </summary>
        /// <param name="maxRequestContentBufferSize">Defaults to HttpClientUtils.DefaultMaxRequestContentBufferSize if null</param>
        /// <returns></returns>
        public static WebRequestHandler NewWebRequestHandler(long? maxRequestContentBufferSize = null)
        {
            if (maxRequestContentBufferSize == null)
            {
                maxRequestContentBufferSize = DefaultMaxRequestContentBufferSize;
            }

            return new WebRequestHandler { AllowAutoRedirect = false, MaxRequestContentBufferSize = (long)maxRequestContentBufferSize, ServerCertificateValidationCallback = ServerCertificateValidationCallbackHandler };
        }

        /// <summary>
        /// Creates a new HttpClient.
        /// </summary>
        /// <param name="requestHandler"></param>
        /// <param name="timeout">Defaults to HttpClientUtils.DefaultTimeout if null</param>
        /// <returns></returns>
        public static HttpClient NewHttpClient(WebRequestHandler requestHandler, TimeSpan? timeout = null)
        {
            if (requestHandler == null)
            {
                throw new ArgumentNullException(nameof(requestHandler));
            }

            if (timeout == null)
            {
                timeout = DefaultTimeout;
            }

            var httpClient = new HttpClient(requestHandler) { MaxResponseContentBufferSize = requestHandler.MaxRequestContentBufferSize, Timeout = (TimeSpan)timeout };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;
            return httpClient;
        }

        /// <summary>
        /// Creates new HttpRequestMessage,
        /// </summary>
        /// <param name="method">Use HttpMethod</param>
        /// <param name="domain"></param>
        /// <param name="requestPathAndQueryString">I.e.: /posts/?q=asdf</param>
        /// <param name="headers"></param>
        /// <param name="scheme">Defaults to "http://" if null</param>
        /// <param name="httpVersion"></param>
        /// <returns></returns>
        public static HttpRequestMessage NewHttpRequestMessage(HttpMethod method,
                                                                string domain,
                                                                string requestPathAndQueryString = "",
                                                                IReadOnlyCollection<KeyValuePair<string, string>> headers = null,
                                                                string scheme = "http",
                                                                Version httpVersion = null)
        {
            return NewHttpRequestMessage(method, new Uri($"{scheme}://{domain}{requestPathAndQueryString}"), headers, httpVersion);
        }
    }
}