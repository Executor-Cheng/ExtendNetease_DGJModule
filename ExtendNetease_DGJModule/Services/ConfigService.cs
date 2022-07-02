using Newtonsoft.Json;
using System;
using System.IO;

namespace ExtendNetease_DGJModule.Services
{
    public sealed class ConfigService
    {
        public MainConfig Config => _config;

        private readonly MainConfig _config;

        private readonly string _configDirectory;

        private readonly string _configFileName;

        public ConfigService()
        {
            _configDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), @"弹幕姬\plugins\ExtendNetease");
            _configFileName = "MainConfig.cfg";
            _config = new MainConfig();
        }

        public bool LoadConfig()
        {
            string configPath = Path.Combine(_configDirectory, _configFileName);
            if (File.Exists(configPath))
            {
                string json = File.ReadAllText(configPath);
                MainConfig config = JsonConvert.DeserializeObject<MainConfig>(json);
                _config.Cookie = config.Cookie;
                _config.Quality = config.Quality;
                return true;
            }
            return false;
        }

        public void SaveConfig()
        {
            if (_config == null)
            {
                return;
            }
            if (!Directory.Exists(_configDirectory))
            {
                Directory.CreateDirectory(_configDirectory);
            }
            string configPath = Path.Combine(_configDirectory, _configFileName);
            File.WriteAllText(configPath, JsonConvert.SerializeObject(_config));
        }
    }
}
