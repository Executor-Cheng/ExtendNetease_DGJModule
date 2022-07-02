using ExtendNetease_DGJModule.Exceptions;
using ExtendNetease_DGJModule.Extensions;
using ExtendNetease_DGJModule.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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
        public static async Task<string> CreateUnikeyAsync(HttpClient client, CancellationToken token = default)
        {
            IDictionary<string, object> data = new Dictionary<string, object>(1)
            {
                ["type"] = 1
            };
            CryptoHelper.WebApiEncryptedData encrypted = CryptoHelper.WebApiEncrypt(data);
            JObject root = (JObject)await client.PostAsync("https://music.163.com/weapi/login/qrcode/unikey", encrypted.GetContent(), token).GetJsonAsync(token).ConfigureAwait(false);
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
            IDictionary<string, object> data = new Dictionary<string, object>(2)
            {
                ["key"] = unikey,
                ["type"] = 1
            };
            CryptoHelper.WebApiEncryptedData encrypted = CryptoHelper.WebApiEncrypt(data);
            JObject root = (JObject)await client.PostAsync("https://music.163.com/weapi/login/qrcode/client/login", encrypted.GetContent(), token).GetJsonAsync(token).ConfigureAwait(false);
            UnikeyStatus result = root["code"].ToObject<UnikeyStatus>();
            if (!Enum.IsDefined(typeof(UnikeyStatus), result))
            {
                throw new UnknownResponseException(root);
            }
            return result;
        }
    }
}
