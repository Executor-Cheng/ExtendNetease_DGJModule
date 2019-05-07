using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ExtendNetease_DGJModule
{
    public static class HttpHelper
    {
        public const string UserAgent = "DGJModule.NeteaseMusicApi/1.0.2 .NET CLR v4.0.30319";
        public static string HttpGet(string url, int timeout = 0, string userAgent = UserAgent, string cookie = null, IDictionary<string, string> headers = null)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Accept = "*/*";
            request.Method = "GET";
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            if (timeout != 0) { request.Timeout = timeout * 1000; request.ReadWriteTimeout = timeout * 1000; }
            else request.ReadWriteTimeout = 10000;
            request.UserAgent = userAgent;
            if (!string.IsNullOrEmpty(cookie))
            {
                request.Headers.Add("Cookie", cookie);
            }
            if (headers != null)
            {
                foreach (string key in headers.Keys)
                {
                    if (key.ToLower() == "accept")
                        request.Accept = headers[key];
                    else if (key.ToLower() == "host")
                        request.Host = headers[key];
                    else if (key.ToLower() == "referer")
                        request.Referer = headers[key];
                    else
                        request.Headers.Add(key, headers[key]);
                }
            }
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                return reader.ReadToEnd();
        }
        public static async Task<string> HttpGetAsync(string url, int timeout = 0, string userAgent = UserAgent, string cookie = null, IDictionary<string, string> headers = null)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.ProtocolVersion = HttpVersion.Version11;
            request.Accept = "*/*";
            request.Method = "GET";
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            if (timeout != 0) { request.Timeout = timeout * 1000; request.ReadWriteTimeout = timeout * 1000; }
            else request.ReadWriteTimeout = 10000;
            request.UserAgent = userAgent;
            if (!string.IsNullOrEmpty(cookie))
            {
                request.Headers.Add("Cookie", cookie);
            }
            if (headers != null)
            {
                foreach (string key in headers.Keys)
                {
                    if (key.ToLower() == "accept")
                        request.Accept = headers[key];
                    else if (key.ToLower() == "host")
                        request.Host = headers[key];
                    else if (key.ToLower() == "referer")
                        request.Referer = headers[key];
                    else
                        request.Headers.Add(key, headers[key]);
                }
            }
            using (HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse)
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                return await reader.ReadToEndAsync();
        }
        public static string HttpGetV2(string url, int timeout = 0, string userAgent = UserAgent, string cookie = null, IDictionary<string, string> headers = null, bool ignoreWebException = false)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Accept = "*/*";
            request.Method = "GET";
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            if (timeout != 0) { request.Timeout = timeout * 1000; request.ReadWriteTimeout = timeout * 1000; }
            else request.ReadWriteTimeout = 10000;
            request.UserAgent = userAgent;
            if (!string.IsNullOrEmpty(cookie))
            {
                request.Headers.Add("Cookie", cookie);
            }
            if (headers != null)
            {
                foreach (string key in headers.Keys)
                {
                    if (key.ToLower() == "accept")
                        request.Accept = headers[key];
                    else if (key.ToLower() == "host")
                        request.Host = headers[key];
                    else if (key.ToLower() == "referer")
                        request.Referer = headers[key];
                    else
                        request.Headers.Add(key, headers[key]);
                }
            }
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                    return reader.ReadToEnd();
            }
            catch (WebException Ex) when (ignoreWebException)
            {
                if (Ex.Response == null) throw;
                using (HttpWebResponse response = (HttpWebResponse)Ex.Response)
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                    return reader.ReadToEnd();
            }
        }
        public static string HttpPost(string url, byte[] buffer, int timeout = 0, string userAgent = UserAgent, string cookie = null, IDictionary<string, string> headers = null)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Accept = "*/*";
            request.Method = "POST";
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.ContentType = "application/x-www-form-urlencoded";
            request.UserAgent = userAgent;
            if (timeout != 0) { request.Timeout = timeout * 1000; request.ReadWriteTimeout = timeout * 1000; }
            else request.ReadWriteTimeout = 10000;
            if (!string.IsNullOrEmpty(cookie))
            {
                request.Headers.Add("Cookie", cookie);
            }
            if (headers != null)
            {
                foreach (string key in headers.Keys)
                {
                    if (key.ToLower() == "accept")
                        request.Accept = headers[key];
                    else if (key.ToLower() == "host")
                        request.Host = headers[key];
                    else if (key.ToLower() == "referer")
                        request.Referer = headers[key];
                    else if (key.ToLower() == "content-type")
                        request.ContentType = headers[key];
                    else
                        request.Headers.Add(key, headers[key]);
                }
            }
            if (buffer?.Length > 0)
            {
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(buffer, 0, buffer.Length);
                }
            }
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                return reader.ReadToEnd();
        }
        public static string HttpPost(string url, string formdata, int timeout = 0, string userAgent = UserAgent, string cookie = null, IDictionary<string, string> headers = null)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(formdata);
            return HttpPost(url, buffer, timeout, userAgent, cookie, headers);
        }
        public static string HttpPost(string url, IDictionary<string, object> parameters = null, int timeout = 0, string userAgent = UserAgent, string cookie = null, IDictionary<string, string> headers = null)
        {
            string formdata = string.Join("&", parameters.Select(p => $"{p.Key}={p.Value}"));
            return HttpPost(url, formdata, timeout, userAgent, cookie, headers);
        }
        public static async Task<string> HttpPostAsync(string url, string formdata = null, int timeout = 0, string userAgent = UserAgent, string cookie = null, IDictionary<string, string> headers = null)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Accept = "*/*";
            request.Method = "POST";
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.ContentType = "application/x-www-form-urlencoded";
            request.UserAgent = userAgent;
            if (timeout != 0) { request.Timeout = timeout * 1000; request.ReadWriteTimeout = timeout * 1000; }
            else request.ReadWriteTimeout = 10000;
            if (!string.IsNullOrEmpty(cookie))
            {
                request.Headers.Add("Cookie", cookie);
            }
            if (headers != null)
            {
                foreach (string key in headers.Keys)
                {
                    if (key.ToLower() == "accept")
                        request.Accept = headers[key];
                    else if (key.ToLower() == "host")
                        request.Host = headers[key];
                    else if (key.ToLower() == "referer")
                        request.Referer = headers[key];
                    else if (key.ToLower() == "content-type")
                        request.ContentType = headers[key];
                    else
                        request.Headers.Add(key, headers[key]);
                }
            }
            if (!string.IsNullOrEmpty(formdata))
            {
                byte[] data = Encoding.UTF8.GetBytes(formdata);
                using (Stream stream = await request.GetRequestStreamAsync())
                {
                    await stream.WriteAsync(data, 0, data.Length);
                }
            }
            using (HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse)
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                return await reader.ReadToEndAsync();
        }
        public static async Task<string> HttpPostAsync(string url, IDictionary<string, object> parameters = null, int timeout = 0, string userAgent = UserAgent, string cookie = null, IDictionary<string, string> headers = null)
        {
            string formdata = string.Join("&", parameters.Select(p => $"{p.Key}={p.Value}"));
            return await HttpPostAsync(url, formdata, timeout, userAgent, cookie, headers);
        }
        public static void HttpDownloadFile(string url, string path, int timeout = 0, string userAgent = UserAgent, string cookie = null, IDictionary<string, string> headers = null)
        {
            byte[] buffer = HttpDownloadFile(url, timeout, userAgent, cookie, headers);
            using (Stream stream = new FileStream(path, FileMode.Create))
            {
                stream.Write(buffer, 0, buffer.Length);
            }
        }
        public static byte[] HttpDownloadFile(string url, int timeout = 0, string userAgent = UserAgent, string cookie = null, IDictionary<string, string> headers = null)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Accept = "*/*";
            request.Method = "GET";
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            if (timeout != 0) { request.Timeout = timeout * 1000; request.ReadWriteTimeout = timeout * 1000; }
            else request.ReadWriteTimeout = 10000;
            request.UserAgent = userAgent;
            if (!string.IsNullOrEmpty(cookie))
            {
                request.Headers.Add("Cookie", cookie);
            }
            if (headers != null)
            {
                foreach (string key in headers.Keys)
                {
                    if (key.ToLower() == "accept")
                        request.Accept = headers[key];
                    else if (key.ToLower() == "host")
                        request.Host = headers[key];
                    else if (key.ToLower() == "referer")
                        request.Referer = headers[key];
                    else
                        request.Headers.Add(key, headers[key]);
                }
            }
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            using (Stream responseStream = response.GetResponseStream())
            using (MemoryStream ms = new MemoryStream())
            {
                responseStream.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }

    public class HttpSession
    {
        public CookieContainer Cookie { get; private set; } = new CookieContainer();
        public const string UserAgent = "DGJModule.NeteaseMusicApi/1.0.2 .NET CLR v4.0.30319";
        public Exception LastException { get; private set; } = null;
        public HttpSession() { }
        #region 构造函数重载
        public HttpSession(Uri url, string cookies)
        {
            foreach (string t in cookies.Split("; ".ToCharArray()))
            {
                var t1 = t.Split('=');
                Cookie.Add(url, new Cookie(t1[0], t1[1]));
            }
        }
        public HttpSession(string url, string cookies)
        {
            foreach (string t in cookies.Split("; ".ToCharArray()))
            {
                var t1 = t.Split('=');
                Cookie.Add(new Uri(url), new Cookie(t1[0], t1[1]));
            }
        }
        public HttpSession(Uri url, CookieCollection cookies)
        {
            Cookie.Add(url, cookies);
        }
        public HttpSession(string url, CookieCollection cookies)
        {
            Cookie.Add(new Uri(url), cookies);
        }
        public HttpSession(CookieContainer cookies)
        {
            Cookie = cookies;
        }
        #endregion
        public string HttpGet(Uri url, int timeout = 0, string userAgent = UserAgent, IDictionary<string, string> headers = null)
        {
            HttpWebRequest request = null;
            request = WebRequest.Create(url) as HttpWebRequest;
            request.ProtocolVersion = HttpVersion.Version11;
            request.Method = "GET";
            if (timeout != 0) request.Timeout = timeout * 1000;
            request.UserAgent = userAgent;
            request.CookieContainer = Cookie;
            if (headers != null)
            {
                foreach (string key in headers.Keys)
                {
                    if (key.ToLower() == "accept")
                        request.Accept = headers[key];
                    else if (key.ToLower() == "host")
                        request.Host = headers[key];
                    else if (key.ToLower() == "referer")
                        request.Referer = headers[key];
                    else if (key.ToLower() == "content-type")
                        request.ContentType = headers[key];
                    else
                        request.Headers.Add(key, headers[key]);
                }
            }
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                    return reader.ReadToEnd();
            }
            catch (Exception Ex)
            {
                LastException = Ex;
                throw;
            }
        }
        public string HttpGet(string url, int timeout = 0, string userAgent = UserAgent, IDictionary<string, string> headers = null)
        {
            return HttpGet(new Uri(url), timeout, userAgent, headers);
        }
        public async Task<string> HttpGetAsync(Uri url, int timeout = 0, string userAgent = UserAgent, IDictionary<string, string> headers = null)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.ProtocolVersion = HttpVersion.Version11;
            request.Method = "GET";
            if (timeout != 0) { request.Timeout = timeout * 1000; request.ReadWriteTimeout = timeout * 1000; }
            else request.ReadWriteTimeout = 10000;
            request.UserAgent = userAgent;
            request.CookieContainer = Cookie;
            if (headers != null)
            {
                foreach (string key in headers.Keys)
                {
                    if (key.ToLower() == "accept")
                        request.Accept = headers[key];
                    else if (key.ToLower() == "host")
                        request.Host = headers[key];
                    else if (key.ToLower() == "referer")
                        request.Referer = headers[key];
                    else
                        request.Headers.Add(key, headers[key]);
                }
            }
            try
            {
                using (HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse)
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return await reader.ReadToEndAsync();
                }
            }
            catch (Exception Ex)
            {
                LastException = Ex;
                throw;
            }
        }
        public async Task<string> HttpGetAsync(string url, int timeout = 0, string userAgent = UserAgent, IDictionary<string, string> headers = null)
        {
            return await HttpGetAsync(new Uri(url), timeout, userAgent, headers);
        }
        public string HttpPost(Uri url, string formdata = null, int timeout = 0, string userAgent = UserAgent, IDictionary<string, string> headers = null)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.UserAgent = userAgent;
            if (timeout != 0) { request.Timeout = timeout * 1000; request.ReadWriteTimeout = timeout * 1000; }
            else request.ReadWriteTimeout = 10000;
            request.CookieContainer = Cookie;
            if (headers != null)
            {
                foreach (string key in headers.Keys)
                {
                    if (key.ToLower() == "accept")
                        request.Accept = headers[key];
                    else if (key.ToLower() == "host")
                        request.Host = headers[key];
                    else if (key.ToLower() == "referer")
                        request.Referer = headers[key];
                    else if (key.ToLower() == "content-type")
                        request.ContentType = headers[key];
                    else
                        request.Headers.Add(key, headers[key]);
                }
            }
            if (!string.IsNullOrEmpty(formdata))
            {
                byte[] data = Encoding.UTF8.GetBytes(formdata);
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            try
            {
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception Ex)
            {
                LastException = Ex;
                throw;
            }
        }
        public string HttpPost(string url, string formdata = null, int timeout = 0, string userAgent = UserAgent, IDictionary<string, string> headers = null)
        {
            return HttpPost(new Uri(url), formdata, timeout, userAgent, headers);
        }
        public string HttpPost(Uri url, IDictionary<string, string> postdata = null, int timeout = 0, string userAgent = UserAgent, IDictionary<string, string> headers = null)
        {
            string formdata = null;
            if (postdata != null && postdata.Count >= 0)
            {
                formdata = string.Join("&", postdata.Select(p => $"{p.Key}={p.Value}"));
            }
            return HttpPost(url, formdata, timeout, userAgent, headers);
        }
        public string HttpPost(string url, IDictionary<string, string> postdata = null, int timeout = 0, string userAgent = UserAgent, IDictionary<string, string> headers = null)
        {
            return HttpPost(new Uri(url), postdata, timeout, userAgent, headers);
        }
        public async Task<string> HttpPostAsync(Uri url, string formdata = null, int timeout = 0, string userAgent = UserAgent, IDictionary<string, string> headers = null)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.UserAgent = userAgent;
            if (timeout != 0) { request.Timeout = timeout * 1000; request.ReadWriteTimeout = timeout * 1000; }
            else request.ReadWriteTimeout = 10000;
            request.CookieContainer = Cookie;
            if (headers != null)
            {
                foreach (string key in headers.Keys)
                {
                    if (key.ToLower() == "accept")
                        request.Accept = headers[key];
                    else if (key.ToLower() == "host")
                        request.Host = headers[key];
                    else if (key.ToLower() == "referer")
                        request.Referer = headers[key];
                    else if (key.ToLower() == "content-type")
                        request.ContentType = headers[key];
                    else
                        request.Headers.Add(key, headers[key]);
                }
            }
            if (!string.IsNullOrEmpty(formdata))
            {
                byte[] data = Encoding.UTF8.GetBytes(formdata);
                using (Stream stream = await request.GetRequestStreamAsync())
                {
                    await stream.WriteAsync(data, 0, data.Length);
                }
            }
            try
            {
                using (HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse)
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return await reader.ReadToEndAsync();
                }
            }
            catch (Exception Ex)
            {
                LastException = Ex;
                throw;
            }
        }
        public async Task<string> HttpPostAsync(string url, string formdata = null, int timeout = 0, string userAgent = UserAgent, IDictionary<string, string> headers = null)
        {
            return await HttpPostAsync(new Uri(url), formdata, timeout, userAgent, headers);
        }
        public async Task<string> HttpPostAsync(Uri url, IDictionary<string, string> parameters = null, int timeout = 0, string userAgent = UserAgent, IDictionary<string, string> headers = null)
        {
            string formdata = null;
            if (parameters != null && parameters.Count >= 0)
            {
                formdata = string.Join("&", parameters.Select(p => $"{p.Key}={p.Value}"));
            }
            return await HttpPostAsync(url, formdata, timeout, userAgent, headers);
        }
        public async Task<string> HttpPostAsync(string url, IDictionary<string, string> parameters = null, int timeout = 0, string userAgent = UserAgent, IDictionary<string, string> headers = null)
        {
            return await HttpPostAsync(new Uri(url), parameters, timeout, userAgent, headers);
        }
        public string GetCookieString(Uri url)
        {
            string result = "";
            foreach (Cookie c in Cookie.GetCookies(url))
                result += c.Name + "=" + c.Value + "; ";
            return result.Replace("\n", "").TrimEnd("; ".ToCharArray());
        }
        public string GetCookieString(string url)
        {
            return GetCookieString(new Uri(url));
        }
        public void Reset()
        {
            Cookie = new CookieContainer();
            LastException = null;
        }
    }
}