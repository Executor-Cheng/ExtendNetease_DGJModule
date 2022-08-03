using ExtendNetease_DGJModule.Crypto;
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
        /// 获取歌曲下载链接
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
            CryptoHelper.WebApiEncryptedData encrypted = CryptoHelper.WebApiEncrypt(data);
            return client.PostAsync("https://music.163.com/weapi/song/enhance/player/url", encrypted.GetContent(), token).GetJsonAsync(token);
        }
    }
}
