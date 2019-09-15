using BilibiliDM_PluginFramework;
using DGJv3;
using ExtendNetease_DGJModule.NeteaseMusic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using SongInfo = DGJv3.SongInfo;

namespace ExtendNetease_DGJModule
{
    public class PluginMain : DMPlugin
    {
        public static MainWindow MainWindow { get; private set; }

        public static ExtendNetease ExtendNeteaseModule { get; private set; }
        
        public PluginMain()
        {
            this.PluginName = "本地网易云喵块";
            try { this.PluginAuth = BiliUtils.GetUserNameByUserId(35744708); }
            catch { this.PluginAuth = "西井丶"; }
            this.PluginCont = "847529602@qq.com";
            this.PluginDesc = "可以添加歌单和登录网易云喵~";
            this.PluginVer = NeteaseMusicApi.Version;
            base.Start();
        }

        public override void Inited()
        {
            try
            {
                ExtendNeteaseModule = new ExtendNetease();
                InjectDGJ();
                MainWindow = new MainWindow();
                MainWindow.OnLoginStatusChanged(MainConfig.Instance.LoginSession.LoginStatus);
            }
            catch (Exception Ex)
            {
                MessageBox.Show($"插件初始化失败了喵,请将桌面上的错误报告发送给作者（/TДT)/\n{Ex.ToString()}", "本地网易云喵块", 0, MessageBoxImage.Error);
                throw;
            }
            VersionChecker vc = new VersionChecker("ExtendNetease_DGJModule");
            if (!vc.FetchInfo())
            {
                Log($"版本检查失败了喵 : {vc.lastException.Message}");
                return;
            }
            if (vc.hasNewVersion(this.PluginVer))
            {
                Log($"有新版本了喵~最新版本 : {vc.Version}\n                {vc.UpdateDescription}");
                Log($"下载地址 : {vc.DownloadUrl}");
                Log($"插件页面 : {vc.WebPageUrl}");
            }
        }

        public override void DeInit()
        {
            MainConfig.Instance.SaveConfig();
            MainWindow.UnbindEventsAndClose();
        }

        public override void Admin()
        {
            MainWindow.Show();
            MainWindow.Topmost = true;
            MainWindow.Topmost = false;
        }

        public override void Start()
        {
            Log("若要启用插件,去点歌姬内把“本地网易云喵块”选入首/备选模块之一即可喵");
        }

        public override void Stop()
        {
            Log("若要禁用插件,去点歌姬内把“本地网易云喵块”移出首/备选模块即可喵");
        }

        private void InjectDGJ()
        {
            try
            {
                Assembly dgjAssembly = Assembly.GetAssembly(typeof(SearchModule)); //如果没有点歌姬插件，插件的构造方法会抛出异常，无需考虑这里的assembly == null的情况
                Assembly dmAssembly = Assembly.GetEntryAssembly();
                Type appType = dmAssembly.ExportedTypes.FirstOrDefault(p => p.FullName == "Bililive_dm.App");
                ObservableCollection<DMPlugin> Plugins = (ObservableCollection<DMPlugin>)appType.GetField("Plugins", BindingFlags.GetField | BindingFlags.Static | BindingFlags.Public).GetValue(null);
                DMPlugin dgjPlugin = Plugins.FirstOrDefault(p => p.ToString() == "DGJv3.DGJMain");
                object dgjWindow = null;
                try
                {
                    dgjWindow = dgjAssembly.DefinedTypes.FirstOrDefault(p => p.Name == "DGJMain").GetField("window", BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(dgjPlugin);
                }
                catch (ReflectionTypeLoadException Ex) // 缺少登录中心时
                {
                    dgjWindow = Ex.Types.FirstOrDefault(p => p.Name == "DGJMain").GetField("window", BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(dgjPlugin);
                }
                object searchModules = dgjWindow.GetType().GetProperty("SearchModules", BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public).GetValue(dgjWindow);
                ObservableCollection<SearchModule> searchModules2 = (ObservableCollection<SearchModule>)searchModules.GetType().GetProperty("Modules", BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public).GetValue(searchModules);
                SearchModule nullModule = (SearchModule)searchModules.GetType().GetProperty("NullModule", BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance).GetValue(searchModules);
                SearchModule lwlModule = searchModules2.FirstOrDefault(p => p != nullModule);
                if (lwlModule != null)
                {
                    Action<string> logHandler = (Action<string>)lwlModule.GetType().GetProperty("_log", BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(lwlModule);
                    ExtendNeteaseModule.SetLogHandler(logHandler);
                }
                searchModules2.Insert(2, ExtendNeteaseModule);
            }
            catch (Exception Ex)
            {
                MessageBox.Show($"注入到点歌姬失败了喵\n{Ex.ToString()}", "本地网易云喵块", 0, MessageBoxImage.Error);
                throw;
            }
        }
    }

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

    public class ExtendNetease : SearchModule
    {
        static ExtendNetease()
        {
            string assemblyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), @"弹幕姬\plugins\Assembly");
            if (!Directory.Exists(assemblyPath))
            {
                Directory.CreateDirectory(assemblyPath);
            }
            string filePath = Path.Combine(assemblyPath, "BouncyCastle.Crypto.dll");
            if (!File.Exists(filePath))
            {
                File.WriteAllBytes(filePath, Properties.Resources.BouncyCastle_Crypto);
            }
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string dllName = args.Name.Split(',')[0];
            if (dllName == "BouncyCastle.Crypto")
            {
                string assemblyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), @"弹幕姬\plugins\Assembly");
                return Assembly.LoadFrom(Path.Combine(assemblyPath, "BouncyCastle.Crypto.dll"));
            }
            else
            {
                return null;
            }
        }

