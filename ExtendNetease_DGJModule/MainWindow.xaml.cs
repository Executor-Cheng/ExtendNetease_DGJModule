using ExtendNetease_DGJModule.Clients;
using ExtendNetease_DGJModule.Exceptions;
using ExtendNetease_DGJModule.Models;
using ExtendNetease_DGJModule.NeteaseMusic.Services;
using ExtendNetease_DGJModule.Services;
using QRCoder;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ExtendNetease_DGJModule
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ConfigService _config;

        private readonly NeteaseSession _session;

        private readonly HttpClientv2 _client;

        private QRCodeLoginService _qrLoginService;

        private CancellationTokenSource _cts;

        public MainWindow(ConfigService config, NeteaseSession session, HttpClientv2 client)
        {
            InitializeComponent();
            _config = config;
            _session = session;
            _client = client;
            QualityPanel.DataContext = _config.Config;
        }

        /// <summary>
        /// 更改前端显示
        /// </summary>
        /// <param name="status">登录状态</param>
        public void OnLoginStatusChanged()
        {
            if (this.Dispatcher.CheckAccess())
            {
                Title = _session.LoginStatus ? $"管理界面 - 本地网易云喵块    用户:{_session.UserName}[{_session.UserId}]" : "管理界面 - 本地网易云喵块    用户:未登录";
            }
            else
            {
                this.Dispatcher.Invoke(OnLoginStatusChanged);
            }
        }

        /// <summary>
        /// 右键登录按钮=登出
        /// </summary>
        private async void Logout_Click(object sender, RoutedEventArgs e)
        {
            if (_session.LoginStatus)
            {
                if (MessageBox.Show("确定要注销喵？", "管理界面 - 本地网易云喵块", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _session.LogoutAsync();
                    }
                    catch
                    {

                    }
                    OnLoginStatusChanged();
                }
            }
            else
            {
                MessageBox.Show("还没登录喵~", "管理界面 - 本地网易云喵块", 0, MessageBoxImage.Warning);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        public void ActivateWindow()
        {
            this.IsEnabled = true;
        }

        public void UnbindEventsAndClose()
        {
            this.Closing -= Window_Closing;
            this.Close();
        }

        private void QRCodeLogin_Click(object sender, RoutedEventArgs e)
        {
            QRCodeLoginBtn.IsEnabled = false;
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
            }
            _cts = new CancellationTokenSource();
            _qrLoginService = new QRCodeLoginService(_client, _session);
            _ = UnikeyCheckLoop(_cts.Token);
        }

        private void CookieLogin_Click(object sender, RoutedEventArgs e)
        {
            new CookieLoginWindow(this, _session).ShowDialog();
        }

        private async Task UnikeyCheckLoop(CancellationToken token)
        {
            try
            {
                using Bitmap qrCode = await _qrLoginService.CreateQRCodeAsync(token);
                MemoryStream ms = new MemoryStream();
                qrCode.Save(ms, ImageFormat.Png);
                QRCodeBox.Source = (ImageSource)new ImageSourceConverter().ConvertFrom(ms);
            }
            finally
            {
                QRCodeLoginBtn.IsEnabled = true;
            }
            try
            {
                while (true)
                {
                    UnikeyStatus status = await _qrLoginService.GetStatusAsync(token);
                    if (status == UnikeyStatus.Authorized)
                    {
                        try
                        {
                            await _session.RefreshAsync(token);
                            MessageBox.Show("登录成功OvO", "管理界面 - 本地网易云喵块", 0, MessageBoxImage.Asterisk);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show($"无法获取用户信息TvT\r\n{e}", "管理界面 - 本地网易云喵块", 0, MessageBoxImage.Error);
                            break;
                        }
                        OnLoginStatusChanged();
                        break;
                    }
                    if (status == UnikeyStatus.Expired)
                    {
                        break;
                    }
                    await Task.Delay(3000, token);
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception e)
            {
                MessageBox.Show($"无法获取二维码登录状态QAQ\r\n{e}", "管理界面 - 本地网易云喵块", 0, MessageBoxImage.Error);
            }
            QRCodeBox.Source = null;
        }
    }
}
