using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ExtendNetease_DGJModule.Clients;
using ExtendNetease_DGJModule.Crypto;
using ExtendNetease_DGJModule.Exceptions;
using ExtendNetease_DGJModule.Extensions;
using ExtendNetease_DGJModule.Models;
using Newtonsoft.Json.Linq;

namespace ExtendNetease_DGJModule.Apis
{
    public static partial class NeteaseMusicApis
    {
        /// <summary>
        /// 处理登录回复
        /// </summary>
        /// <exception cref="LoginFailedException"/>
        /// <exception cref="UnknownResponseException"/>
        /// <param name="json">服务器返回的Json</param>
        private static UserInfo ReadLoginResult(JObject root)
        {
            int code = root["code"].ToObject<int>();
            switch (code)
            {
                case 200:
                    {
                        return new UserInfo
                        {
                            UserName = root["profile"]["nickname"].ToString(),
                            UserId = root["profile"]["userId"].ToObject<long>(),
                            VipType = root["profile"]["vipType"].ToObject<int>()
                        };
                    }
                case 501:
                    {
                        throw new LoginFailedException("给定的账号不存在");
                    }
                case 502:
                case 509:
                    {
                        throw new LoginFailedException(root["msg"].ToString());
                    }
                default:
                    {
                        throw new UnknownResponseException(root);
                    }
            }
        }

        /// <summary>
        /// 使用邮箱和密码异步登录
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <param name="mail">邮箱地址</param>
        /// <param name="password">密码</param>
        public static async Task<UserInfo> LoginAsync(HttpClient client, string mail, string password, CancellationToken token = default)
        {
            JObject data = new JObject(new JProperty("username", mail), new JProperty("password", CryptoHelper.MD5Encrypt(password)),
                new JProperty("rememberLogin", true));
            CryptoHelper.WebApiEncryptedData encrypted = CryptoHelper.WebApiEncrypt(data);
            JObject root = (JObject)await client.PostAsync("https://music.163.com/api/login", encrypted.GetContent(), token).GetJsonAsync(token).ConfigureAwait(false);
            return ReadLoginResult(root);
        }

        /// <summary>
        /// 使用手机号、国家代码和密码异步登录
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <param name="countryCode">国家代码,如86(中国)</param>
        /// <param name="phone">手机号码</param>
        /// <param name="password">密码</param>
        public static async Task<UserInfo> LoginAsync(HttpClient client, int countryCode, long phone, string password, CancellationToken token = default)
        {
            JObject data = new JObject(new JProperty("phone", phone), new JProperty("countrycode", countryCode),
                new JProperty("password", CryptoHelper.MD5Encrypt(password)), new JProperty("rememberLogin", true));
            CryptoHelper.WebApiEncryptedData encrypted = CryptoHelper.WebApiEncrypt(data);
            JObject root = (JObject)await client.PostAsync("https://music.163.com/api/login/cellphone", encrypted.GetContent(), token).GetJsonAsync(token).ConfigureAwait(false);
            return ReadLoginResult(root);
        }

        /// <summary>
        /// 异步创建用于二维码登录的key
        /// </summary>
        /// <exception cref="UnknownResponseException"></exception>
        public static async Task<string> CreateUnikeyAsync(HttpClientv2 client, CancellationToken token = default)
        {
            var guid = Guid.NewGuid().ToString("n");
            var ntesnnid = new Cookie("_ntes_nnid", $"{guid}%2C{Utils.DateTime2UnixTimeMillseconds(DateTime.Now)}", "/", ".163.com");
            client.Cookies.Add(ntesnnid);
            var ntesnuid = new Cookie("_ntes_nuid", guid, "/", ".163.com");
            client.Cookies.Add(ntesnuid);
            var iuqxldmzr = new Cookie("_iuqxldmzr_", "32", "/", ".163.com");
            client.Cookies.Add(iuqxldmzr);
            var jsessionId = new Cookie("JSESSIONID-WYYY", GetJSessionId(), "/", ".163.com");
            client.Cookies.Add(jsessionId);
            IDictionary<string, object> data = new Dictionary<string, object>(3)
            {
                ["noCheckToken"] = true,
                ["secureCaptcha"] = null,
                ["type"] = 1
            };
            CryptoHelper.WebApiEncryptedData encrypted = CryptoHelper.WebApiEncrypt(data);
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, "https://music.163.com/weapi/login/qrcode/unikey");
            req.Content = encrypted.GetContent();
            req.Headers.Add("x-channelsource", "undefined");
            req.Headers.Add("nm-gcore-status", "1");
            req.Headers.Referrer = new Uri("https://music.163.com/");
            req.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/134.0.0.0 Safari/537.36 Edg/134.0.0.0");
            JObject root = (JObject)await client.SendAsync(req, token).GetJsonAsync(token).ConfigureAwait(false);
            if (root["code"].ToObject<int>() == 200)
            {
                return root["unikey"].ToObject<string>();
            }
            throw new UnknownResponseException(root);
        }

        /// <summary>
        /// 异步获取key的状态
        /// </summary>
        /// <exception cref="UnknownResponseException"></exception>
        public static async Task<UnikeyStatus> GetUnikeyStatusAsync(HttpClient client, string unikey, CancellationToken token = default)
        {
            IDictionary<string, object> data = new Dictionary<string, object>(4)
            {
                ["key"] = unikey,
                ["noCheckToken"] = true,
                ["secureCaptcha"] = null,
                ["type"] = 1
            };
            CryptoHelper.WebApiEncryptedData encrypted = CryptoHelper.WebApiEncrypt(data);
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, "https://music.163.com/weapi/login/qrcode/client/login");
            req.Content = encrypted.GetContent();
            req.Headers.Add("x-channelsource", "undefined");
            req.Headers.Add("x-loginmethod", "QrCode");
            req.Headers.Add("nm-gcore-status", "1");
            req.Headers.Referrer = new Uri("https://music.163.com/");
            req.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/134.0.0.0 Safari/537.36 Edg/134.0.0.0");
            JObject root = (JObject)await client.SendAsync(req, token).GetJsonAsync(token).ConfigureAwait(false);
            UnikeyStatus result = root["code"].ToObject<UnikeyStatus>();
            if (!Enum.IsDefined(typeof(UnikeyStatus), result))
            {
                throw new UnknownResponseException(root);
            }
            return result;
        }

        private static string GetJSessionId()
        {
            const string src = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKMNOPQRSTUVWXYZ\\/+";
            StringBuilder sb = new StringBuilder(176);
            Random r = new Random();
            for (int i = 0; i < 176; i++)
            {
                sb.Append(src[r.Next(src.Length)]);
            }
            return sb.ToString();

        }
    }
}
