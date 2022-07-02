using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
namespace ExtendNetease_DGJModule.Extensions
{
    public static partial class HttpClientExtensions
    {
        /// <param name="client">The <see cref="HttpClient"/>.</param>
        /// <param name="uri">The url the request is sent to.</param>
        /// <param name="token">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <inheritdoc cref="HttpClient.PostAsync(string, HttpContent, CancellationToken)"/>
        public static Task<HttpResponseMessage> PostAsync(this HttpClient client, Uri uri, byte[] content, CancellationToken token = default)
            => client.PostAsync(uri, new ByteArrayContent(content), token);

        /// <param name="url">The url the request is sent to.</param>
        /// <inheritdoc cref="PostAsync(HttpClient, Uri, byte[], CancellationToken)"/>
        public static Task<HttpResponseMessage> PostAsync(this HttpClient client, string url, byte[] content, CancellationToken token = default)
            => client.PostAsync(new Uri(url), content, token);
    }
}
