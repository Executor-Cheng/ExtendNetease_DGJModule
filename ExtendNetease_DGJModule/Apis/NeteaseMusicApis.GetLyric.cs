using ExtendNetease_DGJModule.Clients;
using ExtendNetease_DGJModule.Exceptions;
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
        /// 获取给定单曲ID的歌词
        /// </summary>
        /// <exception cref="UnknownResponseException"/>
        /// <param name="songId">单曲ID</param>
        public static async Task<LyricInfo> GetLyricAsync(HttpClientv2 client, long songId, CancellationToken token = default)
        {
            KeyValuePair<string, string>[] payload = new KeyValuePair<string, string>[5]
            {
                new KeyValuePair<string, string>("id", songId.ToString()),
                new KeyValuePair<string, string>("lv", "-1"),
                new KeyValuePair<string, string>("tv", "-1"),
                new KeyValuePair<string, string>("rv", "-1"),
                new KeyValuePair<string, string>("kv", "-1")
            };
            JObject root = (JObject)await client.PostAsync("https://music.163.com/api/song/lyric?_nmclfl=1", new FormUrlEncodedContent(payload), token).GetJsonAsync(token).ConfigureAwait(false);
            if (root["code"].ToObject<int>() == 200)
            {
                string lyricText = root["lrc"]?["lyric"]?.ToString();
                string translatedLyricText = root["tlyric"]?["lyric"]?.ToString();
                LyricInfo lyric = null;
                if (!string.IsNullOrEmpty(lyricText))
                {
                    lyric = new LyricInfo(lyricText);
                }
                if (!string.IsNullOrEmpty(translatedLyricText))
                {
                    lyric.AppendLrc(translatedLyricText);
                }
                return lyric;
            }
            throw new UnknownResponseException(root);
        }
    }
}
