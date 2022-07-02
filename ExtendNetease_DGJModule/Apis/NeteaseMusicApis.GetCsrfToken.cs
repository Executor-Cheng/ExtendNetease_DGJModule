using ExtendNetease_DGJModule.Clients;
using ExtendNetease_DGJModule.Exceptions;
using System;
using System.Linq;
using System.Net;

namespace ExtendNetease_DGJModule.Apis
{
    public static partial class NeteaseMusicApis
    {
        private static string GetCsrfToken(HttpClientv2 client)
        {
            Cookie cookie = client.Cookies.GetCookies(new Uri("http://music.163.com/")).OfType<Cookie>().FirstOrDefault(p => p.Name == "__csrf");
            if (cookie == null)
            {
                throw new InvalidCookieException();
            }
            return cookie.Value;
        }
    }
}
