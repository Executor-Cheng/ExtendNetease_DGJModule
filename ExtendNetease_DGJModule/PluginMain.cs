using BilibiliDM_PluginFramework;
using DGJv3;
using ExtendNetease_DGJModule.Clients;
using ExtendNetease_DGJModule.Exceptions;
using ExtendNetease_DGJModule.NeteaseMusic.Services;
using ExtendNetease_DGJModule.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace ExtendNetease_DGJModule
{
    public class PluginMain : DMPlugin
    {
        private readonly HttpClientv2 _client;

        private readonly ConfigService _config;

        private readonly NeteaseSession _session;

        private readonly DependencyExtractor _dependencyExtractor;

        private MainWindow _mainWindow;

        public PluginMain()
        {
            this.PluginName = "本地网易云喵块";
            this.PluginAuth = "西井丶";
            this.PluginCont = "847529602@qq.com";
            this.PluginDesc = "可以添加歌单和登录网易云喵~";
            this.PluginVer = Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
            _client = new HttpClientv2();
            _client.DefaultRequestHeaders.Accept.ParseAdd("*/*");
            //_client.DefaultRequestHeaders.UserAgent.ParseAdd($"DGJModule.NeteaseMusicApi/{this.PluginVer} .NET CLR v4.0.30319");
            _client.DefaultRequestHeaders.UserAgent.ParseAdd($"Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.30 Safari/537.36");
            _config = new ConfigService();
            _session = new NeteaseSession(_config, _client);
            _dependencyExtractor = new DependencyExtractor();
            base.Start();
        }

        public override void Inited()
        {
            try
            {
                _dependencyExtractor.Extract();
            }
            catch (Exception e)
            {
                MessageBox.Show($"无法释放必要的程序组件,请将桌面上的错误报告发送给作者（/TДT)/\n{e}", "本地网易云喵块", 0, MessageBoxImage.Error);
                throw;
            }
            try
            {
                InjectDGJ();
                _mainWindow = new MainWindow(_config, _session, _client);
            }
            catch (DllNotFoundException)
            {
                MessageBox.Show("你还没有安装点歌姬v3插件, 请先安装再使用本插件, 本插件加载将被取消。", "本地网易云喵块", 0, MessageBoxImage.Error);
                throw;
            }
            catch (Exception e)
            {
                MessageBox.Show($"插件初始化失败了喵,请将桌面上的错误报告发送给作者（/TДT)/\n{e}", "本地网易云喵块", 0, MessageBoxImage.Error);
                throw;
            }
            try
            {
                _config.LoadConfig();
            }
            catch (Exception e)
            {
                MessageBox.Show($"无法读取配置文件（/TДT)/\n{e}", "本地网易云喵块", 0, MessageBoxImage.Error);
            }
            _ = new VersionService(this, _client).CheckAsync("ExtendNetease_DGJModule");
            _ = InitializeWindow();
        }

        private async Task InitializeWindow()
        {
            if (!string.IsNullOrEmpty(_config.Config.Cookie))
            {
                _session.SetCookie(_config.Config.Cookie);
                try
                {
                    await _session.RefreshAsync();
                    _mainWindow.OnLoginStatusChanged();
                }
                catch (InvalidCookieException)
                {
                    _session.SetCookie(null);
                }
                catch (HttpRequestException e)
                {
                    MessageBox.Show($"无法检查登录状态:{e.Message}\r\n这是由于网络原因导致检查失败, 如果多次出现, 请检查你的网络连接喵", "登录 - 本地网易云喵块", 0, MessageBoxImage.Warning);
                }
                catch (Exception e)
                {
                    MessageBox.Show($"无法检查登录状态:{e}", "登录 - 本地网易云喵块", 0, MessageBoxImage.Warning);
                }
            }
            _mainWindow.ActivateWindow();
        }

        public override void DeInit()
        {
            _config.Config.Cookie = _session.GetCookie();
            _config.SaveConfig();
            _client.Dispose();
            _mainWindow?.UnbindEventsAndClose(); // 插件未能正常加载时, Window实例不会被创建
        }

        public override void Admin()
        {
            _mainWindow.Show();
            _mainWindow.Topmost = true;
            _mainWindow.Topmost = false;
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
                DMPlugin dgjPlugin = Bililive_dm.App.Plugins.FirstOrDefault(p => p is DGJMain);
                if (dgjPlugin == null) // 没有点歌姬
                {
                    throw new DllNotFoundException();
                }
                object dgjWindow = null;
                try
                {
                    dgjWindow = dgjAssembly.DefinedTypes.FirstOrDefault(p => p.Name == "DGJMain").GetField("window", BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(dgjPlugin);
                }
                catch (ReflectionTypeLoadException e) // 缺少登录中心时
                {
                    dgjWindow = e.Types.FirstOrDefault(p => p.Name == "DGJMain").GetField("window", BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(dgjPlugin);
                }
                object searchModules = dgjWindow.GetType().GetProperty("SearchModules", BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public).GetValue(dgjWindow);
                ObservableCollection<SearchModule> searchModules2 = (ObservableCollection<SearchModule>)searchModules.GetType().GetProperty("Modules", BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public).GetValue(searchModules);
                SearchModule nullModule = (SearchModule)searchModules.GetType().GetProperty("NullModule", BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance).GetValue(searchModules);
                SearchModule lwlModule = searchModules2.FirstOrDefault(p => p != nullModule);
                ExtendNeteaseModule module = new ExtendNeteaseModule(this, _config, _client);
                if (lwlModule != null)
                {
                    Action<string> logHandler = (Action<string>)lwlModule.GetType().GetProperty("_log", BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(lwlModule);
                    module.SetLogHandler(logHandler);
                }
                searchModules2.Insert(2, module);
            }
            catch (DllNotFoundException)
            {
                throw;
            }
            catch (Exception e)
            {
                MessageBox.Show($"注入到点歌姬失败了喵\n{e}", "本地网易云喵块", 0, MessageBoxImage.Error);
                throw;
            }
        }
    }
}
