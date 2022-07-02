using ExtendNetease_DGJModule.Extensions;
using ExtendNetease_DGJModule.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ExtendNetease_DGJModule.Apis
{
    public static partial class NeteaseMusicApis
    {
        /// <summary>
        /// 获取版权以及下载链接
        /// </summary>
        /// <param name="session"></param>
        /// <param name="bitRate"></param>
        /// <param name="songIds"></param>
        /// <returns></returns>
        private static Task<JToken> GetPlayerUrlResponseAsync(HttpClient client, long[] songIds, Quality bitRate = Quality.SuperQuality, CancellationToken token = default)
        {
            IDictionary<string, object> data = new Dictionary<string, object>
            {
                ["ids"] = songIds,
                ["br"] = (int)bitRate
            };
            CryptoHelper.EApiEncryptedData encrypted = CryptoHelper.EApiEncrypt(data, "/api/song/enhance/player/url");
            return client.PostAsync("https://interface3.music.163.com/eapi/song/enhance/player/url", encrypted.GetContent(), token).GetJsonAsync(token);
        }
    }
}
