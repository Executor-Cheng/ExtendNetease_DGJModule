using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace ExtendNetease_DGJModule.Models
{
    public class SongInfo
    {
        public long Id { get; }

        public string Name { get; }

        public ArtistInfo[] Artists { get; }

        public AlbumInfo Album { get; }

        public TimeSpan Duration { get; }

        public bool HasCopyright { get; }

        public bool NeedPaymentToDownload { get; } // Fee == 1

        public SongInfo(long id, string name, ArtistInfo[] artists, AlbumInfo album, TimeSpan duration, bool hasCopyright, bool needPaymentToDownload)
        {
            Id = id;
            Name = name;
            Artists = artists;
            Album = album;
            Duration = duration;
            HasCopyright = hasCopyright;
            NeedPaymentToDownload = needPaymentToDownload;
        }

        public static SongInfo Parse(JToken node)
        {
            return new SongInfo(node["id"].ToObject<long>(), node["name"].ToString(), (node["artists"] ?? node["ar"]).Select(ArtistInfo.Parse).ToArray(), AlbumInfo.Parse(node["album"] ?? node["al"]), TimeSpan.FromMilliseconds((node["duration"] ?? node["dt"]).ToObject<double>()), node["noCopyrightRcmd"]?.HasValues != true, node["fee"].ToObject<int>() == 1);
        }
    }
}
