using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RSA = OpenSSL.Crypto.RSA;

namespace ExtendNetease_DGJModule.NeteaseMusic
{
    public enum SearchType
    {
        /// <summary>
        /// 单曲
        /// </summary>
        Song = 1,
        /// <summary>
        /// 专辑
        /// </summary>
        Album = 10,
        /// <summary>
        /// 歌手
        /// </summary>
        Artist = 100,
        /// <summary>
        /// 歌单
        /// </summary>
        SongList = 1000,
        /// <summary>
        /// 用户
        /// </summary>
        User = 1002,
        /// <summary>
        /// MV
        /// </summary>
        Movie = 1004,
        /// <summary>
        /// 歌词
        /// </summary>
        Lyric = 1006,
        /// <summary>
        /// 电台
        /// </summary>
        Radio = 1009,
        /// <summary>
        /// 视频
        /// </summary>
        Video = 1014
    }

    public enum Quality
    {
        LowQuality = 128000,
        MediumQuality = 192000,
        HighQuality = 320000,
        SuperQuality = 999000 // Currently DGJ Not Support
    }

    public static class NeteaseMusicApi
    {
        public static string NeteaseMusicApiVersion { get; }

        public static string DefaultUserAgent { get; } = $"DGJModule.NeteaseMusicApi/{NeteaseMusicApiVersion} .NET CLR v4.0.30319";

        static NeteaseMusicApi()
        {
            NeteaseMusicApiVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
        }

        internal static class CryptoHelper
        {
            public static string PublicKey { get; } = "-----BEGIN PUBLIC KEY-----\nMIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDgtQn2JZ34ZC28NWYpAUd98iZ3\n7BUrX/aKzmFbt7clFSs6sXqHauqKWqdtLkF2KexO40H1YTX8z2lSgBBOAxLsvakl\nV8k4cBFK9snQXE9/DDaFt6Rr7iVZMldczhC0JNgTz+SHXT6CBHuX3e9SdB1Ua44o\nncaTWz7OBGLbCiK45wIDAQAB\n-----END PUBLIC KEY-----\n";

            private static byte[] PresetKey { get; } = Encoding.ASCII.GetBytes("0CoJUm6Qyw8W8jud");

