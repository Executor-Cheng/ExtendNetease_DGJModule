using ExtendNetease_DGJModule.Exceptions;
using ExtendNetease_DGJModule.Extensions;
using ExtendNetease_DGJModule.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ExtendNetease_DGJModule.Apis
{
    public static partial class NeteaseMusicApis
    {
        public static async Task<SongInfo> GetSongDetail(HttpClient client, long songId, CancellationToken token = default)
        {
            SongInfo[] songs = await GetSongDetails(client, new long[1] { songId }, token);
            if (songs.Length == 0)
            {
                throw new NoSuchSongException();
            }
            return songs[0];
        }

        public static async Task<SongInfo[]> GetSongDetails(HttpClient client, long[] songIds, CancellationToken token = default)
        {
            SongInfo[] result = new SongInfo[songIds.Length];
            for (int i = 0; i < songIds.Length;)
            {
                int taken = Math.Min(1000, songIds.Length - i);
                KeyValuePair<string, string>[] payload = new KeyValuePair<string, string>[1]
                {
                    new KeyValuePair<string, string>("c", JsonConvert.SerializeObject(songIds.Skip(i).Take(taken).Select(p => new { id = p })))
                };
                JToken j = await client.PostAsync("https://music.163.com/api/v3/song/detail", new FormUrlEncodedContent(payload), token).GetJsonAsync(token);
                if (j["code"].ToObject<int>() != 200)
                {
                    throw new UnknownResponseException(j);
                }
                int k = i;
                foreach (JToken songNode in j["songs"])
                {
                    result[k++] = SongInfo.Parse(songNode);
                }
                i += taken;
            }
            return result;
        }
    }
}
