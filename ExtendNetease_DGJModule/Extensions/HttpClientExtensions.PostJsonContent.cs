using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json;

namespace ExtendNetease_DGJModule.Extensions
{
    public static partial class HttpClientExtensions
    {
        /// <param name="value">The value to be serialized to <see cref="HttpContent"/>.</param>
        /// <param name="options">A <see cref="JsonSerializerSettings"/> to be used while serializing the <paramref name="value"/> to <see cref="HttpContent"/>.</param>
        /// <inheritdoc cref="PostAsync(HttpClient, Uri, byte[], CancellationToken)"/>
        public static Task<HttpResponseMessage> PostAsJsonAsync<TValue>(this HttpClient client, Uri uri, TValue value, JsonSerializerSettings? options, CancellationToken token = default)
        {
            StringContent content = new StringContent(JsonConvert.SerializeObject(value, options!), Encoding.UTF8);
            content.Headers.ContentType = DefaultJsonMediaType;
            return client.PostAsync(uri, content, token);
        }

        /// <param name="url">The url the request is sent to.</param>
        /// <inheritdoc cref="PostAsJsonAsync{TValue}(HttpClient, Uri, TValue, JsonSerializerSettings?, CancellationToken)"/>
        public static Task<HttpResponseMessage> PostAsJsonAsync<TValue>(this HttpClient client, string url, TValue value, JsonSerializerSettings? options, CancellationToken token = default)
            => client.PostAsJsonAsync(new Uri(url), value, options, token);
    }
}
