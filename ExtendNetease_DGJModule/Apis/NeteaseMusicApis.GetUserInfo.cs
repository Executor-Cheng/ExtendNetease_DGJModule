using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ExtendNetease_DGJModule.Clients;
using ExtendNetease_DGJModule.Crypto;
using ExtendNetease_DGJModule.Exceptions;
using ExtendNetease_DGJModule.Extensions;
using ExtendNetease_DGJModule.Models;
using Newtonsoft.Json.Linq;

namespace ExtendNetease_DGJModule.Apis
{
    public static partial class NeteaseMusicApis
    {
        public static async Task<UserInfo> GetUserInfoAsync(HttpClientv2 client, CancellationToken token = default)
        {
            UserInfo user = await GetUserInfoFromPageAsync(client, token).ConfigureAwait(false);
            UserInfo user2 = await GetUserInfoAsync(client, user.UserId, token).ConfigureAwait(false);
            user.VipType = user2.VipType;
            return user;
        }

        private static async Task<UserInfo> GetUserInfoFromPageAsync(HttpClient client, CancellationToken token = default)
        {
            var root = await client.GetAsync("https://music.163.com/discover/g/attr", token).GetObjectAsync<JObject>(token).ConfigureAwait(false);
            var node = root["g_visitor"];
            if (node != null && node.Type != JTokenType.Null)
            {
                return new UserInfo
                {
                    UserId = node["userId"].Value<long>(),
                    UserName = node["nickname"].Value<string>(),
                };
            }
            throw new InvalidCookieException();
        }

        private static async Task<UserInfo> GetUserInfoAsync(HttpClientv2 client, long userId, CancellationToken token = default)
        {
            JObject data = new JObject(new JProperty("csrf_token", GetCsrfToken(client)));
            CryptoHelper.WebApiEncryptedData encrypted = CryptoHelper.WebApiEncrypt(data);
            JObject root = (JObject)await client.PostAsync($"https://music.163.com/api/v1/user/detail/{userId}", encrypted.GetContent(), token).GetJsonAsync(token).ConfigureAwait(false);
            if (root["code"].ToObject<int>() == 200)
            {
                return new UserInfo()
                {
                    UserName = root["profile"]["nickname"].ToString(),
                    VipType = root["profile"]["vipType"].ToObject<int>()
                };
            }
            throw new UnknownResponseException(root);
        }
    }
}
