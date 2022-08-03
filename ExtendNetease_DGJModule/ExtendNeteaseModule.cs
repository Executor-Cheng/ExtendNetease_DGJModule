using DGJv3;
using ExtendNetease_DGJModule.Apis;
using ExtendNetease_DGJModule.Clients;
using ExtendNetease_DGJModule.Models;
using ExtendNetease_DGJModule.NeteaseMusic.Services;
using ExtendNetease_DGJModule.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using DGJSongInfo = DGJv3.SongInfo;
using SongInfo = ExtendNetease_DGJModule.Models.SongInfo;

namespace ExtendNetease_DGJModule
{
    public class ExtendNeteaseModule : SearchModule
    {
        private readonly IDictionary<long, LyricInfo> _lyricCache;

        private readonly IDictionary<Tuple<long, Quality>, DownloadSongInfo> _downloadCache;

        private readonly NeteaseSession _session;

        private readonly ConfigService _config;

        private readonly HttpClientv2 _client;

        public ExtendNeteaseModule(PluginMain plugin, NeteaseSession session, ConfigService config, HttpClientv2 client)
        {
            SetInfo("本地网易云喵块", "西井丶", "847529602@qq.com", plugin.PluginVer, "可以添加歌单和登录网易云喵~");
            this.IsPlaylistSupported = true; // Enable Playlist Supporting
            _session = session;
            _config = config;
            _client = client;
            _lyricCache = new ConcurrentDictionary<long, LyricInfo>();
            _downloadCache = new ConcurrentDictionary<Tuple<long, Quality>, DownloadSongInfo>();
        }

        public void SetLogHandler(Action<string> logHandler)
        {
            this.GetType().GetProperty("_log", BindingFlags.SetProperty | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this, logHandler);
        }

        private void AddSongItemToCache(Tuple<long, Quality> key, DownloadSongInfo value)
        {
            _downloadCache[key] = value;
            if (_downloadCache.Count > 50)
            {
                _downloadCache.Remove(_downloadCache.FirstOrDefault());
            }
        }

        protected override DownloadStatus Download(SongItem item)
        {
            throw new NotImplementedException();
        }

