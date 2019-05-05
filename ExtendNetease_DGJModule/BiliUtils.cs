using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;

namespace ExtendNetease_DGJModule
{
    public static class BiliUtils
    {
        /// <summary>
        /// 通过用户UID获取用户名
        /// </summary>
        /// <exception cref="NotImplementedException"/>
        /// <exception cref="WebException"/>
        /// <param name="userId">用户ID</param>
        /// <returns>用户名</returns>
        public static string GetUserNameByUserId(int userId)
        {
            IDictionary<string, string> headers = new Dictionary<string, string>
            {
                { "Origin", "https://space.bilibili.com" },
                { "Referer", $"https://space.bilibili.com/{userId}/" },
                { "X-Requested-With", "XMLHttpRequest" }
            };
            string json = HttpHelper.HttpPost("https://space.bilibili.com/ajax/member/GetInfo", $"mid={userId}&csrf=", 10, headers: headers);
            JObject j = JObject.Parse(json);
            if (j["status"].ToObject<bool>())
            {
                return j["data"]["name"].ToString();
            }
            else
            {
                throw new NotImplementedException($"未知的服务器返回:{j.ToString(0)}");
            }
        }
    }
}
