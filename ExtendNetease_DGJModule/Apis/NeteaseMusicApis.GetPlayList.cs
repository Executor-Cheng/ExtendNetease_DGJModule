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
        /// 获取歌单内的所有单曲
        /// </summary>
        /// <exception cref="NoSuchPlaylistException"/>
        /// <exception cref="PlaylistAccessDeniedException"/>
        /// <exception cref="UnknownResponseException"/>
        /// <param name="id">歌单Id</param>
        public static async Task<SongInfo[]> GetPlaylistAsync(HttpClient client, long id, CancellationToken token = default)
        {
            KeyValuePair<string, string>[] payload = new KeyValuePair<string, string>[3]
            {
                new KeyValuePair<string, string>("id", id.ToString()),
                new KeyValuePair<string, string>("n", "100000"),
                new KeyValuePair<string, string>("s", "8")
            };
            JObject j = (JObject)await client.PostAsync("https://music.163.com/api/v6/playlist/detail", new FormUrlEncodedContent(payload), token).GetJsonAsync(token).ConfigureAwait(false);
            int code = j["code"].ToObject<int>();
            switch (code)
            {
                case 200:
                    {
                        long[] songIds = j["playlist"]["trackIds"].Select(p => p["id"].ToObject<long>()).ToArray();
                        return await GetSongDetails(client, songIds, token);
                    }
                case 401:
                    {
                        throw new PlaylistAccessDeniedException(id);
                    }
                case 404:
                    {
                        throw new NoSuchPlaylistException(id);
                    }
                default:
                    {
                        throw new UnknownResponseException(j);
                    }
            }
        }
    }
}
