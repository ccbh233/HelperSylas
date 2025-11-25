using HelperSylas;
using HelperSylas.Core;
using HelperSylas.Models;
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
        private string _lastPhase = "None"; // 记录上一次状态

        private bool _isLcConnected;
        public bool IsLcConnected
        {
            get => _isLcConnected;
            set => SetProperty(ref _isLcConnected, value);
        }

        // === 状态栏 ===
        private string _statusText = "初始化中...";
        public string StatusText { get => _statusText; set => SetProperty(ref _statusText, value); }

        private Brush _statusColor = Brushes.Gray;
        public Brush StatusColor { get => _statusColor; set => SetProperty(ref _statusColor, value); }

        // [新增] 详细连接信息
        private string _connectionDetail = "未连接";
        public string ConnectionDetail { get => _connectionDetail; set => SetProperty(ref _connectionDetail, value); }

        // === 导航控制 (0=主页, 1=查询, 2=辅助) ===
        private int _selectedTabIndex = 0;
        public int SelectedTabIndex { get => _selectedTabIndex; set => SetProperty(ref _selectedTabIndex, value); }

        // === 页面数据 ===
        private SummonerProfileViewModel? _myProfile;
        public SummonerProfileViewModel? MyProfile { get => _myProfile; set => SetProperty(ref _myProfile, value); }

        public ObservableCollection<SummonerProfileViewModel> SearchTabs { get; } = new();
        private SummonerProfileViewModel? _currentSearchTab;
        public SummonerProfileViewModel? CurrentSearchTab { get => _currentSearchTab; set => SetProperty(ref _currentSearchTab, value); }

        // === 辅助功能 ===
        private bool _autoAccept;
        public bool IsAutoAcceptEnabled { get => _autoAccept; set => SetProperty(ref _autoAccept, value); }

        private string _gamePhaseText = "等待中";
        public string GamePhaseText { get => _gamePhaseText; set => SetProperty(ref _gamePhaseText, value); }

        // === 搜索 ===
        private string _searchText = "";
        public string SearchText { get => _searchText; set => SetProperty(ref _searchText, value); }

        public ICommand SearchCommand { get; }
        public ICommand SwitchTabCommand { get; }

#pragma warning disable CS8618
        public MainViewModel()
        {
            StatusText = "设计预览模式";
            // 给设计器一些假数据防止报错
            SearchCommand = new RelayCommand(_ => { });
            SwitchTabCommand = new RelayCommand(_ => { });
        }
#pragma warning restore CS8618

        public MainViewModel(ILcuService service)
        {
            _lcuService = service;
            _cts = new CancellationTokenSource();

            SearchCommand = new RelayCommand(async _ => await DoSearch());
            SwitchTabCommand = new RelayCommand(param => SelectedTabIndex = int.Parse(param?.ToString() ?? "0"));

            Task.Run(() => LoopLogic(_cts.Token));
        }

        private async Task LoopLogic(CancellationToken token)
        {
            _gameVer = await _lcuService.GetDataDragonVersionAsync();

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var auth = await _lcuService.GetAuthInfoAsync();

                    if (!IsLcConnected)
                    {
                        var me = await _lcuService.GetSummonerInfoAsync(auth);
                        if (me != null)
                        {
                            IsLcConnected = true;
                            StatusText = "已连接 LCU";
                            StatusColor = Brushes.Green;
                            // 显示详细连接信息
                            ConnectionDetail = $"Port: {auth.Port} | PID: {auth.Pid} | Protocol: {auth.Protocol}";

                            var myVm = new SummonerProfileViewModel(_lcuService, auth, me, _gameVer);
                            await myVm.RefreshData();
                            Application.Current.Dispatcher.Invoke(() => MyProfile = myVm);
                        }
                    }

                    // === 状态检测与自动功能 ===
                    string currentPhase = await _lcuService.GetGameFlowPhaseAsync(auth);
                    GamePhaseText = TranslatePhase(currentPhase);

                    // 1. 自动接受
                    if (IsAutoAcceptEnabled && currentPhase == "ReadyCheck")
                    {
                        await _lcuService.AcceptMatchAsync(auth);
                        GamePhaseText = "已自动接受!";
                    }

                    // 2. 自动刷新逻辑 (检测状态改变)
                    // 当从 "EndOfGame"(结算) 回到 "Lobby"(大厅) 时，刷新战绩
                    if (_lastPhase == "EndOfGame" && currentPhase == "Lobby")
                    {
                        if (MyProfile != null) await MyProfile.RefreshData();
                    }
                    // 当从 "None" 变成 "Lobby" (刚登录)
                    else if (_lastPhase == "None" && currentPhase == "Lobby")
                    {
                        if (MyProfile != null) await MyProfile.RefreshData();
                    }

                    _lastPhase = currentPhase; // 更新记录
                }
                catch
                {
                    IsLcConnected = false;
                    StatusText = "未连接";
                    StatusColor = Brushes.Gray;
                    ConnectionDetail = "等待游戏启动...";
                    _lastPhase = "None";
                }
                await Task.Delay(2000, token);
            }
        }

        private async Task DoSearch()
        {
            // ... (保持之前的搜索逻辑不变) ...
            if (string.IsNullOrWhiteSpace(SearchText) || !SearchText.Contains("#")) return;
            try
            {
                var auth = await _lcuService.GetAuthInfoAsync();
                var summoner = await _lcuService.GetSummonerByNameAsync(auth, SearchText);
                if (summoner == null) return;

                var existing = SearchTabs.FirstOrDefault(x => x.Puuid == summoner.Puuid);
                if (existing != null) { CurrentSearchTab = existing; return; }

                var newTab = new SummonerProfileViewModel(_lcuService, auth, summoner, _gameVer);
                newTab.RequestClose += (vm) => SearchTabs.Remove(vm);
                SearchTabs.Add(newTab);
                CurrentSearchTab = newTab;
                await newTab.RefreshData();
                SearchText = "";
            }
            catch { }
        }

        private string TranslatePhase(string phase) => phase switch
        {
            "Lobby" => "房间中",
            "Matchmaking" => "匹配中",
            "ReadyCheck" => "找到对局",
            "ChampSelect" => "选人中",
            "InGame" => "游戏中",
            "EndOfGame" => "结算中",
            _ => "大厅"
        };
    }
}