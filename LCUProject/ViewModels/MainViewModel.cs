using HelperSylas.Services;
using HelperSylas.ViewModels;
using HelperSylas.Core;
using HelperSylas.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace HelperSylas.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly ILcuService? _lcuService;
        private CancellationTokenSource? _cts;
        private string _gameVer = "";
        private string _lastPhase = "None";

        // 事件：通知连接状态改变
        public event Action<bool>? ConnectionStatusChanged;

        private bool _isLcConnected;
        public bool IsLcConnected { get => _isLcConnected; set => SetProperty(ref _isLcConnected, value); }

        private string _statusText = "初始化中...";
        public string StatusText { get => _statusText; set => SetProperty(ref _statusText, value); }

        private string _connectionDetail = "未连接";
        public string ConnectionDetail { get => _connectionDetail; set => SetProperty(ref _connectionDetail, value); }

        private int _selectedTabIndex = 0;
        public int SelectedTabIndex { get => _selectedTabIndex; set => SetProperty(ref _selectedTabIndex, value); }

        private SummonerProfileViewModel? _myProfile;
        public SummonerProfileViewModel? MyProfile { get => _myProfile; set => SetProperty(ref _myProfile, value); }

        public ObservableCollection<SummonerProfileViewModel> SearchTabs { get; } = new();
        private SummonerProfileViewModel? _currentSearchTab;
        public SummonerProfileViewModel? CurrentSearchTab { get => _currentSearchTab; set => SetProperty(ref _currentSearchTab, value); }

        private bool _autoAccept;
        public bool IsAutoAcceptEnabled { get => _autoAccept; set => SetProperty(ref _autoAccept, value); }

        private string _gamePhaseText = "等待中";
        public string GamePhaseText { get => _gamePhaseText; set => SetProperty(ref _gamePhaseText, value); }

        private string _searchText = "";
        public string SearchText { get => _searchText; set => SetProperty(ref _searchText, value); }

        public ICommand SearchCommand { get; }
        public ICommand SwitchTabCommand { get; }

        public event Action<long>? RequestOpenMatchWindow;

#pragma warning disable CS8618
        public MainViewModel() { StatusText = "设计预览模式"; }
