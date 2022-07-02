using ExtendNetease_DGJModule.Extensions;
using ExtendNetease_DGJModule.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ExtendNetease_DGJModule.Apis
{
    public static class DanmujiApis
    {
        public static async Task<PluginInfo> GetPluginInfoAsync(HttpClient client, string pluginName, CancellationToken token = default)
        {
            JToken root = await client.GetAsync($"https://www.danmuji.org/api/v2/{pluginName}", token).GetJsonAsync(token).ConfigureAwait(false);
            return new PluginInfo()
            {
                Name = root["name"].ToString(),
                Author = root["author"].ToString(),
                Version = Version.Parse(root["version"].ToString()),
                Description = root["description"].ToString(),
                UpdateTime = DateTimeOffset.Parse(root["update_datetime"].ToString(), null).DateTime,
                UpdateDescription = root["update_desc"].ToString(),
                DownloadUrl = new Uri(new Uri("https://www.danmuji.org"), root["dl_url"].ToString()).ToString(),
                DownloadNote = root["dl_note"].ToString()
            };
        }
    }
}
