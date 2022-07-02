using ExtendNetease_DGJModule.Models;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ExtendNetease_DGJModule
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class MainConfig : INotifyPropertyChanged
    {
        [JsonProperty]
        public string Cookie { get => _cookie; set { if (_cookie != value) { _cookie = value; OnPropertyChanged(); } } }

        [JsonProperty]
        public Quality Quality { get => _quality; set { if (_quality != value) { _quality = value; OnPropertyChanged(); } } }

        private string _cookie;

        private Quality _quality = Quality.HighQuality;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
