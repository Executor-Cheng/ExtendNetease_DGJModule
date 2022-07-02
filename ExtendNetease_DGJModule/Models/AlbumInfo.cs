using Newtonsoft.Json.Linq;
using System;

namespace ExtendNetease_DGJModule.Models
{
    public class AlbumInfo
    {
        public long Id { get; }
        public string Name { get; }
        public DateTime PublishTime { get; }
        public int Count { get; }
        public AlbumInfo(long id, string name, DateTime publishTime, int count)
        {
            Id = id;
            Name = name;
            PublishTime = publishTime;
            Count = count;
        }
        public AlbumInfo(JToken jt) : this(jt["id"].ToObject<long>(), jt["name"].ToString(), Utils.UnixTime2DateTime(jt["publishTime"]?.ToObject<long>() ?? 0), jt["size"]?.ToObject<int>() ?? 0)
        {

        }
    }
}