            private static byte[] IV { get; } = Encoding.ASCII.GetBytes("0102030405060708");
            /// <summary>
            /// 生成由参数指定的字符集随机组成的具有指定长度的字符串
            /// </summary>
            /// <exception cref="ArgumentException"/>
            /// <exception cref="ArgumentOutOfRangeException"/>
            /// <param name="length">生成字符串的长度</param>
            /// <param name="enableDigits">启用数字生成</param>
            /// <param name="enableUpperCases">启用大写字母生成</param>
            /// <param name="enableLowerCases">启用小写字母生成</param>
            /// <returns>生成的字符串</returns>
            public static string GenerateRandomString(int length, bool enableDigits = true, bool enableUpperCases = true, bool enableLowerCases = true)
            {
                if (length < 1)
                {
                    throw new ArgumentOutOfRangeException("length", length, "长度必须大于0");
                }
                StringBuilder charPool = new StringBuilder(62);
                if (enableDigits)
                {
                    charPool.Append("0123456789");
                }
                if (enableLowerCases)
                {
                    charPool.Append("abcdefghijklmnopqrstuvwxyz");
                }
                if (enableUpperCases)
                {
                    charPool.Append("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
                }
                if (charPool.Length > 0)
                {
                    string chars = charPool.ToString();
                    Random r = new Random();
                    StringBuilder result = new StringBuilder(length);
                    for (int i = 0; i < length; i++)
                    {
                        int rnd = r.Next(chars.Length);
                        result.Append(chars[rnd]);
                    }
                    return result.ToString();
                }
                else
                {
                    throw new ArgumentException("必须启用至少一项字符集的生成");
                }
            }

            public static byte[] AesEncrypt(byte[] toEncrypt, CipherMode mode, byte[] key)
            {
                using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
                {
                    aes.Mode = mode;
                    aes.IV = IV;
                    aes.Key = key;
                    using (MemoryStream ms = new MemoryStream())
                    using (CryptoStream cStream = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cStream.Write(toEncrypt, 0, toEncrypt.Length);
                        cStream.FlushFinalBlock();
                        return ms.ToArray();
                    }
                }
            }

            public static byte[] AesEncrypt(string toEncrypt, CipherMode mode, byte[] key)
                => AesEncrypt(Encoding.UTF8.GetBytes(toEncrypt), mode, key);

            public static byte[] RsaEncrypt(byte[] toEncrypt)
            {
                using (OpenSSL.Core.BIO bio = new OpenSSL.Core.BIO(PublicKey))
                using (RSA rsa = RSA.FromPublicKey(bio))
                {
                    return rsa.PublicEncrypt(toEncrypt, RSA.Padding.None);
                }
            }

            public static string MD5Encrypt(byte[] toEncrypt)
            {
                using (MD5CryptoServiceProvider mD5 = new MD5CryptoServiceProvider())
                {
                    byte[] buffer = mD5.ComputeHash(toEncrypt);
                    return BitConverter.ToString(buffer).Replace("-", "").ToLower();
                }
            }

            public static string MD5Encrypt(string toEncrypt)
                => MD5Encrypt(Encoding.UTF8.GetBytes(toEncrypt));

            public static Encrypted WebApiEncrypt(JObject j)
            {
                string json = j.ToString(0);
                byte[] secretKey = Encoding.ASCII.GetBytes(GenerateRandomString(16));
                byte[] reversedSecretKey = new byte[112].Concat(secretKey.Reverse()).ToArray();
                string @params = Convert.ToBase64String(AesEncrypt(Encoding.ASCII.GetBytes(Convert.ToBase64String(AesEncrypt(json, CipherMode.CBC, PresetKey))), CipherMode.CBC, secretKey));
                string encSecKey = BitConverter.ToString(RsaEncrypt(reversedSecretKey)).Replace("-", "").ToLower();
                return new Encrypted(@params, encSecKey);
            }

            public static Encrypted WebApiEncrypt(IDictionary<string, object> keyValues)
                => WebApiEncrypt(JObject.FromObject(keyValues));

            internal struct Encrypted
            {
                public string Params { get; }
                public string EncSecKey { get; }
                internal Encrypted(string @params, string encSecKey)
                {
                    Params = @params;
                    EncSecKey = encSecKey;
                }
                public string GetFormdata()
                    => $"params={WebUtility.UrlEncode(Params)}&encSecKey={WebUtility.UrlEncode(EncSecKey)}";
            }
        }

