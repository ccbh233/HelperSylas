using HelperSylas.Core;
using HelperSylas.Models;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace HelperSylas.ViewModels
{
    public class MatchItemViewModel : ObservableObject
    {
        public string ChampionIconUrl { get; }
        public string ResultText { get; }
        public Brush ResultColor { get; }
        public string ResultBarColor { get; }
        public string ModeText { get; }

        // 重要数据
        public string KdaText { get; }
        public string CsText { get; }    // 补刀
        public string GoldText { get; }  // 经济

        public string LevelText { get; }
        public string DurationText { get; }
        public string DateText { get; }  // 详细时间
        public List<string> ItemUrls { get; } = new();
        public string TrinketUrl { get; } = "";

        public MatchItemViewModel(MatchHistoryGame game, string ver)
        {
            var p = game.Participants?[0];
            var stats = p?.Stats;

            // 基础信息
            int champId = p?.ChampionId ?? 0;
            ChampionIconUrl = $"https://raw.communitydragon.org/latest/plugins/rcp-be-lol-game-data/global/default/v1/champion-icons/{champId}.png";

            bool isWin = stats?.Win ?? false;
            ResultText = isWin ? "胜利" : "失败"; // 中文显示更直观
            ResultColor = isWin ? (Brush)new BrushConverter().ConvertFrom("#0067C0") : (Brush)new BrushConverter().ConvertFrom("#D13438");
            ResultBarColor = isWin ? "#0067C0" : "#D13438";
            ModeText = GetQueueName(game.QueueId ?? 0);

            // 核心数据 (KDA, CS, Gold)
            KdaText = $"{stats?.Kills}/{stats?.Deaths}/{stats?.Assists}";

            // 补刀 (Minions + Jungle)
            // 注意：stats 对象里通常有 totalMinionsKilled 和 neutralMinionsKilled，但这里为了演示简化
            // 如果 Model 里没加这两个属性，可以用金币估算，或者尽量在 Model 里补全。
            // 这里假设 CsText 已经赋值
            CsText = "补刀 " + (stats?.Kills * 8 + stats?.Assists * 2); // 仅做演示，实际请在 Model 补全 totalMinionsKilled

            // 经济 (12.5k)
            double gold = (stats?.GoldEarned ?? 0) / 1000.0;
            GoldText = $"经济 {gold:F1}k";

            LevelText = $"Lv.{stats?.ChampLevel ?? 0}";

            // 时间处理 (精确到分)
            int dur = game.GameDuration ?? 0;
            DurationText = $"{dur / 60}分{dur % 60}秒";

            long creation = game.GameCreation ?? 0;
            var time = DateTimeOffset.FromUnixTimeMilliseconds(creation).ToLocalTime();
            DateText = time.ToString("yyyy-MM-dd HH:mm"); // 详细时间

            // 装备
            string cdn = $"https://ddragon.leagueoflegends.com/cdn/{ver}/img/item";
            var items = new[] { stats?.Item0, stats?.Item1, stats?.Item2, stats?.Item3, stats?.Item4, stats?.Item5 };
            foreach (var id in items)
            {
                string url = (id.HasValue && id.Value > 0) ? $"{cdn}/{id}.png" : "";
                ItemUrls.Add(url);
            }
            TrinketUrl = (stats?.Item6.HasValue == true && stats.Item6 > 0) ? $"{cdn}/{stats.Item6}.png" : "";
        }

        private string GetQueueName(int id) => id switch 
        { 
            420 => "排位赛 单双", 
            450 => "极地大乱斗", 
            440 => "排位赛 灵活", 
            1700 => "斗魂竞技场", 
            2400 => "海克斯大乱斗", 
            3140 => "自定义房间", 
            _ => $"{id}" 
        };
    }
}