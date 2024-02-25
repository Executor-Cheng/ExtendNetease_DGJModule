using ExtendNetease_DGJModule.Clients;
using ExtendNetease_DGJModule.Crypto;
using ExtendNetease_DGJModule.Exceptions;
using ExtendNetease_DGJModule.Extensions;
using ExtendNetease_DGJModule.Models;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

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
            string html = await client.GetAsync("https://music.163.com/", token).ForcePlain().GetStringAsync(token).ConfigureAwait(false);
            Match m = Regex.Match(html, @"GUser\s*=\s*([^;]+);");
            if (m.Success)
            {
                string js = m.Groups[1].Value;
                if (js != "{}")
                {
                    return new UserInfo
                    {
                        UserId = long.Parse(Regex.Match(js, @"userId:(\d+)").Groups[1].Value),
                        UserName = Regex.Match(js, @"nickname:""(.*?)""").Groups[1].Value,
                    };
                }
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