#pragma warning restore CS8618

        public MainViewModel(ILcuService service)
        {
            _lcuService = service;
            _cts = new CancellationTokenSource();
            SearchCommand = new RelayCommand(async _ => await DoSearch());
            SwitchTabCommand = new RelayCommand(param => SelectedTabIndex = int.Parse(param?.ToString() ?? "0"));
            Task.Run(() => LoopLogic(_cts.Token));
        }

        // 创建 VM 时绑定事件
        private void SetupProfileEvents(SummonerProfileViewModel vm)
        {
            // 当子 VM 请求打开详情时，转发给 View
            vm.RequestOpenDetail += (gameId) => RequestOpenMatchWindow?.Invoke(gameId);
        }

        private async Task LoopLogic(CancellationToken token)
        {
            if (_lcuService != null) _gameVer = await _lcuService.GetDataDragonVersionAsync();

            while (!token.IsCancellationRequested && _lcuService != null)
            {
                // ================================================================
                // 1. 第一层：连接保活检测 (这是决定是否显示“等待动画”的唯一标准)
                // ================================================================
                LcuAuthInfo? auth = null;
                try
                {
                    // 尝试获取端口密码。如果这里报错，说明游戏没开，或者权限不足。
                    auth = await _lcuService.GetAuthInfoAsync();
                }
                catch
                {
                    // [断开连接逻辑]
                    if (IsLcConnected)
                    {
                        IsLcConnected = false;
                        ConnectionStatusChanged?.Invoke(false);
                        StatusText = "未连接";
                        ConnectionDetail = "等待游戏启动...";
                        _lastPhase = "None";

                        // 清理数据，防止下次连接显示旧数据
                        Application.Current.Dispatcher.Invoke(() => MyProfile = null);
                    }

                    // 没找到游戏，休息久一点再试
                    await Task.Delay(2000, token);
                    continue; // 跳过本次循环剩下的代码
                }

                // ================================================================
                // 2. 第二层：数据业务逻辑 (如果这里出错，绝对不能断开连接！)
                // ================================================================
                try
                {
                    // 如果之前是断开状态，现在连上了 -> 初始化
                    if (!IsLcConnected)
                    {
                        // 尝试获取个人信息
                        var me = await _lcuService.GetSummonerInfoAsync(auth);

                        // 注意：刚启动游戏时，LCU API 可能还没准备好，me 会是 null
                        // 这种情况下，我们保持 IsLcConnected = false，等待下一次循环
                        if (me != null)
                        {
                            IsLcConnected = true;
                            ConnectionStatusChanged?.Invoke(true);

                            StatusText = "已连接 LCU";
                            ConnectionDetail = $"Port: {auth.Port} | PID: {auth.Pid}";

                            // 初始化主页 VM
                            var myVm = new SummonerProfileViewModel(_lcuService, auth, me, _gameVer);

                            // 绑定事件
                            SetupProfileEvents(myVm);

                            // 异步加载数据，不要 await 卡住主循环，防止 UI 假死
                            _ = myVm.RefreshData();

                            Application.Current.Dispatcher.Invoke(() => MyProfile = myVm);
                        }
                        else
                        {
                            // 游戏进程在，但 API 还没准备好
                            StatusText = "正在初始化 LCU API...";
                            await Task.Delay(1000, token);
                            continue;
                        }
                    }

                    // --- 下面是已连接状态下的实时逻辑 ---

                    // 获取游戏状态
                    string currentPhase = await _lcuService.GetGameFlowPhaseAsync(auth);
                    GamePhaseText = TranslatePhase(currentPhase); // 记得要有这个翻译方法，或者直接用 currentPhase

                    // 自动接受
                    if (IsAutoAcceptEnabled && currentPhase == "ReadyCheck")
                    {
                        await _lcuService.AcceptMatchAsync(auth);
                    }

                    // 自动刷新 (对局结束回大厅)
                    if ((_lastPhase == "EndOfGame" || _lastPhase == "None") && currentPhase == "Lobby" && MyProfile != null)
                    {
                        _ = MyProfile.RefreshData(); // 异步刷新
                    }

                    _lastPhase = currentPhase;
                }
                catch (Exception ex)
                {
                    // [关键] 这里捕获的是 API 请求失败（比如超时、404）
                    // 我们只记录日志，**千万不要** 设置 IsLcConnected = false
                    System.Diagnostics.Debug.WriteLine("LCU API 轮询警告: " + ex.Message);

                    // 可以更新状态栏提示用户网络波动，但别断开
                    StatusText = "连接不稳定，重试中...";
                }

                await Task.Delay(2000, token);
            }
        }

        private async Task DoSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchText) || !SearchText.Contains("#") || _lcuService == null) return;
            try
            {
                var auth = await _lcuService.GetAuthInfoAsync();
                var summoner = await _lcuService.GetSummonerByNameAsync(auth, SearchText);
                if (summoner == null) return;

                var existing = SearchTabs.FirstOrDefault(x => x.Puuid == summoner.Puuid);
                if (existing != null) { CurrentSearchTab = existing; return; }

                var newTab = new SummonerProfileViewModel(_lcuService, auth, summoner, _gameVer);
                newTab.RequestClose += (vm) => SearchTabs.Remove(vm);
                // 绑定事件
                SetupProfileEvents(newTab);
                SearchTabs.Add(newTab);
                CurrentSearchTab = newTab;
                await newTab.RefreshData();
                SearchText = "";
            }
            catch { }
        }

        // 在 MainViewModel 类内部添加这个方法
        private string TranslatePhase(string phase)
        {
            return phase switch
            {
                "Lobby" => "房间中",
                "Matchmaking" => "匹配中",
                "ReadyCheck" => "找到对局",
                "ChampSelect" => "选人中",
                "InGame" => "游戏中",
                "EndOfGame" => "结算中",
                "None" => "大厅",
                _ => phase // 其他未知状态显示英文原名
            };
        }
    }
}