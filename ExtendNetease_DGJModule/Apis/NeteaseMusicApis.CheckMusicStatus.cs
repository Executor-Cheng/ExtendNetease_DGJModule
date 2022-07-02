using ExtendNetease_DGJModule.Exceptions;
using ExtendNetease_DGJModule.Extensions;
using ExtendNetease_DGJModule.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ExtendNetease_DGJModule.Apis
{
    public static partial class NeteaseMusicApis
    {
        /// <summary>
        /// 检查给定ID对应的音乐有无版权
        /// </summary>
        /// <param name="bitRate">比特率</param>
        /// <param name="songIds">音乐IDs</param>
        /// <returns></returns>
        public static async Task<IDictionary<long, bool>> CheckMusicStatusAsync(HttpClient client, long[] songIds, CancellationToken token = default)
        {
            JObject root = (JObject)await GetPlayerUrlResponseAsync(client, songIds, Quality.SuperQuality, token).ConfigureAwait(false);
            if (root["code"].ToObject<int>() == 200)
            {
                return root["data"].ToDictionary(p => p["id"].ToObject<long>(), p => p["code"].ToObject<int>() == 200);
            }
            throw new UnknownResponseException(root);
        }
    }
}
