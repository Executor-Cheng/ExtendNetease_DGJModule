using ExtendNetease_DGJModule.NeteaseMusic;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;

namespace ExtendNetease_DGJModule
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class MainConfig : INotifyPropertyChanged
    {
        public static string ConfigFullPath { get; }

        public static MainConfig Instance { get; }

        static MainConfig()
        {
            try
            {
                string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), @"弹幕姬\plugins\ExtendNetease");
                if (!Directory.Exists(configPath))
                {
                    Directory.CreateDirectory(configPath);
                }
                ConfigFullPath = Path.Combine(configPath, "MainConfig.cfg");
                if (File.Exists(ConfigFullPath))
                {
                    string json = File.ReadAllText(ConfigFullPath);
                    try
                    {
                        Instance = JsonConvert.DeserializeObject<MainConfig>(json);
                    }
                    catch
                    {
                        Instance = new MainConfig();
                    }
                }
                else
                {
                    Instance = new MainConfig();
                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.ToString());
            }
        }

        [JsonProperty]
        public string Cookie
        {
            get => LoginSession.Session.GetCookieString("http://music.163.com/");
            set => LoginSession.Login(value);
        }

        [JsonProperty]
        public Quality Quality { get => _Quality; set { if (_Quality != value) { _Quality = value; OnPropertyChanged(); } } }

        public NeteaseSession LoginSession { get; } = new NeteaseSession();

        private Quality _Quality = Quality.HighQuality;

        public void SaveConfig()
        {
            File.WriteAllText(ConfigFullPath, JsonConvert.SerializeObject(this));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