        /// <summary>
        /// 根据网易云用户ID获取其生日(若用户未填写,返回DateTime.MinValue)
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>生日</returns>
        public static DateTime GetNeteaseMusicUserBirthDay(int userId)
        {
            JObject data = new JObject(new JProperty("uid", userId), new JProperty("limit", 30), new JProperty("offset", 0));
            CryptoHelper.Encrypted encrypted = CryptoHelper.WebApiEncrypt(data);
            string json = HttpHelper.HttpPost("https://music.163.com/weapi/user/playlist", encrypted.GetFormdata(), userAgent: DefaultUserAgent);
            JObject j = JObject.Parse(json);
            long? birthday = j["playlist"].FirstOrDefault(p => p["creator"]["userId"].ToObject<int>() == userId)?["creator"]["birthday"].ToObject<long>();
            return birthday.HasValue ? Utils.UnixTimeStamp2DateTime(birthday.Value) : default(DateTime);
        }
        /// <summary>
        /// 按给定的信息执行搜索由于类型不定,本方法将返回json
        /// <para>
        /// 由于类型不定,本方法将返回json
        /// </para>
        /// </summary>
        /// <param name="keyWords">关键词</param>
        /// <param name="type">搜索类型</param>
        /// <param name="pageSize">返回的json中,实体个数上限</param>
        /// <param name="offset">偏移量</param>
        /// <returns>服务器返回的Json</returns>
        public static string Search(string keyWords, SearchType type, int pageSize = 30, int offset = 0)
        {
            IDictionary<string, object> data = new Dictionary<string, object>
            {
                ["s"] = keyWords,
                ["type"] = (int)type,
                ["limit"] = pageSize,
                ["offset"] = offset
            };
            CryptoHelper.Encrypted encrypted = CryptoHelper.WebApiEncrypt(data);
            string json = HttpHelper.HttpPost("https://music.163.com/weapi/search/get", encrypted.GetFormdata(), userAgent: DefaultUserAgent);
            return json;
        }
        /// <summary>
        /// 按给定的关键词搜索单曲
        /// </summary>
        /// <param name="keyWords">关键词</param>
        /// <param name="pageSize">本次搜索返回的实例个数上限</param>
        /// <param name="offset">偏移量</param>
        public static SongInfo[] SearchSongs(string keyWords, int pageSize = 30, int offset = 0)
        {
            string json = Search(keyWords, SearchType.Song, 30, 0);
            JObject j = JObject.Parse(json);
            if (j["code"].ToObject<int>() == 200)
            {
                SongInfo[] result = j["result"]["songs"].Select(p => new SongInfo(p)).ToArray();
                IDictionary<long, bool> canPlayDic = CheckMusicStatus(songIds: result.Select(p => p.Id).ToArray());
                foreach (SongInfo song in result)
                {
                    if (canPlayDic.TryGetValue(song.Id, out bool canPlay))
                    {
                        song.CanPlay = canPlay;
                    }
                }
                return result;
            }
            else
            {
                NotImplementedException exception = new NotImplementedException($"未知的服务器返回");
                exception.Data.Add("Response", j.ToString());
                throw exception;
            }
        }
        /// <summary>
        /// 检查给定ID对应的音乐有无版权
        /// </summary>
        /// <param name="bitRate">比特率</param>
        /// <param name="songIds">音乐IDs</param>
        /// <returns></returns>
        public static IDictionary<long, bool> CheckMusicStatus(int bitRate = 999000, params long[] songIds)
        {
            IDictionary<string, object> data = new Dictionary<string, object>
            {
                ["ids"] = songIds,
                ["br"] = bitRate
            };
            CryptoHelper.Encrypted encrypted = CryptoHelper.WebApiEncrypt(data);
            string json = HttpHelper.HttpPost("https://music.163.com/weapi/song/enhance/player/url", encrypted.GetFormdata(), userAgent: DefaultUserAgent);
            JObject j = JObject.Parse(json);
            if (j["code"].ToObject<int>() == 200)
            {
                return j["data"].ToDictionary(p => p["id"].ToObject<long>(), p => p["code"].ToObject<int>() == 200);
            }
            else
            {
                NotImplementedException exception = new NotImplementedException($"未知的服务器返回");
                exception.Data.Add("Response", j.ToString());
                throw exception;
            }
        }
        /// <summary>
        /// 批量获取单曲下载链接
        /// </summary>
        /// <param name="bitRate">比特率上限</param>
        /// <param name="songIds">单曲IDs</param>
        public static IDictionary<long, DownloadSongInfo> GetSongsUrl(Quality bitRate = Quality.SuperQuality, params long[] songIds)
            => GetSongsUrl(null, bitRate, songIds);
        /// <summary>
        /// 批量获取单曲下载链接
        /// </summary>
        /// <param name="bitRate">比特率上限</param>
        /// <param name="songIds">单曲IDs</param>
        public static IDictionary<long, DownloadSongInfo> GetSongsUrl(NeteaseSession session, Quality bitRate = Quality.SuperQuality, params long[] songIds)
        {
            IDictionary<string, object> data = new Dictionary<string, object>
            {
                ["ids"] = songIds,
                ["br"] = bitRate
            };
            CryptoHelper.Encrypted encrypted = CryptoHelper.WebApiEncrypt(data);
            string json = session == null ? HttpHelper.HttpPost("https://music.163.com/weapi/song/enhance/player/url", encrypted.GetFormdata(), userAgent: DefaultUserAgent) :
                session.Session.HttpPost("https://music.163.com/weapi/song/enhance/player/url", encrypted.GetFormdata(), userAgent: DefaultUserAgent);
            JObject j = JObject.Parse(json);
            if (j["code"].ToObject<int>() == 200)
            {
                return j["data"].ToDictionary(p => p["id"].ToObject<long>(), p => new DownloadSongInfo(p["id"].ToObject<int>(), p["br"].ToObject<int>(), p["url"].ToString(), p["type"].ToString()));
            }
            else
            {
                NotImplementedException exception = new NotImplementedException($"未知的服务器返回");
                exception.Data.Add("Response", j.ToString());
                throw exception;
            }
        }
        /// <summary>
        /// 获取给定单曲ID的歌词
        /// </summary>
        /// <exception cref="NotImplementedException"/>
        /// <param name="songId">单曲ID</param>
        public static LyricInfo GetLyric(long songId)
        {
            IDictionary<string, object> data = new Dictionary<string, object>
            {
                ["csrf_token"] = "",
                ["id"] = songId.ToString(),
                ["lv"] = -1,
                ["tv"] = -1
            };
            CryptoHelper.Encrypted encrypted = CryptoHelper.WebApiEncrypt(data);
            string json = HttpHelper.HttpPost("https://music.163.com/weapi/song/lyric", encrypted.GetFormdata(), userAgent: DefaultUserAgent);
            JObject j = JObject.Parse(json);
            if (j["code"].ToObject<int>() == 200)
            {
                string lyricText = j["lrc"]?["lyric"].ToString();
                string translatedLyricText = j["tlyric"]?["lyric"].ToString();
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
            else
            {
                NotImplementedException exception = new NotImplementedException($"未知的服务器返回");
                exception.Data.Add("Response", j.ToString());
                throw exception;
            }
        }
        /// <summary>
        /// 获取歌单内的所有单曲
        /// </summary>
        /// <exception cref="NotImplementedException"/>
        /// <param name="id">歌单Id</param>
        public static SongInfo[] GetPlayList(long id)
        {
            IDictionary<string, object> data = new Dictionary<string, object>()
            {
                ["id"] = id,
                ["n"] = 100000,
                ["s"] = 8
            };
            CryptoHelper.Encrypted encrypted = CryptoHelper.WebApiEncrypt(data);
            string json = HttpHelper.HttpPost("https://music.163.com/weapi/v3/playlist/detail", encrypted.GetFormdata(), userAgent: DefaultUserAgent);
            JObject j = JObject.Parse(json);
            if (j["code"].ToObject<int>() == 200)
            {
                SongInfo[] result = j["playlist"]["tracks"].Select(p => new SongInfo(p)).ToArray();
                IDictionary<long, bool> canPlayDic = CheckMusicStatus(songIds: result.Select(p => p.Id).ToArray());
                foreach (SongInfo song in result)
                {
                    if (canPlayDic.TryGetValue(song.Id, out bool canPlay))
                    {
                        song.CanPlay = canPlay;
                    }
                }
                return result;
            }
            else
            {
                NotImplementedException exception = new NotImplementedException($"未知的服务器返回");
                exception.Data.Add("Response", j.ToString());
                throw exception;
            }
        }
    }

