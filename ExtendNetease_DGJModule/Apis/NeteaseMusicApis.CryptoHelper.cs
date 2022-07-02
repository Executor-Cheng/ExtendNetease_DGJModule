using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

#nullable enable
namespace ExtendNetease_DGJModule.Apis
{
    public static partial class NeteaseMusicApis
    {
        internal static class CryptoHelper
        {
            public static string PublicKey { get; } = "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDgtQn2JZ34ZC28NWYpAUd98iZ3\n7BUrX/aKzmFbt7clFSs6sXqHauqKWqdtLkF2KexO40H1YTX8z2lSgBBOAxLsvakl\nV8k4cBFK9snQXE9/DDaFt6Rr7iVZMldczhC0JNgTz+SHXT6CBHuX3e9SdB1Ua44o\nncaTWz7OBGLbCiK45wIDAQAB";

            private static byte[] PresetKey { get; } = Encoding.ASCII.GetBytes("0CoJUm6Qyw8W8jud");

            private static byte[] IV { get; } = Encoding.ASCII.GetBytes("0102030405060708");

            private static byte[] EapiKey { get; } = Encoding.ASCII.GetBytes("e82ckenh8dichen8");

            private static readonly IBufferedCipher RsaEncrypter = CipherUtilities.GetCipher("RSA/ECB/NoPadding");

            static CryptoHelper()
            {
                AsymmetricKeyParameter publicKey = PublicKeyFactory.CreateKey(Convert.FromBase64String(PublicKey));
                RsaEncrypter.Init(true, publicKey);
            }

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
                throw new ArgumentException("必须启用至少一项字符集的生成");
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
                return RsaEncrypter.DoFinal(toEncrypt);
            }

            public static string MD5Encrypt(byte[] toEncrypt)
            {
                using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
                {
                    byte[] buffer = md5.ComputeHash(toEncrypt);
                    return BitConverter.ToString(buffer).Replace("-", "").ToLower();
                }
            }

            public static string MD5Encrypt(string toEncrypt)
                => MD5Encrypt(Encoding.UTF8.GetBytes(toEncrypt));

            public static WebApiEncryptedData WebApiEncrypt(JObject j)
            {
                string json = j.ToString(0);
                byte[] secretKey = Encoding.ASCII.GetBytes(GenerateRandomString(16));
                byte[] reversedSecretKey = new byte[112].Concat(secretKey.Reverse()).ToArray();
                string @params = Convert.ToBase64String(AesEncrypt(Encoding.ASCII.GetBytes(Convert.ToBase64String(AesEncrypt(json, CipherMode.CBC, PresetKey))), CipherMode.CBC, secretKey));
                string encSecKey = BitConverter.ToString(RsaEncrypt(reversedSecretKey)).Replace("-", "").ToLower();
                return new WebApiEncryptedData(@params, encSecKey);
            }

            public static WebApiEncryptedData WebApiEncrypt(IDictionary<string, object> keyValues)
                => WebApiEncrypt(JObject.FromObject(keyValues));

            public static EApiEncryptedData EApiEncrypt(JObject j, string url)
            {
                string json = j.ToString(0);
                string message = $"nobody{url}use{json}md5forencrypt";
                string digest = MD5Encrypt(message);
                string data = $"{url}-36cd479b6b5-{json}-36cd479b6b5-{digest}";
                return new EApiEncryptedData(BitConverter.ToString(AesEncrypt(data, CipherMode.ECB, EapiKey)).Replace("-", "").ToLower());
            }

            public static EApiEncryptedData EApiEncrypt(IDictionary<string, object> keyValues, string url) 
                => EApiEncrypt(JObject.FromObject(keyValues), url);

            public struct WebApiEncryptedData
            {
                public string Params { get; }
                public string EncSecKey { get; }
                public WebApiEncryptedData(string @params, string encSecKey)
                {
                    Params = @params;
                    EncSecKey = encSecKey;
                }
                public FormUrlEncodedContent GetContent()
                {
                    KeyValuePair<string?, string?>[] payload = new KeyValuePair<string?, string?>[2]
                    {
                        new KeyValuePair<string?, string?>("params", Params),
                        new KeyValuePair<string?, string?>("encSecKey", EncSecKey),
                    };
                    return new FormUrlEncodedContent(payload);
                }
            }

            public struct EApiEncryptedData
            {
                public string Params { get; }
                public EApiEncryptedData(string @params)
                {
                    Params = @params;
                }
                public FormUrlEncodedContent GetContent()
                {
                    KeyValuePair<string?, string?>[] payload = new KeyValuePair<string?, string?>[1]
                    {
                        new KeyValuePair<string?, string?>("params", Params)
                    };
                    return new FormUrlEncodedContent(payload);
                }
            }
        }
    }
}
