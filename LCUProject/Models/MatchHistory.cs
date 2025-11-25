using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HelperSylas.Models
{
    // 1. 最外层的根对象
    public class MatchHistoryRoot
    {
        [JsonPropertyName("games")]
        public MatchHistoryWrapper? Wrapper { get; set; }
    }

    // 2. 中间层包装器
    public class MatchHistoryWrapper
    {
        [JsonPropertyName("games")]
        public List<MatchHistoryGame>? Games { get; set; }
    }

    // 3. 单场比赛信息 (保持不变)
    public class MatchHistoryGame
    {
        [JsonPropertyName("gameId")]
        public long? GameId { get; set; }

        [JsonPropertyName("gameCreation")]
        public long? GameCreation { get; set; }

        [JsonPropertyName("gameDuration")]
        public int? GameDuration { get; set; }

        [JsonPropertyName("queueId")]
        public int? QueueId { get; set; }

        [JsonPropertyName("participants")]
        public List<MatchHistoryParticipant>? Participants { get; set; }
    }

    public class MatchHistoryParticipant
    {
        [JsonPropertyName("championId")]
        public int? ChampionId { get; set; }

        [JsonPropertyName("stats")]
        public MatchHistoryStats? Stats { get; set; }
    }

    public class MatchHistoryStats
    {
        [JsonPropertyName("win")] public bool? Win { get; set; }
        [JsonPropertyName("kills")] public int? Kills { get; set; }
        [JsonPropertyName("deaths")] public int? Deaths { get; set; }
        [JsonPropertyName("assists")] public int? Assists { get; set; }

        // 经济、等级
        [JsonPropertyName("goldEarned")] public int? GoldEarned { get; set; }
        [JsonPropertyName("champLevel")] public int? ChampLevel { get; set; }

        // 装备 (0-6)
        [JsonPropertyName("item0")] public int? Item0 { get; set; }
        [JsonPropertyName("item1")] public int? Item1 { get; set; }
        [JsonPropertyName("item2")] public int? Item2 { get; set; }
        [JsonPropertyName("item3")] public int? Item3 { get; set; }
        [JsonPropertyName("item4")] public int? Item4 { get; set; }
        [JsonPropertyName("item5")] public int? Item5 { get; set; }
        [JsonPropertyName("item6")] public int? Item6 { get; set; } // 饰品

        // 符文 (Perks)
        [JsonPropertyName("perk0")] public int? Perk0 { get; set; } // 基石符文
        [JsonPropertyName("perkSubStyle")] public int? PerkSubStyle { get; set; } // 副系
    }
}