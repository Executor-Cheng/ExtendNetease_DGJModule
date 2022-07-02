using Newtonsoft.Json.Linq;

namespace ExtendNetease_DGJModule.Models
{
    public class PlaylistInfo
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public PlaylistInfo()
        {

        }

        public PlaylistInfo(long id, string name)
        {
            Id = id;
            Name = name;
        }

        public PlaylistInfo(JToken node)
        {
            Id = node["id"].ToObject<long>();
            Name = null;
        }
    }
}
