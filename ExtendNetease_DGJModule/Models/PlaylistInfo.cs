using Newtonsoft.Json.Linq;

namespace ExtendNetease_DGJModule.Models
{
    public class PlaylistInfo
    {
        public long Id { get; }

        public string Name { get; }

        public PlaylistInfo(long id, string name)
        {
            Id = id;
            Name = name;
        }

        public static PlaylistInfo Parse(JToken node)
        {
            return new PlaylistInfo(node["id"].ToObject<long>(), null);
        }
    }
}
