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
        public string ArtistNames => Artists == null ? null : string.Join(",", Artists.Select(p => p.Name));
        public AlbumInfo Album { get; }
        public TimeSpan Duration { get; }
        public bool CanPlay { get; set; }
        public bool NeedPaymentToDownload { get; } // Fee == 8 | > 0
        public SongInfo(long id, string name, ArtistInfo[] artists, AlbumInfo album, TimeSpan duration, bool needPaymentToDownload)
        {
            Id = id;
            Name = name;
            Artists = artists;
            Album = album;
            Duration = duration;
            NeedPaymentToDownload = needPaymentToDownload;
        }
        public SongInfo(JToken jt) : this(jt["id"].ToObject<long>(), jt["name"].ToString(), (jt["artists"] ?? jt["ar"]).Select(p => new ArtistInfo(p)).ToArray(), new AlbumInfo(jt["album"] ?? jt["al"]), TimeSpan.FromMilliseconds((jt["duration"] ?? jt["dt"]).ToObject<double>()), jt["fee"].ToObject<bool>())
        {

        }
    }
}
