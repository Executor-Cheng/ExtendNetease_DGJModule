using System;

namespace ExtendNetease_DGJModule.Models
{
    public class PluginInfo
    {
        public string Name { get; set; }

        public string Author { get; set; }

        public Version Version { get; set; }
        
        public string Description { get; set; }

        public DateTime UpdateTime { get; set; }

        public string UpdateDescription { get; set; }

        public string DownloadUrl { get; set; }

        public string DownloadNote { get; set; }
    }
}
