﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Integration.DataAccess.Entitys;

namespace Implementation.Shared
{
    public static class HttpClientUtils
    {
        /// <summary>
        /// 30 Seconds
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public static TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Trust all certificates and ignore "errors" like; chain issues or distinguished name does not match...
        /// </summary>
        /// <returns>true</returns>
        // ReSharper disable once MemberCanBePrivate.Global
        public static bool ServerCertificateValidationCallbackHandler(object sender,
                                                                      X509Certificate certificate,
                                                                      X509Chain chain,
                                                                      SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public static async Task<HttpResponseMessage> MakeSimpleWebRequest(HttpClientRequestOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            using (var requestHandler = NewWebRequestHandler())
            using (var httpClient = NewHttpClient(requestHandler))
            {
                var clientRequest =
                    NewHttpClientRequest(
                        NewHttpRequestMessage(options.Method, options.Uri, options.Headers, new Version(options.HttpVersion)),
                        httpClient, options.CancellationToken);
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

        public static Task<HttpResponseMessage> NewHttpClientRequest(HttpRequestMessage requestMessage, HttpClient httpClient, CancellationToken cancellationToken)
        {
            if (httpClient == null)
            {
                throw new ArgumentNullException(nameof(httpClient));
            }

            if (requestMessage == null)
            {
                throw new ArgumentNullException(nameof(requestMessage));
            }

            return httpClient.SendAsync(requestMessage, cancellationToken);
        }

        /// <summary>
        /// Create a new WebRequestHandler, which will NOT allow autoredirection and ignores all server certificate validation errors.
        /// </summary>
        public static WebRequestHandler NewWebRequestHandler()
        {
            return new WebRequestHandler
                {
                    AllowAutoRedirect = false,
                    ServerCertificateValidationCallback = ServerCertificateValidationCallbackHandler
                };
        }

        public static HttpClient NewHttpClient(WebRequestHandler requestHandler)
        {
            if (requestHandler == null)
            {
                throw new ArgumentNullException(nameof(requestHandler));
            }

            var httpClient = new HttpClient(requestHandler) { Timeout = DefaultTimeout };
            return httpClient;
        }
    }
}