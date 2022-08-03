using Newtonsoft.Json.Linq;

namespace ExtendNetease_DGJModule.Models
{
    public class ArtistInfo
    {
        public long Id { get; }

        public string Name { get; }

        public ArtistInfo(long id, string name)
        {
            Id = id;
            Name = name;
        }

        public static ArtistInfo Parse(JToken node)
        {
            return new ArtistInfo(node["id"].ToObject<long>(), node["name"].ToString());
        }
    }
}
