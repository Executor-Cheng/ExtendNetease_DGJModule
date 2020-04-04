using DGJv3;
using ExtendNetease_DGJModule.NeteaseMusic;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using SongInfo = DGJv3.SongInfo;

namespace ExtendNetease_DGJModule
{
    public class ExtendNeteaseModule : SearchModule
    {
        static ExtendNeteaseModule()
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

        public ExtendNeteaseModule()
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
            try
            {
                long songId = long.Parse(songInfo.SongId);
                DownloadSongInfo ds = MainConfig.Instance.LoginSession.LoginStatus ?
                _GetDownloadSongInfo(MainConfig.Instance.LoginSession, songId, MainConfig.Instance.Quality) :
                _GetDownloadSongInfo(songId, MainConfig.Instance.Quality);
                if (ds != null)
                {
                    if (ds.Type.ToLower() == "mp3")
                    {
                        return ds.Url;
                    }
                    Log($"由于点歌姬目前只支持播放mp3格式,当前单曲:{string.Join(";", songInfo.Singers)} - {songInfo.SongName} 格式:{ds.Type} 无法播放喵");
                }
                else
                {
                    Log($"获取下载链接失败了喵(服务器未返回下载链接)");
                }
            }
            catch (WebException e)
            {
                Log($"获取下载链接失败了喵:{e.Message}\r\n这是由于网络原因导致获取失败, 如果多次出现, 请检查你的网络连接喵。");
            }
            catch (Exception e)
            {
                Log($"获取下载链接失败了喵:{e.Message}");
            }
            return null; // 返回null, 点歌姬会自动移除掉当前歌曲
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
            catch (WebException e)
            {
                Log($"获取歌词失败了喵:{e.Message}\r\n这是由于网络原因导致获取失败, 如果多次出现, 请检查你的网络连接喵。");
            }
            catch (Exception e)
            {
                Log($"获取歌词失败了喵:{e.Message}");
            }
            return lyric?.GetLyricText();
        }

        protected override List<SongInfo> GetPlaylist(string keyword)
        {
            try
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
                            Log("在获取歌单时收到了未知的服务器返回喵:" + json);
                            return null;
                        }
                    }
                    else
                    {
                        Log("在获取歌单时收到了未知的服务器返回喵:" + json);
                        return null;
                    }
                }
                NeteaseMusic.SongInfo[] cantPlaySongs = songs.Where(p => !p.CanPlay).ToArray();
                if (cantPlaySongs.Length > 0)
                {
                    if (songs.Length == cantPlaySongs.Length)
                    {
                        Log("该歌单内所有的单曲,网易云都没有版权,所以歌单添加失败了喵");
                        return null;
                    }
                    Log($"以下列出的单曲,网易云暂时没有版权,所以它们被除外了喵~\n{string.Join("\n", cantPlaySongs.Select(p => $"{string.Join("; ", p.Artists.Select(q => q.Name))} - {p.Name}"))}");
                }
                return songs.Where(p => p.CanPlay).Select(p => new SongInfo(this, p.Id.ToString(), p.Name, p.Artists.Select(q => q.Name).ToArray(), null)).ToList();
            }
            catch (WebException e)
            {
                Log($"获取歌单失败了喵:{e.Message}\r\n这是由于网络原因导致获取失败, 如果多次出现, 请检查你的网络连接喵。");
            }
            catch (Exception e)
            {
                Log($"获取歌单失败了喵:{e.Message}");
            }
            return null;
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
                    catch (Exception e)
                    {
                        Log($"获取歌词失败了喵:{e.Message}");
                    }
                    return new SongInfo(this, song.Id.ToString(), song.Name, song.Artists.Select(p => p.Name).ToArray(), lyric?.GetLyricText());
                }
                else
                {
                    Log($"{song.ArtistNames} - {song.Name} : 暂无版权喵");
                }
            }
            catch (WebException e)
            {
                Log($"搜索单曲失败了喵:{e.Message}\r\n这是由于网络原因导致搜索失败, 如果多次出现, 请检查你的网络连接喵。");
            }
            catch (Exception e)
            {
                Log($"搜索单曲失败了喵:{e.Message}");
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
