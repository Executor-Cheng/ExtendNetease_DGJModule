using ExtendNetease_DGJModule.Exceptions;
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
        /// 按给定的关键词搜索单曲
        /// </summary>
        /// <param name="keywords">关键词</param>
        /// <param name="pageSize">本次搜索返回的实例个数上限</param>
        /// <param name="offset">偏移量</param>
        public static async Task<SongInfo[]> SearchSongsAsync(HttpClient client, string keywords, int pageSize = 30, int offset = 0, CancellationToken token = default)
        {
            JObject root = (JObject)await SearchAsync(client, keywords, SearchType.Song, pageSize, offset, token).ConfigureAwait(false);
            if (root["code"].ToObject<int>() == 200)
            {
                SongInfo[] result = root["result"]["songs"].Select(p => new SongInfo(p)).ToArray();
                IDictionary<long, bool> canPlayDic = await CheckMusicStatusAsync(client, result.Select(p => p.Id).ToArray(), token);
                foreach (SongInfo song in result)
                {
                    if (canPlayDic.TryGetValue(song.Id, out bool canPlay))
                    {
                        song.CanPlay = canPlay;
                    }
                }
                return result;
            }
            throw new UnknownResponseException(root);
        }
    }
}
