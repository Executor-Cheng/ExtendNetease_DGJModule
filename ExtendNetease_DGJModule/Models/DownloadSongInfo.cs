using Newtonsoft.Json.Linq;
using System;

namespace ExtendNetease_DGJModule.Models
{
    public class DownloadSongInfo
    {
        public long Id { get; }
        public int Bitrate { get; }
        public Quality RequestQuality { get; }
        public Quality Quality { get; }
        public string Url { get; }
        public string Type { get; }
        public DateTime ExpireTime { get; }
        public DownloadSongInfo(long id, int bitrate, Quality requestQuality, string url, string type)
        {
            Id = id;
            Bitrate = bitrate;
            RequestQuality = requestQuality;
            if (bitrate > 0) // For self uploaded musics, bitrate may not be one of the Quality Enum values
            {
                if (bitrate <= (int)Quality.LowQuality)
                {
                    Quality = Quality.LowQuality;
                }
                else if (bitrate <= (int)Quality.MediumQuality)
                {
                    Quality = Quality.MediumQuality;
                }
                else if (bitrate <= (int)Quality.HighQuality)
                {
                    Quality = Quality.HighQuality;
                }
                else
                {
                    Quality = Quality.SuperQuality;
                }
            }
            Url = url;
            Type = type;
            ExpireTime = DateTime.Now.AddMinutes(20);
        }
        public DownloadSongInfo(JToken jt, Quality requestQuality) : this(jt["id"].ToObject<long>(), jt["br"].ToObject<int>(), requestQuality, jt["url"].ToString(), jt["type"].ToString())
        {
            
        }
    }
}
