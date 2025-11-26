using HelperSylas.Core;
using HelperSylas.Models;
using HelperSylas.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HelperSylas.ViewModels
{
    public class SummonerProfileViewModel : ObservableObject
    {
        private readonly ILcuService _lcuService;
        private readonly LcuAuthInfo _auth;
        private readonly SummonerInfo _info;
        private readonly string _gameVer;

        public string DisplayName => string.IsNullOrEmpty(_info.GameName) ? _info.DisplayName : $"{_info.GameName} #{_info.TagLine}";
        public string LevelText => $"{_info.SummonerLevel}";
        public string IconUrl => $"https://ddragon.leagueoflegends.com/cdn/{_gameVer}/img/profileicon/{_info.ProfileIconId}.png";
        public string Puuid => _info.Puuid;

        private string _rankText = "加载中...";
        public string RankText { get => _rankText; set => SetProperty(ref _rankText, value); }

        private string _rankLp = "";
        public string RankLp { get => _rankLp; set => SetProperty(ref _rankLp, value); }

        public ObservableCollection<MatchItemViewModel> MatchList { get; } = new();

        private int _currentPage = 1;
        public int CurrentPage { get => _currentPage; set => SetProperty(ref _currentPage, value); }

        private bool _isLoading;
        public bool IsLoading { get => _isLoading; set => SetProperty(ref _isLoading, value); }

        public ICommand RefreshCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PrevPageCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand CopyNameCommand { get; }

        public event Action<SummonerProfileViewModel>? RequestClose;

        public event Action<long>? RequestOpenDetail;

        // 经验百分比 (0-100)
        public double XpProgress { get; set; }

        public SummonerProfileViewModel(ILcuService service, LcuAuthInfo auth, SummonerInfo info, string gameVer)
        {
            // ... 初始化 ...
            _lcuService = service; _auth = auth; _info = info; _gameVer = gameVer;

            // 计算经验百分比
            if (_info.XpUntilNextLevel > 0)
            {
                XpProgress = (double)_info.XpSinceLastLevel / (_info.XpSinceLastLevel + _info.XpUntilNextLevel) * 100;
            }
            else
            {
                XpProgress = 0; // 满级或数据异常
            }

            RefreshCommand = new RelayCommand(async _ => await RefreshData());
            NextPageCommand = new RelayCommand(async _ => await ChangePage(1), _ => !IsLoading);
            PrevPageCommand = new RelayCommand(async _ => await ChangePage(-1), _ => !IsLoading && CurrentPage > 1);
            CloseCommand = new RelayCommand(_ => RequestClose?.Invoke(this));
            CopyNameCommand = new RelayCommand(_ =>
            {
                try
                {
                    Clipboard.SetText(DisplayName);
                    MessageBox.Show($"已复制: {DisplayName}");
                }
                catch { }
            });
        }

        // 提供给外部（MainViewModel）调用的自动刷新方法
        public async Task RefreshData()
        {
            await LoadRank();
            _currentPage = 1;
            OnPropertyChanged(nameof(CurrentPage));
            await LoadHistory();
        }

        private async Task LoadRank()
        {
            try
            {
                var stats = await _lcuService.GetRankedStatsAsync(_auth, _info.Puuid);
                var solo = stats?.Queues?.Find(q => q.QueueType == "RANKED_SOLO_5x5");
                if (solo != null)
                {
                    RankText = solo.RankTextCN;
                    RankLp = $"{solo.LeaguePoints} LP";
                }
                else
                {
                    RankText = "未定级";
                    RankLp = "";
                }
            }
            catch { RankText = "获取失败"; }
        }

        private async Task ChangePage(int delta)
        {
            if (IsLoading) return;
            CurrentPage += delta;
            await LoadHistory();
        }

        private async Task LoadHistory()
        {
            IsLoading = true;
            MatchList.Clear();
            try
            {
                // 加载最近 20 场
                var root = await _lcuService.GetMatchHistoryAsync(_auth, _info.Puuid, 0, 20);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (root?.Wrapper?.Games != null)
                    {
                        foreach (var game in root.Wrapper.Games)
                        {
                            // 传入回调函数
                            MatchList.Add(new MatchItemViewModel(game, _gameVer, (gameId) => RequestOpenDetail?.Invoke(gameId)));
                        }
                    }
                });
            }
            catch { }
            finally { IsLoading = false; }
        }
    }
}