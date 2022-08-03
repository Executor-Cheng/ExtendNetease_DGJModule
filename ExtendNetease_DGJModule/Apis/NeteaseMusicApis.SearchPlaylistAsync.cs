using ExtendNetease_DGJModule.Exceptions;
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
        public static async Task<PlaylistInfo[]> SearchPlaylistsAsync(HttpClient client, string keywords, int pageSize = 30, int offset = 0, CancellationToken token = default)
        {
            JObject root = (JObject)await SearchAsync(client, keywords, SearchType.SongList, pageSize, offset, token).ConfigureAwait(false);
            if (root["code"].ToObject<int>() == 200)
            {
                return root["result"]["playlists"].Select(PlaylistInfo.Parse).ToArray();
            }
            throw new UnknownResponseException(root);
        }
    }
}