        protected override string GetDownloadUrl(SongItem songInfo)
        {
            try
            {
                if (long.TryParse(songInfo.SongId, out long songId))
                {
                    Quality quality = _config.Config?.Quality ?? Quality.HighQuality;
                    Tuple<long, Quality> key = new Tuple<long, Quality>(songId, quality);
                    if (!_downloadCache.TryGetValue(key, out DownloadSongInfo downloadInfo))
                    {
                        DownloadSongInfo[] songs = Task.Factory.StartNew(() => NeteaseMusicApis.GetSongsUrlAsync(_client, new long[1] { songId }, quality).ConfigureAwait(false).GetAwaiter().GetResult()).GetAwaiter().GetResult();
                        if (songs.Length != 0)
                        {
                            downloadInfo = songs[0];
                            if (downloadInfo.Quality == Quality.Unknown)
                            {
                                Log($"当前单曲:{string.Join(";", songInfo.Singers)} - {songInfo.SongName} 由于没有版权/你木有VIP,无法播放喵");
                                return null;
                            }
                            if (downloadInfo.Type.Equals("mp3", StringComparison.OrdinalIgnoreCase))
                            {
                                AddSongItemToCache(key, downloadInfo);
                                return downloadInfo.Url;
                            }
                            Log($"由于点歌姬目前只支持播放mp3格式,当前单曲:{string.Join(";", songInfo.Singers)} - {songInfo.SongName} 格式:{downloadInfo.Type} 无法播放喵");
                        }
                        else
                        {
                            Log("获取下载链接失败了喵(服务器未返回下载链接)");
                        }
                        return null;
                    }
                    return downloadInfo.Url;
                }
            }
            catch (HttpRequestException e)
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

        protected override string GetLyricById(string id)
        {
            if (long.TryParse(id, out long songId))
            {
                return GetLyricWithLogging(songId)?.GetLyricText();
            }
            return null;
        }

        protected override List<DGJSongInfo> GetPlaylist(string keyword)
        {
            return Task.Factory.StartNew(() => GetPlaylistCore(keyword)).GetAwaiter().GetResult();
        }

        private List<DGJSongInfo> GetPlaylistCore(string keyword)
        {
            try
            {
                SongInfo[] songs;
                if (long.TryParse(keyword, out long id))
                {
                    songs = NeteaseMusicApis.GetPlaylistAsync(_client, id).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                else
                {
                    PlaylistInfo[] playlists = NeteaseMusicApis.SearchPlaylistsAsync(_client, keyword, 1).ConfigureAwait(false).GetAwaiter().GetResult();
                    if (playlists.Length != 0)
                    {
                        songs = NeteaseMusicApis.GetPlaylistAsync(_client, playlists[0].Id).ConfigureAwait(false).GetAwaiter().GetResult();
                    }
                    return null;
                }
                SongInfo[] cantPlaySongs = songs.Where(p => !p.HasCopyright || (p.NeedPaymentToDownload && _session.VipType == 0)).ToArray();
                if (cantPlaySongs.Length > 0)
                {
                    if (songs.Length == cantPlaySongs.Length)
                    {
                        Log("该歌单内所有的单曲,网易云都没有版权,或者需要会员才能下载,所以歌单添加失败了喵");
                        return null;
                    }
                    Log($"以下列出的单曲,网易云暂时没有版权,所以它们被除外了喵~\n{string.Join("\n", cantPlaySongs.Select(p => $"{string.Join("; ", p.Artists.Select(q => q.Name))} - {p.Name}"))}");
                }
                return songs.Where(p => p.HasCopyright && !p.NeedPaymentToDownload).Select(p => new DGJSongInfo(this, p.Id.ToString(), p.Name, p.Artists.Select(q => q.Name).ToArray(), null)).ToList();
            }
            catch (HttpRequestException e)
            {
                Log($"获取歌单失败了喵:{e.Message}\r\n这是由于网络原因导致获取失败, 如果多次出现, 请检查你的网络连接喵。");
            }
            catch (Exception e)
            {
                Log($"获取歌单失败了喵:{e.Message}");
            }
            return null;
        }

        protected override DGJSongInfo Search(string keyword)
        {
            return Task.Factory.StartNew(() => SearchCore(keyword)).GetAwaiter().GetResult();
        }

        private DGJSongInfo SearchCore(string keyword)
        {
            try
            {
                SongInfo[] songs = NeteaseMusicApis.SearchSongsAsync(_client, keyword, 1).ConfigureAwait(false).GetAwaiter().GetResult();
                if (songs.Length != 0)
                {
                    SongInfo song = songs[0];
                    string artistName = string.Join(",", song.Artists?.Select(p => p.Name) ?? Array.Empty<string>());
                    if (song.HasCopyright)
                    {
                        if (!song.NeedPaymentToDownload || _session.VipType != 0)
                        {
                            LyricInfo lyric = GetLyricWithLogging(song.Id);
                            return new DGJSongInfo(this, song.Id.ToString(), song.Name, song.Artists?.Select(p => p.Name).ToArray() ?? Array.Empty<string>(), lyric?.GetLyricText());
                        }
                        Log($"{artistName} - {song.Name} : 需要会员才可下载喵");
                    }
                    else
                    {
                        Log($"{artistName} - {song.Name} : 暂无版权喵");
                    }
                }
            }
            catch (HttpRequestException e)
            {
                Log($"搜索单曲失败了喵:{e.Message}\r\n这是由于网络原因导致搜索失败, 如果多次出现, 请检查你的网络连接喵。");
            }
            catch (Exception e)
            {
                Log($"搜索单曲失败了喵:{e.Message}");
            }
            return null;
        }

        private LyricInfo GetLyricWithLogging(long songId)
        {
            try
            {
                return GetLyricCore(songId);
            }
            catch (HttpRequestException e)
            {
                Log($"获取歌词失败了喵:{e.Message}\r\n这是由于网络原因导致获取失败, 如果多次出现, 请检查你的网络连接喵。");
            }
            catch (Exception e)
            {
                Log($"获取歌词失败了喵:{e.Message}");
            }
            return null;
        }

        private LyricInfo GetLyricCore(long songId)
        {
            if (!_lyricCache.TryGetValue(songId, out LyricInfo lyric))
            {
                lyric = Task.Factory.StartNew(() => NeteaseMusicApis.GetLyricAsync(_client, songId).ConfigureAwait(false).GetAwaiter().GetResult()).GetAwaiter().GetResult();
                _lyricCache[songId] = lyric;
                if (_lyricCache.Count > 100)
                {
                    _lyricCache.Remove(_lyricCache.FirstOrDefault());
                }
            }
            return lyric;
        }
    }
}
