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
        public ArtistInfo(JToken jt) : this (jt["id"].ToObject<long>(), jt["name"].ToString())
        {

        }
    }
}
