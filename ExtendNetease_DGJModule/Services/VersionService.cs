using ExtendNetease_DGJModule.Apis;
using ExtendNetease_DGJModule.Models;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ExtendNetease_DGJModule.Services
{
    public sealed class VersionService
    {
        private readonly PluginMain _plugin;

        private readonly HttpClient _client;

        public VersionService(PluginMain plugin, HttpClient client)
        {
            _plugin = plugin;
            _client = client;
        }

        public Task CheckAsync(string pluginName, CancellationToken token = default)
        {
            if (Version.TryParse(_plugin.PluginVer, out Version version))
            {
                return CheckAsyncCore(pluginName, version, token);
            }
            return Task.CompletedTask;
        }

        private async Task CheckAsyncCore(string pluginName, Version version, CancellationToken token)
        {
            try
            {
                PluginInfo pInfo = await DanmujiApis.GetPluginInfoAsync(_client, pluginName, token);
                if (pInfo.Version > version)
                {
                    _plugin.Log($"有新版本了喵~最新版本: {pInfo.Version}");
                    if (!string.IsNullOrEmpty(pInfo.UpdateDescription))
                    {
                        _plugin.Log(pInfo.UpdateDescription);
                    }
                    _plugin.Log($"下载地址: {pInfo.DownloadUrl}");
                }
            }
            catch (Exception e)
            {
                _plugin.Log($"无法获取插件更新信息。详细错误信息:{e}");
            }
        }
    }
}
