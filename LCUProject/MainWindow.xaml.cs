using HelperSylas.Services;
using HelperSylas.ViewModels;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Windows;
using System.Windows.Input; // 用于鼠标忙碌状态

namespace HelperSylas
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ILcuService service = new LcuService();
            var vm = new MainViewModel(service);
            this.DataContext = vm;

            // 监听连接状态
            vm.ConnectionStatusChanged += (isConnected) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    if (isConnected) LoadingWebView.NavigateToString("<html><body style='background:#121212'></body></html>");
                    else LoadingWebView.NavigateToString(AnimationAssets.KingslayerHtml);
                });
            };

            // [关键修复] 监听打开详情页请求 (增加异常捕获)
            vm.RequestOpenMatchWindow += async (gameId) =>
            {
                try
                {
                    // 1. 设置鼠标为忙碌状态，告诉用户“别急，正在加载”
                    Mouse.OverrideCursor = Cursors.Wait;

                    var auth = await service.GetAuthInfoAsync();

                    // 获取详情数据
                    var gameDetail = await service.GetGameDetailAsync(auth, gameId);
                    var ver = await service.GetDataDragonVersionAsync();

                    if (gameDetail != null)
                    {
                        // 2. 尝试创建 ViewModel (这里最容易崩)
                        var detailVm = new MatchDetailViewModel(gameDetail, ver);

                        // 3. 显示窗口
                        var win = new MatchDetailWindow { DataContext = detailVm };
                        win.Owner = this;
                        win.Show();
                    }
                    else
                    {
                        MessageBox.Show("未获取到对局详情，LCU接口返回为空。\n(可能是太久之前的对局)", "提示");
                    }
                }
                catch (Exception ex)
                {
                    // 捕获所有错误
                    MessageBox.Show($"打开详情页出错：\n{ex.Message}\n\n{ex.StackTrace}", "错误");
                }
                finally
                {
                    // 恢复鼠标指针
                    Mouse.OverrideCursor = null;
                }
            };

            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            try
            {
                await LoadingWebView.EnsureCoreWebView2Async();
                LoadingWebView.NavigateToString(AnimationAssets.KingslayerHtml);
            }
            catch { }
        }
    }
}