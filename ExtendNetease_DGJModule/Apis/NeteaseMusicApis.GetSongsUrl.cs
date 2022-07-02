using ExtendNetease_DGJModule.Models;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ExtendNetease_DGJModule.Apis
{
    public static partial class NeteaseMusicApis
    {
        /// <summary>
        /// 批量获取单曲下载链接
        /// </summary>
        /// <param name="bitRate">比特率上限</param>
        /// <param name="songIds">单曲IDs</param>
        public static async Task<DownloadSongInfo[]> GetSongsUrlAsync(HttpClient client, long[] songIds, Quality bitRate = Quality.SuperQuality, CancellationToken token = default)
        {
            JObject j = (JObject)await GetPlayerUrlResponseAsync(client, songIds, bitRate, token).ConfigureAwait(false);
            return j["data"].Select(p => new DownloadSongInfo(p["id"].ToObject<long>(), p["br"].ToObject<int>(), bitRate, p["url"].ToString(), p["type"].ToString())).ToArray();
        }
    }
}
