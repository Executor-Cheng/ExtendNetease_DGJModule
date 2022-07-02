using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#pragma warning disable CS1573 // Parameter <parameter> has no matching param tag in the XML comment for <method> (but other parameters do)
namespace ExtendNetease_DGJModule.Extensions
{
    /// <summary>
    /// Contains the extensions methods for easily performing request or handling response in HttpClient.
    /// </summary>
    public static partial class HttpClientExtensions
    {
        private static Version DefaultHttpVersion { get; } = new Version(2, 0);

        private static readonly MediaTypeHeaderValue DefaultJsonMediaType = new MediaTypeHeaderValue("application/json") { CharSet = "utf-8" };

        private static readonly MediaTypeHeaderValue DefaultPlainType = new MediaTypeHeaderValue("text/plain") { CharSet = "utf-8" };

        /// <param name="client">The <see cref="HttpClient"/>.</param>
        /// <param name="method">The HTTP method.</param>
        /// <param name="uri">The Uri to request.</param>
        /// <param name="content">The contents of the HTTP message.</param>
        /// <param name="token">A <see cref="CancellationToken"/> which may be used to cancel the request operation.</param>
        /// <inheritdoc cref="HttpClient.SendAsync(HttpRequestMessage, CancellationToken)"/>
        public static async Task<HttpResponseMessage> SendAsync(this HttpClient client, HttpMethod method, Uri uri, HttpContent? content, CancellationToken token = default)
        {
            using HttpRequestMessage request = new HttpRequestMessage(method, uri)
            {
                Content = content,
                Version = DefaultHttpVersion
            };
            return await client.SendAsync(request, token);
        }

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0060 // Remove unused parameter
        /// <param name="responseTask">An asynchronous operation that represents the HTTP response.</param>
        /// <param name="token">The <paramref name="token"/> is ignored since <see cref="HttpContent.ReadAsByteArrayAsync()"/> has no reload that uses <see cref="CancellationToken"/>.</param>
        /// <inheritdoc cref="HttpContent.ReadAsByteArrayAsync()"/>
        public static async Task<byte[]> GetBytesAsync(this Task<HttpResponseMessage> responseTask, CancellationToken token = default)
        {
            using HttpResponseMessage response = await responseTask.ConfigureAwait(false);
            return await response.Content.ReadAsByteArrayAsync();
        }

        /// <param name="responseTask">An asynchronous operation that represents the HTTP response.</param>
        /// <param name="token">The <paramref name="token"/> is ignored since <see cref="HttpContent.ReadAsStringAsync()"/> has no reload that uses <see cref="CancellationToken"/>.</param>
        /// <inheritdoc cref="HttpContent.ReadAsStringAsync()"/>
        public static async Task<string> GetStringAsync(this Task<HttpResponseMessage> responseTask, CancellationToken token = default)
        {
            using HttpResponseMessage response = await responseTask.ConfigureAwait(false);
            return await response.Content.ReadAsStringAsync();
        }
#pragma warning restore IDE0060
#pragma warning restore IDE0079

        /// <inheritdoc cref="GetObjectAsync{T}(Task{HttpResponseMessage}, JsonSerializerSettings?, CancellationToken)"/>
        public static Task<T> GetObjectAsync<T>(this Task<HttpResponseMessage> responseTask, CancellationToken token = default)
        {
            return responseTask.GetObjectAsync<T>(null, token);
        }

        /// <summary>
        /// Deserializes the HTTP content to an instance of <typeparamref name="T"/> as an asynchronous operation.
        /// </summary>
        /// <param name="responseTask">An asynchronous operation that represents the HTTP response.</param>
        /// <param name="options">A <see cref="JsonSerializerSettings"/> to be used while deserializing the HTTP content.</param>
        /// <param name="token">The <paramref name="token"/> is ignored since <see cref="HttpContent.ReadAsStringAsync"/> has no reload that uses <see cref="CancellationToken"/>.</param>
        /// <returns>A task that represents the asynchronous deserialize operation.</returns>
        public static async Task<T> GetObjectAsync<T>(this Task<HttpResponseMessage> responseTask, JsonSerializerSettings? options, CancellationToken token = default)
        {
            string content = await responseTask.GetStringAsync(token);
            return JsonConvert.DeserializeObject<T>(content, options)!;
        }

        /// <inheritdoc cref="GetObjectAsync(Task{HttpResponseMessage}, Type, JsonSerializerSettings?, CancellationToken)"/>
        public static Task<object?> GetObjectAsync(this Task<HttpResponseMessage> responseTask, Type returnType, CancellationToken token = default)
        {
            return responseTask.GetObjectAsync(returnType, null, token);
        }

        /// <summary>
        /// Deserializes the HTTP content to an instance of <paramref name="returnType"/> as an asynchronous operation.
        /// </summary>
        /// <param name="returnType">The type of the HTTP content to convert to and return.</param>
        /// <inheritdoc cref="GetObjectAsync{T}(Task{HttpResponseMessage}, JsonSerializerSettings?, CancellationToken)"/>
        public static async Task<object?> GetObjectAsync(this Task<HttpResponseMessage> responseTask, Type returnType, JsonSerializerSettings? options, CancellationToken token = default)
        {
            string content = await responseTask.GetStringAsync(token);
            return JsonConvert.DeserializeObject(content, returnType, options);
        }

        /// <inheritdoc cref="GetJsonAsync(Task{HttpResponseMessage}, JsonSerializerSettings, CancellationToken)"/>
        public static Task<JToken> GetJsonAsync(this Task<HttpResponseMessage> responseTask, CancellationToken token = default)
        {
            return responseTask.GetJsonAsync(null, token);
        }

        /// <summary>
        /// Deserializes the HTTP content to an instance of <see cref="JToken"/> as an asynchronous operation.
        /// </summary>
        /// <inheritdoc cref="GetObjectAsync(Task{HttpResponseMessage}, JsonSerializerSettings, CancellationToken)"/>
        public static Task<JToken> GetJsonAsync(this Task<HttpResponseMessage> responseTask, JsonSerializerSettings? options, CancellationToken token = default)
        {
            return responseTask.GetObjectAsync<JToken>(options, token);
        }

        /// <summary>
        /// Sets Content-Type in response to text/plain; charset=utf-8.
        /// </summary>
        /// <param name="responseTask">An asynchronous operation that represents the HTTP response.</param>
        public static Task<HttpResponseMessage> ForcePlain(this Task<HttpResponseMessage> responseTask)
        {
            return responseTask.ContinueWith(p =>
            {
                if ((p.Status & (TaskStatus.RanToCompletion | TaskStatus.Canceled | TaskStatus.Faulted)) == TaskStatus.RanToCompletion) // treats response as text/plain; charset=utf-8
                {
                    HttpContentHeaders headers = p.Result.Content.Headers;
                    headers.ContentType = DefaultPlainType;
                }
                return p;
            }, TaskContinuationOptions.ExecuteSynchronously).Unwrap();
        }
    }
}