        private IDictionary<long, LyricInfo> LyricCache { get; } = new Dictionary<long, LyricInfo>();

        private List<DownloadSongInfo> DownloadSongInfoCache { get; } = new List<DownloadSongInfo>();

        public ExtendNetease()
        {
            string authorName;
            try { authorName = BiliUtils.GetUserNameByUserId(35744708); }
            catch { authorName = "西井丶"; }
            SetInfo("本地网易云喵块", authorName, "847529602@qq.com", NeteaseMusicApi.Version, "可以添加歌单和登录网易云喵~");
            this.GetType().GetProperty("IsPlaylistSupported", BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.Instance).SetValue(this, true); // Enable Playlist Supporting
        }

        public void SetLogHandler(Action<string> logHandler)
        {
            this.GetType().GetProperty("_log", BindingFlags.SetProperty | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this, logHandler);
        }

        protected override DownloadStatus Download(SongItem item)
        {
            throw new NotImplementedException();
        }

        protected override string GetDownloadUrl(SongItem songInfo)
        {
            long songId = long.Parse(songInfo.SongId);
            DownloadSongInfo ds = MainConfig.Instance.LoginSession.LoginStatus ?
                _GetDownloadSongInfo(MainConfig.Instance.LoginSession, songId, MainConfig.Instance.Quality) :
                _GetDownloadSongInfo(songId, MainConfig.Instance.Quality);
            if (ds != null)
            {
                if (ds.Type.ToLower() != "mp3")
                {
                    Log($"由于点歌姬目前只支持播放mp3格式,当前单曲:{string.Join(";", songInfo.Singers)} - {songInfo.SongName} 格式:{ds.Type} 无法播放喵");
                }
                else
                {
                    //string url = ds.Url;
                    //if (!string.IsNullOrEmpty(url) && songInfo.UserName == "空闲歌单" && songInfo.Lyric.LrcWord.Count < 1)
                    //{
                    //    try
                    //    {
                    //        LyricInfo lyric = _GetLyric(songId);
                    //        Lrc lrc = Lrc.InitLrc(lyric?.GetLyricText());
                    //        Type songItemType = songInfo.GetType();
                    //        songItemType.GetProperty("Lyric", BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.Instance).SetValue(songInfo, lrc);
                    //    }
                    //    catch (Exception Ex)
                    //    {
                    //        Log($"获取歌词失败了喵:{Ex.Message}");
                    //    }
                    //}
                    return ds.Url;
                }
            }
            else
            {
                Log($"获取下载链接失败了喵(服务器未返回下载链接)");
            }
            return null;
        }

        [Obsolete("Use GetLyricById instead", true)]
        protected override string GetLyric(SongItem songInfo)
        {
            throw new NotSupportedException();
        }

        protected override string GetLyricById(string Id)
        {
            long id = long.Parse(Id);
            LyricInfo lyric = null;
            try
            {
                lyric = _GetLyric(id);
            }
            catch (Exception Ex)
            {
                Log($"获取歌词失败了喵:{Ex.Message}");
            }
            return lyric?.GetLyricText();
        }

