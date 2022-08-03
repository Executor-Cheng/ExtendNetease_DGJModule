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

        public static AlbumInfo Parse(JToken node)
        {
            long publishTime = node["publishTime"]?.ToObject<long>() ?? 0;
            int count = node["size"]?.ToObject<int>() ?? 0;
            return new AlbumInfo(node["id"].ToObject<long>(), node["name"].ToString(), Utils.UnixTime2DateTime(publishTime), count);
        }
    }
}
