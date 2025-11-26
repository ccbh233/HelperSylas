using HelperSylas.Models;
using HelperSylas.Core;
using HelperSylas.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace HelperSylas.ViewModels
{
    public class MatchPlayerViewModel : ObservableObject
    {
        public string ChampionIcon { get; set; } = "";
        public string SummonerName { get; set; } = "Unknown";
        public string LevelText { get; set; } = "1";

        // KDA
        public string KdaText { get; set; } = "0/0/0";
        public string KdaRatio { get; set; } = "0.0";
        public Brush KdaColor { get; set; } = Brushes.Gray;

        // 数据
        public string CsText { get; set; } = "0";
        public string GoldText { get; set; } = "0k";
        public string VisionText { get; set; } = "0";

        // 伤害转化率 (UI显示文本)
        public string DmgConversionText { get; set; } = "0%";
        // 数值用于评分计算
        private double _dmgConversionValue;

        // 图标
        public string Spell1Icon { get; set; } = "";
        public string Spell2Icon { get; set; } = "";
        public string RuneMainIcon { get; set; } = "";
        public string RuneSubIcon { get; set; } = "";
        public List<string> Items { get; } = new();

        // 进度条
        public double DamagePercent { get; set; } = 0;
        public string DamageVal { get; set; } = "0";
        public double TakenPercent { get; set; } = 0;
        public string TakenVal { get; set; } = "0";

        // 评分
        public string ScoreText { get; set; } = "3.0";
        public Brush ScoreColor { get; set; } = Brushes.Gray;
        public Brush ResultColor { get; set; } = Brushes.Gray;
        public bool IsMvp { get; set; }
        public bool IsSvp { get; set; }

        // [修改] 构造函数增加 teamTotalDmg 和 teamTotalGold 参数
        public MatchPlayerViewModel(MatchHistoryParticipant p, MatchHistoryGame game, string ver,
                                    int maxDmg, int maxTaken,
                                    double teamTotalDmg, double teamTotalGold)
        {
            var s = p.Stats;
            ChampionIcon = $"https://raw.communitydragon.org/latest/plugins/rcp-be-lol-game-data/global/default/v1/champion-icons/{p.ChampionId}.png";
            LevelText = (s?.ChampLevel ?? 1).ToString();

            // 名字映射
            var identity = game.ParticipantIdentities?.FirstOrDefault(x => x.ParticipantId == p.ParticipantId);
            if (identity?.Player != null)
            {
                SummonerName = !string.IsNullOrEmpty(identity.Player.GameName)
                    ? $"{identity.Player.GameName} #{identity.Player.TagLine}"
                    : identity.Player.SummonerName ?? "Unknown";
            }

            // 胜负颜色
            bool isWin = s?.Win ?? false;
            ResultColor = isWin ? Brushes.DodgerBlue : Brushes.Red;

            // KDA
            int k = s?.Kills ?? 0; int d = s?.Deaths ?? 0; int a = s?.Assists ?? 0;
            KdaText = $"{k} / {d} / {a}";
            double ratio = d == 0 ? (k + a) : (double)(k + a) / d;
            KdaRatio = $"{ratio:F2}:1";
            KdaColor = ratio > 4.0 ? Brushes.OrangeRed : (ratio > 2.5 ? Brushes.DodgerBlue : Brushes.Gray);

            // 基础数据
            int totalCs = (s?.TotalMinionsKilled ?? 0) + (s?.NeutralMinionsKilled ?? 0);
            CsText = totalCs.ToString();
            double gold = s?.GoldEarned ?? 0;
            GoldText = $"{(gold / 1000.0):F1}k";
            VisionText = (s?.VisionScore ?? 0).ToString();

            // =========================================================
            // [核心修改] 伤害转化率计算
            // 公式：(个人伤害占比) / (个人经济占比)
            // =========================================================
            double myDmg = s?.TotalDamageDealtToChampions ?? 0;

            double dmgShare = teamTotalDmg > 0 ? myDmg / teamTotalDmg : 0; // 伤害占比
            double goldShare = teamTotalGold > 0 ? gold / teamTotalGold : 0; // 经济占比

            if (goldShare > 0.01) // 防止分母过小
            {
                _dmgConversionValue = dmgShare / goldShare;
            }
            else
            {
                _dmgConversionValue = 0;
            }
            // 显示为百分比，例如 145%
            DmgConversionText = $"{_dmgConversionValue * 100:F0}% 转";

            // 资源路径
            string cdn = $"https://ddragon.leagueoflegends.com/cdn/{ver}/img";
            Spell1Icon = GetSpellUrl(p.Spell1Id);
            Spell2Icon = GetSpellUrl(p.Spell2Id);
            RuneMainIcon = $"https://ddragon.leagueoflegends.com/cdn/img/perk-images/Styles/{GetRunePath(s?.Perk0)}.png";
            RuneSubIcon = $"https://ddragon.leagueoflegends.com/cdn/img/perk-images/Styles/{GetStyleIcon(s?.PerkSubStyle)}.png";

            var ids = new[] { s?.Item0, s?.Item1, s?.Item2, s?.Item3, s?.Item4, s?.Item5, s?.Item6 };
            foreach (var id in ids)
                Items.Add((id.HasValue && id > 0) ? $"{cdn}/item/{id}.png" : "");

            int taken = s?.TotalDamageTaken ?? 0;
            DamageVal = myDmg.ToString("N0");
            TakenVal = taken.ToString("N0");
            DamagePercent = maxDmg > 0 ? (double)myDmg / maxDmg * 100 : 0;
            TakenPercent = maxTaken > 0 ? (double)taken / maxTaken * 100 : 0;

            double score = CalculateScore(s, game.GameDuration ?? 1800);
            ScoreText = score.ToString("F1");
            ScoreColor = GetScoreColor(score);
        }

        private double CalculateScore(MatchHistoryStats? s, int durationSec)
        {
            if (s == null) return 3.0;
            double score = 3.0;

            int k = s.Kills ?? 0; int d = s.Deaths ?? 0; int a = s.Assists ?? 0;
            double kda = d == 0 ? (k + a) : (double)(k + a * 0.8) / d;
            score += Math.Min(kda, 10) * 0.5;

            // [调整] 转化率评分权重
            // 100% (1.0) 是及格，150% (1.5) 是优秀
            // 这里的权重系数调整为 2.0，即 150% 转化率能提供 3 分
            score += Math.Min(_dmgConversionValue * 2.0, 4.0);

            double vpm = (double)(s.VisionScore ?? 0) / (durationSec / 60.0);
            score += Math.Min(vpm * 2, 2.0);
            if (s.Win == true) score += 1.0;

            return Math.Min(16.0, Math.Max(3.0, score));
        }

        private Brush GetScoreColor(double score)
        {
            if (score >= 13) return Brushes.OrangeRed;
            if (score >= 10) return Brushes.DodgerBlue;
            return Brushes.Gray;
        }

        private string GetStyleIcon(int? styleId)
        {
            return styleId switch
            {
                8000 => "7201_Precision",
                8100 => "7200_Domination",
                8200 => "7202_Sorcery",
                8300 => "7203_Whimsy",
                8400 => "7204_Resolve",
                _ => "7201_Precision"
            };
        }

        private string GetSpellUrl(int id)
        {
            string name = id switch
            {
                4 => "SummonerFlash",
                14 => "SummonerDot",
                12 => "SummonerTeleport",
                11 => "SummonerSmite",
                7 => "SummonerHeal",
                6 => "SummonerHaste",
                21 => "SummonerBarrier",
                3 => "SummonerExhaust",
                1 => "SummonerBoost",
                32 => "SummonerSnowball",
                _ => "SummonerFlash"
            };
            return $"https://ddragon.leagueoflegends.com/cdn/14.23.1/img/spell/{name}.png";
        }

        private string GetRunePath(int? id) => "7201_Precision";
    }
}