using ExtendNetease_DGJModule.Apis;
using ExtendNetease_DGJModule.Clients;
using ExtendNetease_DGJModule.Models;
using ExtendNetease_DGJModule.NeteaseMusic.Services;
using QRCoder;
using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace ExtendNetease_DGJModule.Services
{
    public class QRCodeLoginService
    {
        private readonly HttpClientv2 _client;

        private readonly NeteaseSession _session;

        private string _key;

        public QRCodeLoginService(HttpClientv2 client, NeteaseSession session)
        {
            _client = client;
            _session = session;
        }

        public async Task<Bitmap> CreateQRCodeAsync(CancellationToken token = default)
        {
            _key = await NeteaseMusicApis.CreateUnikeyAsync(_client, token);
            return QRCodeHelper.GetQRCode($"https://music.163.com/login?codekey={_key}", 2, Color.Black, Color.White, QRCodeGenerator.ECCLevel.L);
        }

        public Task<UnikeyStatus> GetStatusAsync(CancellationToken token = default)
        {
            if (!string.IsNullOrEmpty(_key))
            {
                return NeteaseMusicApis.GetUnikeyStatusAsync(_client, _key, token);
            }
            throw new InvalidOperationException("请先调用 CreateQRCodeAsync");
        }
    }
}
