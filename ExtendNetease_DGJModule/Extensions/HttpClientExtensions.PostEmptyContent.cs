using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ExtendNetease_DGJModule.Extensions
{
    public static partial class HttpClientExtensions
    {
        /// <inheritdoc cref="PostAsync(HttpClient, Uri, byte[], CancellationToken)"/>
        public static Task<HttpResponseMessage> PostAsync(this HttpClient client, Uri uri, CancellationToken token = default)
            => client.PostAsync(uri, null!, token);

        /// <inheritdoc cref="PostAsync(HttpClient, string, byte[], CancellationToken)"/>
        public static Task<HttpResponseMessage> PostAsync(this HttpClient client, string url, CancellationToken token = default)
            => client.PostAsync(new Uri(url), token);
    }
}
