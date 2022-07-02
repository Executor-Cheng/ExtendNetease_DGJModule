using ExtendNetease_DGJModule.Apis;
using ExtendNetease_DGJModule.Clients;
using ExtendNetease_DGJModule.Exceptions;
using ExtendNetease_DGJModule.Models;
using ExtendNetease_DGJModule.Services;
using System;
using System.ComponentModel;
using System.Net;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ExtendNetease_DGJModule.NeteaseMusic.Services
{
    public sealed class NeteaseSession : INotifyPropertyChanged
    {
        /// <summary>
        /// 用户昵称
        /// </summary>
        public string UserName { get => _userName; private set { if (_userName != value) { _userName = value; OnPropertyChanged(); } } }

        /// <summary>
        /// 用户ID
        /// </summary>
        public long UserId { get => _userId; private set { if (_userId != value) { _userId = value; OnPropertyChanged(); } } }

        /// <summary>
        /// VIP类型(Todo:做成Enum)
        /// </summary>
        public int VipType { get => _vipType; private set { if (_vipType != value) { _vipType = value; OnPropertyChanged(); } } }

        /// <summary>
        /// 登录状态
        /// </summary>
        public bool LoginStatus { get => _loginStatus; private set { if (_loginStatus != value) { _loginStatus = value; OnPropertyChanged(); } } }

        private readonly HttpClientv2 _client;

        private readonly ConfigService _config;

        private string _userName;

        private long _userId;

        private int _vipType;

        private bool _loginStatus;

        /// <summary>
        /// 初始化 <see cref="NeteaseSession"/> 类的新实例
        /// </summary>
        public NeteaseSession(ConfigService config, HttpClientv2 client)
        {
            _client = client;
            _config = config;
        }

        public string GetCookie()
        {
            return string.Join("; ", _client.Cookies.GetCookies(new Uri("https://music.163.com/")).OfType<Cookie>().Select(p => $"{p.Name}={p.Value}"));
        }

        public void SetCookie(string cookie)
        {
            _config.Config.Cookie = cookie;
            foreach (Cookie c in _client.Cookies.GetCookies(new Uri("https://music.163.com/")))
            {
                c.Expired = true;
            }
            if (!string.IsNullOrEmpty(cookie))
            {
                _client.Cookies.SetCookies(new Uri("https://music.163.com/"), cookie.Replace(';', ','));
            }
        }

        private async Task LoginCore(Task loginTask, CancellationToken token = default)
        {
            await loginTask.ConfigureAwait(false);
            await RefreshAsync(token).ConfigureAwait(false);
            LoginStatus = true;
        }

        public Task LoginAsync(int countryCode, long phoneNumber, string password, CancellationToken token = default)
        {
            return LoginCore(NeteaseMusicApis.LoginAsync(_client, countryCode, phoneNumber, password, token), token);
        }

        public Task LoginAsync(string email, string password, CancellationToken token = default)
        {
            return LoginCore(NeteaseMusicApis.LoginAsync(_client, email, password, token), token);
        }

        public Task LoginAsync(string cookie, CancellationToken token = default)
        {
            SetCookie(cookie);
            return LoginCore(Task.CompletedTask, token);
        }

        public Task LogoutAsync(CancellationToken token = default)
        {
            UserName = null;
            UserId = 0;
            VipType = 0;
            LoginStatus = false;
            return NeteaseMusicApis.LogoutAsync(_client, token);
        }

        public async Task RefreshAsync(CancellationToken token = default)
        {
            try
            {
                UserInfo userInfo = await NeteaseMusicApis.GetUserInfoAsync(_client, token).ConfigureAwait(false);
                UserName = userInfo.UserName;
                UserId = userInfo.UserId;
                VipType = userInfo.VipType;
                LoginStatus = true;
            }
            catch (InvalidCookieException)
            {
                UserName = null;
                UserId = 0;
                VipType = 0;
                LoginStatus = false;
                throw;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
