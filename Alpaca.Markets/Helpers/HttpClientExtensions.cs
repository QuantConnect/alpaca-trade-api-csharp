﻿using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Alpaca.Markets
{
    internal static partial class HttpClientExtensions
    {
#if !NETSTANDARD1_3
        private static readonly String _sdkVersion =
            typeof(HttpClientExtensions).Assembly.GetName().Version!.ToString();
#endif

        public static void AddAuthenticationHeaders(
            this HttpClient httpClient,
            SecurityKey securityKey)
        {
            foreach (var pair in securityKey.GetAuthenticationHeaders())
            {
                httpClient.DefaultRequestHeaders.Add(pair.Key, pair.Value);
            }
        }

        public static HttpClient Configure(
            this HttpClient httpClient,
            Uri baseAddress)
        {
            httpClient.DefaultRequestHeaders.Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.AcceptEncoding
                .Add(new StringWithQualityHeaderValue("gzip"));
#if !NETSTANDARD1_3
            httpClient.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("Alpaca-dotNET-SDK", _sdkVersion));
#endif
            httpClient.BaseAddress = baseAddress;

#if NET45
            System.Net.ServicePointManager.SecurityProtocol =
#pragma warning disable CA5364 // Do Not Use Deprecated Security Protocols
#pragma warning disable CA5386 // Avoid hard coding SecurityProtocolType value
                System.Net.SecurityProtocolType.Tls12 | System.Net.SecurityProtocolType.Tls11;
#pragma warning restore CA5386 // Avoid hard coding SecurityProtocolType value
#pragma warning restore CA5364 // Do Not Use Deprecated Security Protocols
#endif
            return httpClient;
        }

        private static async Task<TApi> callAndDeserializeAsync<TApi, TJson>(
            HttpClient httpClient,
            HttpMethod method,
            Uri endpointUri,
            CancellationToken cancellationToken,
            IThrottler? throttler = null)
            where TJson : TApi
        {
            using var request = new HttpRequestMessage(method, endpointUri);
            return await callAndDeserializeAsync<TApi, TJson>(
                httpClient, request, cancellationToken, throttler)
                .ConfigureAwait(false);
        }

        private static async Task<TApi> callAndDeserializeAsync<TApi, TJson, TContent>(
            HttpClient httpClient,
            HttpMethod method,
            Uri endpointUri,
            TContent content,
            CancellationToken cancellationToken,
            IThrottler? throttler = null)
            where TJson : TApi
        {
            using var request = new HttpRequestMessage(method, endpointUri) { Content = toStringContent(content) };
            return await callAndDeserializeAsync<TApi, TJson>(
                httpClient, request, cancellationToken, throttler)
                .ConfigureAwait(false);
        }

        private static async Task<TApi> callAndDeserializeAsync<TApi, TJson>(
            HttpClient httpClient,
            HttpRequestMessage request,
            CancellationToken cancellationToken,
            IThrottler? throttler = null)
            where TJson : TApi
        {
            using var response = await sendThrottledAsync(
                    httpClient, request, cancellationToken, throttler)
                .ConfigureAwait(false);

            return await response.DeserializeAsync<TApi, TJson>()
                .ConfigureAwait(false);
        }

        private static async Task<Boolean> callAndReturnSuccessCodeAsync(
            HttpClient httpClient,
            HttpMethod method,
            Uri endpointUri,
            CancellationToken cancellationToken,
            IThrottler? throttler = null)
        {
            using var request = new HttpRequestMessage(method, endpointUri);

            using var response = await sendThrottledAsync(
                    httpClient, request, cancellationToken, throttler)
                .ConfigureAwait(false);

            return await response.IsSuccessStatusCodeAsync()
                .ConfigureAwait(false);
        }

        
        private static async Task<HttpResponseMessage> sendThrottledAsync(
            HttpClient httpClient,
            HttpRequestMessage request,
            CancellationToken cancellationToken,
            IThrottler? throttler = null)
        {
            throttler ??= FakeThrottler.Instance;

            for(var attempts = 0; attempts < throttler.MaxRetryAttempts; ++attempts)
            {
                await throttler.WaitToProceed(cancellationToken).ConfigureAwait(false);

                var response = await httpClient
                    .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                    .ConfigureAwait(false);

                // Check response for server and caller specified waits and retries
                if (throttler.CheckHttpResponse(response))
                {
                    return response;
                }
                
                response.Dispose();
            }

            throw new RestClientErrorException(
                $"Unable to successfully call REST API endpoint `{request.RequestUri}` after {throttler.MaxRetryAttempts} attempts.");
        }

        private static Uri asUri(String endpointUri) => new Uri(endpointUri, UriKind.RelativeOrAbsolute);

        private static StringContent toStringContent<T>(T value)
        {
            var serializer = new JsonSerializer();
            using var stringWriter = new StringWriter();

            serializer.Serialize(stringWriter, value);
            return new StringContent(stringWriter.ToString());
        }
    }
}
