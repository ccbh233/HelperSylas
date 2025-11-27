using HelperSylas.Models;
using HelperSylas.Core;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace HelperSylas.ViewModels
{
    public class MatchDetailViewModel : ObservableObject
    {
        public ObservableCollection<MatchPlayerViewModel> Team100 { get; } = new();
        public ObservableCollection<MatchPlayerViewModel> Team200 { get; } = new();
        public ObservableCollection<string> Bans100 { get; } = new();
        public ObservableCollection<string> Bans200 { get; } = new();

        public string Team100Result { get; set; } = "VICTORY";
        public string Team200Result { get; set; } = "DEFEAT";
        public string Team100Obj { get; set; } = "";
        public string Team200Obj { get; set; } = "";

        public string GameModeText { get; set; } // "单双排"
        public string GameDurationText { get; set; } // "25分30秒"
        public string GameDateText { get; set; } // "2023-10-01"

        public MatchDetailViewModel(MatchHistoryGame? game, string ver)
        {
            if (game != null)
            {
                LoadData(game, ver);
            }
        }

        private void LoadData(MatchHistoryGame game, string ver)
        {
            if (game.Participants == null) return;

            GameModeText = MatchItemViewModel.GetQueueName(game.QueueId ?? 0);

            int dur = game.GameDuration ?? 0;
            GameDurationText = $"{dur / 60}分{dur % 60}秒";

            long creation = game.GameCreation ?? 0;
            GameDateText = DateTimeOffset.FromUnixTimeMilliseconds(creation).ToLocalTime().ToString("yyyy-MM-dd HH:mm");

            // 1. 计算最大值 (用于进度条)
            int maxDmg = game.Participants.Max(p => p.Stats?.TotalDamageDealtToChampions ?? 0);
            int maxTaken = game.Participants.Max(p => p.Stats?.TotalDamageTaken ?? 0);
            if (maxDmg == 0) maxDmg = 1;
            if (maxTaken == 0) maxTaken = 1;

            // 2. [关键新增] 计算红蓝双方的总伤害和总经济
            // Team 100 (蓝队)
            var p100List = game.Participants.Where(p => p.TeamId == 100).ToList();
            double team100TotalDmg = p100List.Sum(p => (long)(p.Stats?.TotalDamageDealtToChampions ?? 0));
            double team100TotalGold = p100List.Sum(p => (long)(p.Stats?.GoldEarned ?? 0));

            // Team 200 (红队)
            var p200List = game.Participants.Where(p => p.TeamId == 200).ToList();
            double team200TotalDmg = p200List.Sum(p => (long)(p.Stats?.TotalDamageDealtToChampions ?? 0));
            double team200TotalGold = p200List.Sum(p => (long)(p.Stats?.GoldEarned ?? 0));

            // 防止分母为0
            if (team100TotalDmg == 0) team100TotalDmg = 1; if (team100TotalGold == 0) team100TotalGold = 1;
            if (team200TotalDmg == 0) team200TotalDmg = 1; if (team200TotalGold == 0) team200TotalGold = 1;

            var p100VMs = new List<MatchPlayerViewModel>();
            var p200VMs = new List<MatchPlayerViewModel>();

            foreach (var p in game.Participants)
            {
                // 3. [关键修改] 传入对应队伍的总数据
                double currentTeamDmg = p.TeamId == 100 ? team100TotalDmg : team200TotalDmg;
                double currentTeamGold = p.TeamId == 100 ? team100TotalGold : team200TotalGold;

                var vm = new MatchPlayerViewModel(p, game, ver, maxDmg, maxTaken, currentTeamDmg, currentTeamGold);

                if (p.TeamId == 100) p100VMs.Add(vm);
                else p200VMs.Add(vm);
            }

            // 计算 MVP (这里可以保持原样，或者根据新的评分逻辑优化)
            var mvp = p100VMs.Concat(p200VMs).OrderByDescending(x => double.Parse(x.ScoreText)).FirstOrDefault();
            if (mvp != null)
            {
                bool team100Win = game.Teams?.FirstOrDefault(t => t.TeamId == 100)?.Win == "Win";
                if (team100Win && p100VMs.Contains(mvp)) mvp.IsMvp = true;
                else if (!team100Win && p200VMs.Contains(mvp)) mvp.IsMvp = true;
                else mvp.IsSvp = true;
            }

            foreach (var vm in p100VMs) Team100.Add(vm);
            foreach (var vm in p200VMs) Team200.Add(vm);

            if (game.Teams != null)
            {
                var t1 = game.Teams.FirstOrDefault(t => t.TeamId == 100);
                var t2 = game.Teams.FirstOrDefault(t => t.TeamId == 200);

                ProcessBans(t1, Bans100);
                ProcessBans(t2, Bans200);

                Team100Result = t1?.Win == "Win" ? "VICTORY" : "DEFEAT";
                Team200Result = t2?.Win == "Win" ? "VICTORY" : "DEFEAT";

                Team100Obj = $"塔:{t1?.TowerKills} 龙:{t1?.DragonKills} 大龙:{t1?.BaronKills}";
                Team200Obj = $"塔:{t2?.TowerKills} 龙:{t2?.DragonKills} 大龙:{t2?.BaronKills}";
            }
        }

        private void ProcessBans(MatchTeamInfo? team, ObservableCollection<string> collection)
        {
            if (team?.Bans == null) return;
            foreach (var ban in team.Bans)
            {
                if (ban.ChampionId > 0)
                    collection.Add($"https://raw.communitydragon.org/latest/plugins/rcp-be-lol-game-data/global/default/v1/champion-icons/{ban.ChampionId}.png");
            }
        }
    }
}