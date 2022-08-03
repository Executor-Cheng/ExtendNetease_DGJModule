using Newtonsoft.Json.Linq;
using System;

namespace ExtendNetease_DGJModule.Models
{
    public class DownloadSongInfo
    {
        public long Id { get; }

        public int Bitrate { get; }

        public Quality Quality { get; }

        public string Url { get; }

        public string Type { get; }

        public DateTime ExpireTime { get; }

        public DownloadSongInfo(long id, int bitrate, string url, string type)
        {
            Id = id;
            Bitrate = bitrate;
            if (bitrate <= (int)Quality.Unknown) // For self uploaded musics, bitrate may not be one of the Quality Enum values
            {
                Quality = Quality.Unknown;
            }
            else if (bitrate <= (int)Quality.LowQuality)
            {
                Quality = Quality.LowQuality;
            }
            else if (bitrate <= (int)Quality.MediumQuality)
            {
                Quality = Quality.MediumQuality;
            }
            else if (Bitrate <= (int)Quality.HighQuality)
            {
                Quality = Quality.HighQuality;
            }
            else
            {
                Quality = Quality.SuperQuality;
            }
            Url = url;
            Type = type;
            ExpireTime = DateTime.Now.AddMinutes(20);
        }

        public static DownloadSongInfo Parse(JToken node)
        {
            return new DownloadSongInfo(node["id"].ToObject<long>(), node["br"].ToObject<int>(), node["url"].ToString(), node["type"].ToString());
        }
    }
}
