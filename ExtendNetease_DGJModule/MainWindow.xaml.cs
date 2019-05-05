using System;
using System.Windows;
using System.Windows.Input;

namespace ExtendNetease_DGJModule
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            QualityPanel.DataContext = MainConfig.Instance;
            //UserIdBox.DataContext = UserNameBox.DataContext = MainConfig.Instance.LoginSession;
        }

        /// <summary>
        /// 更改前端显示
        /// </summary>
        /// <param name="status">登录状态</param>
        public void OnLoginStatusChanged(bool status)
        {
            if (this.Dispatcher.CheckAccess())
            {
                if (status)
                {
                    Title = $"管理界面 - 本地网易云喵块    用户:{MainConfig.Instance.LoginSession.UserName}[{MainConfig.Instance.LoginSession.UserId}]";
                    LoginBtn.Content = "已登录";
                }
                else
                {
                    Title = $"管理界面 - 本地网易云喵块    用户:未登录";
                    LoginBtn.Content = "登录";
                }
            }
            else
            {
                this.Dispatcher.Invoke(() => OnLoginStatusChanged(status));
            }
        }
        /// <summary>
        /// 左键登录按钮=登录
        /// </summary>
        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            if (!MainConfig.Instance.LoginSession.LoginStatus || MessageBox.Show("你已经登录了,确定要继续登录喵？", "管理界面 - 本地网易云喵块", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    LoginBtn.IsEnabled = false;
                    LoginBtn.Content = "请稍等喵...";
                    if (long.TryParse(LoginUserNameBox.Text, out long phone))
                    {
                        await MainConfig.Instance.LoginSession.LoginAsync(86, phone, LoginPasswordBox.Password);
                    }
                    else
                    {
                        await MainConfig.Instance.LoginSession.LoginAsync(LoginUserNameBox.Text, LoginPasswordBox.Password);
                    }
                    OnLoginStatusChanged(true);
                    LoginUserNameBox.Text = LoginPasswordBox.Password = "";
                    MessageBox.Show($"登录成功(=・ω・=)", "登录 - 本地网易云喵块", 0, MessageBoxImage.Information);
                }
                catch (Exception Ex)
                {
                    OnLoginStatusChanged(false);
                    MessageBox.Show($"登录失败了喵:{Ex.Message}", "登录 - 本地网易云喵块", 0, MessageBoxImage.Warning);
                }
                finally
                {
                    LoginBtn.IsEnabled = true;
                }
            }
        }
        /// <summary>
        /// 右键登录按钮=登出
        /// </summary>
        private async void LogOut_Click(object sender, MouseButtonEventArgs e)
        {
            if (MainConfig.Instance.LoginSession.LoginStatus)
            {
                if (MessageBox.Show("确定要注销喵？", "管理界面 - 本地网易云喵块", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        await MainConfig.Instance.LoginSession.LogOutAsync();
                    }
                    catch
                    {

                    }
                    OnLoginStatusChanged(false);
                }
            }
            else
            {
                MessageBox.Show("还没登录喵~", "管理界面 - 本地网易云喵块", 0, MessageBoxImage.Warning);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            MainConfig.Instance.SaveConfig();
            Hide();
        }

        public void UnbindEventsAndClose()
        {
            this.Closing -= Window_Closing;
            this.Close();
        }

        private void OnEnterKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Login_Click(this, e);
            }
        }
    }
}
