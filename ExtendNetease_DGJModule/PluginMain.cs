using BilibiliDM_PluginFramework;
using DGJv3;
using ExtendNetease_DGJModule.NeteaseMusic;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace ExtendNetease_DGJModule
{
    public class PluginMain : DMPlugin
    {
        public static MainWindow MainWindow { get; private set; }

        public static ExtendNeteaseModule ExtendNeteaseModule { get; private set; }
        
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
                InjectDGJ();
                MainWindow = new MainWindow();
                MainWindow.OnLoginStatusChanged(MainConfig.Instance.LoginSession.LoginStatus);
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
            MainWindow?.UnbindEventsAndClose(); // 插件未能正常加载时, Window实例不会被创建
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
                DMPlugin dgjPlugin = Bililive_dm.App.Plugins.FirstOrDefault(p => p.GetType() == typeof(DGJMain));
                if (dgjPlugin == null) // 没有点歌姬
                {
                    throw new DllNotFoundException();
                }
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
                ExtendNeteaseModule = new ExtendNeteaseModule();
                if (lwlModule != null)
                {
                    Action<string> logHandler = (Action<string>)lwlModule.GetType().GetProperty("_log", BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(lwlModule);
                    ExtendNeteaseModule.SetLogHandler(logHandler);
                }
                searchModules2.Insert(2, ExtendNeteaseModule);
            }
            catch (DllNotFoundException)
            {
                throw;
            }
            catch (Exception Ex)
            {
                MessageBox.Show($"注入到点歌姬失败了喵\n{Ex}", "本地网易云喵块", 0, MessageBoxImage.Error);
                throw;
            }
        }
    }
}