        protected override List<SongInfo> GetPlaylist(string keyword)
        {
            NeteaseMusic.SongInfo[] songs;
            if (long.TryParse(keyword, out long id))
            {
                songs = MainConfig.Instance.LoginSession.LoginStatus ? 
                    NeteaseMusicApi.GetPlayList(MainConfig.Instance.LoginSession, id) : 
                    NeteaseMusicApi.GetPlayList(id);
            }
            else
            {
                string json = NeteaseMusicApi.Search(keyword, SearchType.SongList, 1);
                JObject j = JObject.Parse(json);
                if (j["code"].ToObject<int>() == 200)
                {
                    id = j["result"]["playlists"].Select(p => p["id"].ToObject<long>()).FirstOrDefault();
                    if (id > 0)
                    {
                        songs = MainConfig.Instance.LoginSession.LoginStatus ?
                            NeteaseMusicApi.GetPlayList(MainConfig.Instance.LoginSession, id) :
                            NeteaseMusicApi.GetPlayList(id);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            NeteaseMusic.SongInfo[] cantPlaySongs = songs.Where(p => !p.CanPlay).ToArray();
            if (cantPlaySongs.Length > 0)
            {
                if (songs.Length == cantPlaySongs.Length)
                {
                    Log("该歌单内所有的单曲,网易云都没有版权,所以歌单添加失败了喵");
                    return new List<SongInfo>();
                }
                else
                {
                    Log($"以下列出的单曲,网易云暂时没有版权,所以它们被除外了喵~\n{string.Join("\n", cantPlaySongs.Select(p => $"{string.Join("; ", p.Artists.Select(q => q.Name))} - {p.Name}"))}");
                }
            }
            return songs.Where(p => p.CanPlay).Select(p => new SongInfo(this, p.Id.ToString(), p.Name, p.Artists.Select(q => q.Name).ToArray(), null)).ToList();
        }

        protected override SongInfo Search(string keyword)
        {
            try
            {
                NeteaseMusic.SongInfo[] songs = MainConfig.Instance.LoginSession.LoginStatus ?
                    NeteaseMusicApi.SearchSongs(MainConfig.Instance.LoginSession, keyword, 1) :
                    NeteaseMusicApi.SearchSongs(keyword, 1);
                NeteaseMusic.SongInfo song = songs.FirstOrDefault();
                if (song?.CanPlay == true)
                {
                    LyricInfo lyric = null;
                    try
                    {
                        lyric = _GetLyric(song.Id);
                    }
                    catch (Exception Ex)
                    {
                        Log($"获取歌词失败了喵:{Ex.Message}");
                    }
                    return new SongInfo(this, song.Id.ToString(), song.Name, song.Artists.Select(p => p.Name).ToArray(), lyric?.GetLyricText());
                }
                else
                {
                    Log($"{song.ArtistNames} - {song.Name} : 暂无版权喵");
                }
            }
            catch (Exception Ex)
            {
                Log($"搜索单曲失败了喵:{Ex.Message}");
            }
            return null;
        }

        private LyricInfo _GetLyric(long id, bool useCache = true)
        {
            if (!useCache || !LyricCache.ContainsKey(id))
            {
                LyricInfo lyric = NeteaseMusicApi.GetLyric(id);
                LyricCache[id] = lyric;
            }
            return LyricCache[id];
        }

        private DownloadSongInfo _GetDownloadSongInfo(long id, Quality quality, bool useCache = true)
            => _GetDownloadSongInfo(null, id, quality, useCache);

        private DownloadSongInfo _GetDownloadSongInfo(NeteaseSession session, long id, Quality quality, bool useCache = true)
        {
            DownloadSongInfo dsi;
            lock (DownloadSongInfoCache)
            {
                DownloadSongInfoCache.RemoveAll(p => p.ExpireTime < DateTime.Now);
                dsi = DownloadSongInfoCache.Find(p => p.Id == id && p.RequestQuality == quality);
            }
            if (!useCache || dsi == null)
            {
                IDictionary<long, DownloadSongInfo> dss = NeteaseMusicApi.GetSongsUrl(session, MainConfig.Instance.Quality, songIds: id);
                if (dss.TryGetValue(id, out dsi))
                {
                    DownloadSongInfoCache.Add(dsi);
                }
            }
            return dsi;
        }
    }
}