    public class NeteaseSession : INotifyPropertyChanged
    {
        public HttpSession Session { get; }
        private string _UserName;
        /// <summary>
        /// 用户昵称
        /// </summary>
        public string UserName { get => _UserName; private set { if (_UserName != value) { _UserName = value; OnPropertyChanged(); } } }
        private int _UserId;
        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId { get => _UserId; private set { if (_UserId != value) { _UserId = value; OnPropertyChanged(); } } }
        /// <summary>
        /// VIP类型(Todo:做成Enum)
        /// </summary>
        public int VipType { get; private set; }
        private bool _LoginStatus;
        /// <summary>
        /// 登录状态
        /// </summary>
        public bool LoginStatus { get => _LoginStatus; private set { if (_LoginStatus != value) { _LoginStatus = value; OnPropertyChanged(); } } }
        /// <summary>
        /// 初始化 <see cref="NeteaseSession"/> 类的新实例
        /// </summary>
        public NeteaseSession()
        {
            Session = new HttpSession();
        }
        /// <summary>
        /// 使用已有的Cookie字符串初始化 <see cref="NeteaseSession"/> 类的新实例
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <param name="cookieString">一个有效的Cookie字符串</param>
        public NeteaseSession(string cookieString) : this()
        {
            Login(cookieString);
        }
        /// <summary>
        /// 使用邮箱和密码登录
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <param name="mail">邮箱地址</param>
        /// <param name="password">密码</param>
        public void Login(string mail, string password)
        {
            /*
             * query.cookie.os = 'pc'
                const data = {
                    username: query.email,
                    password: crypto.createHash('md5').update(query.password).digest('hex'),
                    rememberLogin: 'true'
                }
                return request(
                    'POST', `https://music.163.com/weapi/login`, data,
                    {crypto: 'weapi', ua: 'pc', cookie: query.cookie, proxy: query.proxy}
                )
             */
            PresetLoginCookie();
            JObject data = new JObject(new JProperty("username", mail), new JProperty("password", NeteaseMusicApi.CryptoHelper.MD5Encrypt(password)),
                new JProperty("rememberLogin", true));
            NeteaseMusicApi.CryptoHelper.Encrypted encrypted = NeteaseMusicApi.CryptoHelper.WebApiEncrypt(data);
            string json = Session.HttpPost("https://music.163.com/weapi/login", encrypted.GetFormdata(), userAgent: NeteaseMusicApi.DefaultUserAgent);
            HandleLoginResult(json);
        }
        /// <summary>
        /// 使用手机号、国家代码和密码登录
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <param name="countryCode">国家代码,如86(中国)</param>
        /// <param name="phone">手机号码</param>
        /// <param name="password">密码</param>
        public void Login(int countryCode, long phone, string password)
        {
            PresetLoginCookie();
            JObject data = new JObject(new JProperty("phone", phone), new JProperty("countrycode", countryCode),
                new JProperty("password", NeteaseMusicApi.CryptoHelper.MD5Encrypt(password)), new JProperty("rememberLogin", true));
            NeteaseMusicApi.CryptoHelper.Encrypted encrypted = NeteaseMusicApi.CryptoHelper.WebApiEncrypt(data);
            string json = Session.HttpPost("https://music.163.com/weapi/login/cellphone", encrypted.GetFormdata(), userAgent: NeteaseMusicApi.DefaultUserAgent);
            HandleLoginResult(json);
        }
        /// <summary>
        /// 使用已有的Cookie字符串登录
        /// </summary>
        /// <param name="cookie">一个有效的Cookie字符串</param>
        public void Login(string cookieString)
        {
            Session.Reset();
            foreach (string splited in cookieString.Split(new string[] { "; " }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] nameValue = splited.Split('=');
                Cookie cookie = new Cookie(nameValue[0].Trim(), nameValue.Length > 1 ? nameValue[1].Trim() : "", "/", ".music.163.com");
                Session.Cookie.Add(cookie);
            }
            try
            {
                CheckLoginStatus();
            }
            catch (InvalidOperationException)
            {
                Session.Reset();
                throw new ArgumentException("给定的Cookie无效");
            }
        }
        /// <summary>
        /// 使用邮箱和密码异步登录
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <param name="mail">邮箱地址</param>
        /// <param name="password">密码</param>
        public async Task LoginAsync(string mail, string password)
        {
            PresetLoginCookie();
            JObject data = new JObject(new JProperty("username", mail), new JProperty("password", NeteaseMusicApi.CryptoHelper.MD5Encrypt(password)),
                new JProperty("rememberLogin", true));
            NeteaseMusicApi.CryptoHelper.Encrypted encrypted = NeteaseMusicApi.CryptoHelper.WebApiEncrypt(data);
            string json = await Session.HttpPostAsync("https://music.163.com/weapi/login", encrypted.GetFormdata(), userAgent: NeteaseMusicApi.DefaultUserAgent);
            HandleLoginResult(json);
        }
        /// <summary>
        /// 使用手机号、国家代码和密码异步登录
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <param name="countryCode">国家代码,如86(中国)</param>
        /// <param name="phone">手机号码</param>
        /// <param name="password">密码</param>
        public async Task LoginAsync(int countryCode, long phone, string password)
        {
            PresetLoginCookie();
            JObject data = new JObject(new JProperty("phone", phone), new JProperty("countrycode", countryCode),
                new JProperty("password", NeteaseMusicApi.CryptoHelper.MD5Encrypt(password)), new JProperty("rememberLogin", true));
            NeteaseMusicApi.CryptoHelper.Encrypted encrypted = NeteaseMusicApi.CryptoHelper.WebApiEncrypt(data);
            string json = await Session.HttpPostAsync("https://music.163.com/weapi/login/cellphone", encrypted.GetFormdata(), userAgent: NeteaseMusicApi.DefaultUserAgent);
            HandleLoginResult(json);
        }
        /// <summary>
        /// 使用已有的Cookie字符串异步登录
        /// </summary>
        /// <param name="cookie">一个有效的Cookie字符串</param>
        public async Task LoginAsync(string cookieString)
        {
            Session.Reset();
            foreach (string splited in cookieString.Split(new string[] { "; " }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] nameValue = splited.Split('=');
                Cookie cookie = new Cookie(nameValue[0].Trim(), nameValue.Length > 1 ? nameValue[1].Trim() : "", "/", ".music.163.com");
                Session.Cookie.Add(cookie);
            }
            try
            {
                await CheckLoginStatusAsync();
            }
            catch (InvalidOperationException)
            {
                throw new ArgumentException("给定的Cookie无效");
            }
        }
        /// <summary>
        /// 登出
        /// </summary>
        /// <exception cref="NotImplementedException"/>
        public void LogOut()
        {
            IDictionary<string, object> data = new Dictionary<string, object>
            {
                ["csrf_token"] = GetCsrfToken(),
            };
            NeteaseMusicApi.CryptoHelper.Encrypted encrypted = NeteaseMusicApi.CryptoHelper.WebApiEncrypt(data);
            try
            {
                string json = Session.HttpPost("https://music.163.com/weapi/logout", encrypted.GetFormdata(), userAgent: NeteaseMusicApi.DefaultUserAgent);
                JObject j = JObject.Parse(json);
                if (j["code"].ToObject<int>() != 0)
                {
                    NotImplementedException exception = new NotImplementedException($"未知的服务器返回");
                    exception.Data.Add("Response", j.ToString());
                    throw exception;
                }
            }
            finally
            {
                Session.Reset();
            }
        }
        /// <summary>
        /// 异步登出
        /// </summary>
        /// <exception cref="NotImplementedException"/>
        public async Task LogOutAsync()
        {
            IDictionary<string, object> data = new Dictionary<string, object>
            {
                ["csrf_token"] = GetCsrfToken(),
            };
            NeteaseMusicApi.CryptoHelper.Encrypted encrypted = NeteaseMusicApi.CryptoHelper.WebApiEncrypt(data);
            try
            {
                string json = await Session.HttpPostAsync("https://music.163.com/weapi/logout", encrypted.GetFormdata(), userAgent: NeteaseMusicApi.DefaultUserAgent);
                JObject j = JObject.Parse(json);
                if (j["code"].ToObject<int>() != 0)
                {
                    NotImplementedException exception = new NotImplementedException($"未知的服务器返回");
                    exception.Data.Add("Response", j.ToString());
                    throw exception;
                }
            }
            finally
            {
                Session.Reset();
            }
        }
        /// <summary>
        /// 刷新用户信息
        /// </summary>
        /// <exception cref="NotImplementedException"/>
        public void GetUserInfo()
        {
            JObject data = new JObject(new JProperty("csrf_token", GetCsrfToken()));
            var encrypted = NeteaseMusicApi.CryptoHelper.WebApiEncrypt(data);
            string json = Session.HttpPost($"https://music.163.com/weapi/v1/user/detail/{UserId}", encrypted.GetFormdata(), userAgent: NeteaseMusicApi.DefaultUserAgent);
            JObject j = JObject.Parse(json);
            if (j["code"].ToObject<int>() == 200)
            {
                UserName = j["profile"]["nickname"].ToString();
                VipType = j["profile"]["vipType"].ToObject<int>();
            }
            else
            {
                throw new NotImplementedException($"未知的服务器返回:{j.ToString(0)}");
            }
            ;
        }
        /// <summary>
        /// 检查登录状态
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        public void CheckLoginStatus()
        {
            string html = Session.HttpGet("https://music.163.com/", userAgent: NeteaseMusicApi.DefaultUserAgent);
            string fakeJson = Regex.Match(html, @"GUser\s*=\s*([^;]+);").Groups[1].Value;
            if (string.IsNullOrEmpty(fakeJson) || fakeJson == "{}")
            {
                LoginStatus = false;
                throw new InvalidOperationException("实例中的Cookie无效");
            }
            else
            {
                UserId = int.Parse(Regex.Match(fakeJson, @"userId:(\d+)").Groups[1].Value);
                UserName = Regex.Match(fakeJson, @"nickname:""(.*?)""").Groups[1].Value;
                LoginStatus = true;
            }
        }
        /// <summary>
        /// 异步检查登录状态
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        public async Task CheckLoginStatusAsync()
        {
            string html = await Session.HttpGetAsync("https://music.163.com/", userAgent: NeteaseMusicApi.DefaultUserAgent);
            string fakeJson = Regex.Match(html, @"GUser\s*=\s*([^;]+);").Groups[1].Value;
            if (string.IsNullOrEmpty(fakeJson) || fakeJson == "{}")
            {
                LoginStatus = false;
                throw new InvalidOperationException("实例中的Cookie无效");
            }
            else
            {
                UserId = int.Parse(Regex.Match(fakeJson, @"userId:(\d+)").Groups[1].Value);
                UserName = Regex.Match(fakeJson, @"nickname:""(.*?)""").Groups[1].Value;
                LoginStatus = true;
            }
        }
        /// <summary>
        /// 获取Csrf_Token
        /// </summary>
        /// <returns>Csrf_Token字符串</returns>
        private string GetCsrfToken()
            => Session.Cookie.GetCookies(new Uri("http://music.163.com/")).OfType<Cookie>().FirstOrDefault(p => p.Name == "__csrf")?.Value;
        /// <summary>
        /// 给Session加上os=pc的Cookie
        /// </summary>
        private void PresetLoginCookie()
        {
            CookieCollection cookies = Session.Cookie.GetCookies(new Uri("https://music.163.com/weapi/login/cellphone"));
            Cookie osCookie = cookies.OfType<Cookie>().FirstOrDefault(p => p.Name == "os");
            if (osCookie == null)
            {
                Session.Cookie.Add(new Cookie("os", "pc", "/", ".music.163.com"));
            }
            else if (osCookie.Value != "pc")
            {
                osCookie.Value = "pc";
            }
        }
        /// <summary>
        /// 处理登录回复
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="NotImplementedException"/>
        /// <param name="json">服务器返回的Json</param>
        private void HandleLoginResult(string json)
        {
            JObject j = JObject.Parse(json);
            int code = j["code"].ToObject<int>();
            switch (code)
            {
                case 200:
                    {
                        UserName = j["profile"]["nickname"].ToString();
                        UserId = j["profile"]["userId"].ToObject<int>();
                        VipType = j["profile"]["vipType"].ToObject<int>();
                        LoginStatus = true;
                        break;
                    }
                case 501:
                case 502:
                case 509:
                    {
                        LoginStatus = false;
                        throw new ArgumentException(j["msg"].ToString());
                    }
                default:
                    {
                        LoginStatus = false;
                        NotImplementedException exception = new NotImplementedException($"未知的服务器返回");
                        exception.Data.Add("Response", j.ToString());
                        throw exception;
                    }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName]string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class SongInfo
    {
        public long Id { get; }
        public string Name { get; }
        public ArtistInfo[] Artists { get; }
        public AlbumInfo Album { get; }
        public TimeSpan Duration { get; }
        public bool CanPlay { get; set; }
        public bool NeedPaymentToDownload { get; } // Fee == 8 | > 0
        public SongInfo(int id, string name, ArtistInfo[] artists, AlbumInfo album, TimeSpan duration, bool needPaymentToDownload)
        {
            Id = id;
            Name = name;
            Artists = artists;
            Album = album;
            Duration = duration;
            NeedPaymentToDownload = needPaymentToDownload;
        }
        public SongInfo(JToken jt) : this(jt["id"].ToObject<int>(), jt["name"].ToString(), (jt["artists"] ?? jt["ar"]).Select(p => new ArtistInfo(p)).ToArray(), new AlbumInfo(jt["album"] ?? jt["al"]), TimeSpan.FromMilliseconds((jt["duration"] ?? jt["dt"]).ToObject<int>()), jt["fee"].ToObject<bool>())
        {

        }
    }

    public class ArtistInfo
    {
        public long Id { get; }
        public string Name { get; }
        public ArtistInfo(int id, string name)
        {
            Id = id;
            Name = name;
        }
        public ArtistInfo(JToken jt) : this (jt["id"].ToObject<int>(), jt["name"].ToString())
        {

        }
    }

    public class AlbumInfo
    {
        public long Id { get; }
        public string Name { get; }
        public DateTime PublishTime { get; }
        public int Count { get; }
        public AlbumInfo(int id, string name, DateTime publishTime, int count)
        {
            Id = id;
            Name = name;
            PublishTime = publishTime;
            Count = count;
        }
        public AlbumInfo(JToken jt) : this(jt["id"].ToObject<int>(), jt["name"].ToString(), Utils.UnixTimeStamp2DateTime(jt["publishTime"]?.ToObject<long>() ?? 0), jt["size"]?.ToObject<int>() ?? 0)
        {

        }
    }

    public class DownloadSongInfo
    {
        public long Id { get; }
        public int Bitrate { get; }
        public string Url { get; }
        public string Type { get; }
        public DownloadSongInfo(int id, int bitrate, string url, string type)
        {
            Id = id;
            Bitrate = bitrate;
            Url = url;
            Type = type;
        }
        public DownloadSongInfo(JToken jt) : this(jt["id"].ToObject<int>(), jt["br"].ToObject<int>(), jt["url"].ToString(), jt["type"].ToString())
        {
            
        }
    }

    public class LyricInfo
    {
        public string Title { get; private set; }

        public string Artist { get; private set; }

        public string Album { get; private set; }

        public string LrcBy { get; private set; }

        public int Offset { get; private set; }

        public IDictionary<double, string> LrcWord { get; }

        public LyricInfo()
        {
            LrcWord = new SortedDictionary<double, string>();
        }

        public LyricInfo(string lyricText) : this()
            => AppendLrc(lyricText);

        public int GetCurrentLyric(double seconds, out string current, out string upcoming)
        {
            if (LrcWord.Count < 1)
            {
                current = "无歌词";
                upcoming = string.Empty;
                return -1;
            }
            List<KeyValuePair<double, string>> list = LrcWord.ToList();
            int i;
            if (seconds < list[0].Key)
            {
                i = 0;
                current = string.Empty;
                upcoming = list[0].Value;
            }
            else
            {
                for (i = 1; i < LrcWord.Count && !(seconds < list[i].Key); i++)
                {
                }
                current = list[i - 1].Value;
                if (list.Count > i)
                {
                    upcoming = list[i].Value;
                }
                else
                {
                    upcoming = string.Empty;
                }
            }
            return i;
        }

        public string GetLyricText()
        {
            StringBuilder lyric = new StringBuilder();
            if (!string.IsNullOrEmpty(Title))
            {
                lyric.AppendLine($"[ti:{Title}]");
            }
            if (!string.IsNullOrEmpty(Artist))
            {
                lyric.AppendLine($"[ar:{Artist}]");
            }
            if (!string.IsNullOrEmpty(Album))
            {
                lyric.AppendLine($"[al:{Album}]");
            }
            if (!string.IsNullOrEmpty(LrcBy))
            {
                lyric.AppendLine($"[by:{LrcBy}]");
            }
            if (Offset != 0)
            {
                lyric.AppendLine($"[offset:{Offset}]");
            }
            lyric.Append(string.Join(Environment.NewLine, LrcWord.GroupBy(p => p.Value).Select(p => $"{string.Join("", p.Select(q => $"[{TimeSpan.FromSeconds(q.Key).ToString(@"mm\:ss\.ff")}]"))}{p.Key}")));
            return lyric.ToString();
        }

        public void AppendLrc(string lrcText)
        {
            string[] array = lrcText.Split(new string[2]
            {
                "\r\n",
                "\n"
            }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string text in array)
            {
                if (text.StartsWith("[ti:"))
                {
                    Title = SplitInfo(text);
                }
                else if (text.StartsWith("[ar:"))
                {
                    Artist = SplitInfo(text);
                }
                else if (text.StartsWith("[al:"))
                {
                    Album = SplitInfo(text);
                }
                else if (text.StartsWith("[by:"))
                {
                    LrcBy = SplitInfo(text);
                }
                else if (text.StartsWith("[offset:"))
                {
                    Offset = int.Parse(SplitInfo(text));
                }
                else
                {
                    try
                    {
                        string value = new Regex(".*\\](.*)").Match(text).Groups[1].Value;
                        if (!(value.Replace(" ", "") == ""))
                        {
                            foreach (Match item in new Regex("\\[([0-9.:]*)\\]", RegexOptions.Compiled).Matches(text))
                            {
                                double totalSeconds = TimeSpan.Parse("00:" + item.Groups[1].Value).TotalSeconds + Offset / 1000;
                                if (LrcWord.ContainsKey(totalSeconds))
                                {
                                    LrcWord[totalSeconds] += $"({value})";
                                }
                                else
                                {
                                    LrcWord[totalSeconds] = value;
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }
        
        private static string SplitInfo(string line)
        {
            return line.Substring(line.IndexOf(":") + 1).TrimEnd(']');
        }
    }
}
