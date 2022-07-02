using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
namespace ExtendNetease_DGJModule.Extensions
{
    public static partial class HttpClientExtensions
    {
        private static readonly string DefaultBoundary = $"ACRCloudSdkCore/{Assembly.GetExecutingAssembly().GetName().Version}";

        /// <param name="client">The <see cref="HttpClient"/>.</param>
        /// <param name="uri">The url the request is sent to.</param>
        /// <param name="contents">An <see cref="IEnumerable{HttpContent}"/>, will be serialized to multipart/form-data MIME type.</param>
        /// <param name="token">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <inheritdoc cref="HttpClient.PostAsync(Uri, HttpContent, CancellationToken)"/>
        public static Task<HttpResponseMessage> PostAsync(this HttpClient client, Uri uri, IEnumerable<HttpContent> contents, CancellationToken token = default)
        {
            MultipartFormDataContent multipart = new MultipartFormDataContent(DefaultBoundary);
            foreach (HttpContent content in contents)
            {
                multipart.Add(content);
            }
            return client.PostAsync(uri, multipart, token);
        }

        /// <param name="url">The url the request is sent to.</param>
        /// <inheritdoc cref="PostAsync(HttpClient, Uri, IEnumerable{HttpContent}, CancellationToken)"/>
        public static Task<HttpResponseMessage> PostAsync(this HttpClient client, string url, IEnumerable<HttpContent> contents, CancellationToken token = default)
            => client.PostAsync(new Uri(url), contents, token);
    }
}
