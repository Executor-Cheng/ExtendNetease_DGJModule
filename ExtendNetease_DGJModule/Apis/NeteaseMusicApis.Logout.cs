using ExtendNetease_DGJModule.Clients;
using ExtendNetease_DGJModule.Crypto;
using ExtendNetease_DGJModule.Exceptions;
using ExtendNetease_DGJModule.Extensions;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ExtendNetease_DGJModule.Apis
{
    public static partial class NeteaseMusicApis
    {
        public static async Task LogoutAsync(HttpClientv2 client, CancellationToken token = default)
        {
            IDictionary<string, object> data = new Dictionary<string, object>
            {
                ["csrf_token"] = GetCsrfToken(client),
            };
            CryptoHelper.WebApiEncryptedData encrypted = CryptoHelper.WebApiEncrypt(data);
            JObject root = (JObject)await client.PostAsync("https://music.163.com/weapi/logout", encrypted.GetContent(), token).GetJsonAsync(token).ConfigureAwait(false);
            if (root["code"].ToObject<int>() != 0)
            {
                throw new UnknownResponseException(root);
            }
        }
    }
}
