using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Integration.DataAccess.Entitys;

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
        /// <returns>true</returns>
        public static bool ServerCertificateValidationCallbackHandler(object sender,
                                                                      X509Certificate certificate,
                                                                      X509Chain chain,
                                                                      SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public static async Task<HttpResponseMessage> MakeSimpleWebRequest(string method, Uri uri)
        {
            return await MakeSimpleWebRequest(method, uri, DefaultTimeout, null, HttpVersion.Version11);
        }

        public static async Task<HttpResponseMessage> MakeSimpleWebRequest(string method,
                                                                           Uri uri,
                                                                           TimeSpan timeout,
                                                                           ICollection<RequestHeader> headers,
                                                                           Version httpVersion)
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

        public static HttpRequestMessage NewHttpRequestMessage(string method,
                                                               Uri uri,
                                                               ICollection<RequestHeader> headers = null,
                                                               Version httpVersion = null)
        {
            if (httpVersion == null)
            {
                httpVersion = HttpVersion.Version11;
            }

            var request = new HttpRequestMessage { Method = new HttpMethod(method), Version = httpVersion, RequestUri = uri };

            if (headers != null && request.Headers != null && headers.Count > 0)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            return request;
        }

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
        public static WebRequestHandler NewWebRequestHandler(long? maxRequestContentBufferSize = null)
        {
            if (maxRequestContentBufferSize == null)
            {
                maxRequestContentBufferSize = DefaultMaxRequestContentBufferSize;
            }

            return new WebRequestHandler
                {
                    AllowAutoRedirect = false,
                    MaxRequestContentBufferSize = (long)maxRequestContentBufferSize,
                    ServerCertificateValidationCallback = ServerCertificateValidationCallbackHandler
                };
        }

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

            var httpClient = new HttpClient(requestHandler)
                {
                    MaxResponseContentBufferSize = requestHandler.MaxRequestContentBufferSize,
                    Timeout = (TimeSpan)timeout
                };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls
                                                   | SecurityProtocolType.Ssl3;
            return httpClient;
        }

        public static HttpRequestMessage NewHttpRequestMessage(string method,
                                                               string domain,
                                                               string requestPathAndQueryString = "",
                                                               ICollection<RequestHeader> headers = null,
                                                               string scheme = "http",
                                                               Version httpVersion = null)
        {
            return NewHttpRequestMessage(method, new Uri($"{scheme}://{domain}{requestPathAndQueryString}"), headers, httpVersion);
        }
    }
}