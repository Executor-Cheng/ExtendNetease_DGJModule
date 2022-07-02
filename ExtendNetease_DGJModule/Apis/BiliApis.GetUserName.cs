using ExtendNetease_DGJModule.Exceptions;
using ExtendNetease_DGJModule.Extensions;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ExtendNetease_DGJModule.Apis
{
    public static class BiliApis
    {
        public static async Task<string> GetUserNameAsync(HttpClient client, int userId, CancellationToken token = default)
        {
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, "https://space.bilibili.com/ajax/member/GetInfo");
            req.Headers.Add("Origin", "https://space.bilibili.com");
            req.Headers.Referrer = new Uri($"https://space.bilibili.com/{userId}/");
            req.Headers.Add("X-Requested-With", "XMLHttpRequest");
            JToken root = await client.SendAsync(req, token).GetJsonAsync(token).ConfigureAwait(false);
            if (root["status"].ToObject<bool>())
            {
                return root["data"]["name"].ToString();
            }
            throw new UnknownResponseException(root);
        }
    }
}
