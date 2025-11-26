using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HelperSylas.Models
{
    public class MatchHistoryRoot { [JsonPropertyName("games")] public MatchHistoryWrapper? Wrapper { get; set; } }
    public class MatchHistoryWrapper { [JsonPropertyName("games")] public List<MatchHistoryGame>? Games { get; set; } }

    public class MatchHistoryGame
    {
        [JsonPropertyName("gameId")] public long? GameId { get; set; }
        [JsonPropertyName("gameCreation")] public long? GameCreation { get; set; }
        [JsonPropertyName("gameDuration")] public int? GameDuration { get; set; }
        [JsonPropertyName("queueId")] public int? QueueId { get; set; }
        [JsonPropertyName("gameMode")] public string? GameMode { get; set; }

        [JsonPropertyName("teams")] public List<MatchTeamInfo>? Teams { get; set; }
        [JsonPropertyName("participants")] public List<MatchHistoryParticipant>? Participants { get; set; }

        // [关键] 名字映射表
        [JsonPropertyName("participantIdentities")] public List<MatchParticipantIdentity>? ParticipantIdentities { get; set; }
    }

    public class MatchParticipantIdentity
    {
        [JsonPropertyName("participantId")] public int ParticipantId { get; set; }
        [JsonPropertyName("player")] public MatchPlayerInfo? Player { get; set; }
    }

    public class MatchPlayerInfo
    {
        [JsonPropertyName("summonerName")] public string? SummonerName { get; set; }
        [JsonPropertyName("gameName")] public string? GameName { get; set; }
        [JsonPropertyName("tagLine")] public string? TagLine { get; set; }
    }

    public class MatchTeamInfo
    {
        [JsonPropertyName("teamId")] public int TeamId { get; set; }
        [JsonPropertyName("win")] public string? Win { get; set; }
        [JsonPropertyName("bans")] public List<MatchBanInfo>? Bans { get; set; }

        [JsonPropertyName("towerKills")] public int TowerKills { get; set; }
        [JsonPropertyName("dragonKills")] public int DragonKills { get; set; }
        [JsonPropertyName("baronKills")] public int BaronKills { get; set; }
        [JsonPropertyName("riftHeraldKills")] public int RiftHeraldKills { get; set; }
    }

    public class MatchBanInfo
    {
        [JsonPropertyName("championId")] public int ChampionId { get; set; }
        [JsonPropertyName("pickTurn")] public int PickTurn { get; set; }
    }

    public class MatchHistoryParticipant
    {
        [JsonPropertyName("participantId")] public int ParticipantId { get; set; } // 必须有这个ID来关联名字
        [JsonPropertyName("championId")] public int? ChampionId { get; set; }
        [JsonPropertyName("teamId")] public int TeamId { get; set; }
        [JsonPropertyName("spell1Id")] public int Spell1Id { get; set; }
        [JsonPropertyName("spell2Id")] public int Spell2Id { get; set; }
        [JsonPropertyName("stats")] public MatchHistoryStats? Stats { get; set; }
    }

    public class MatchHistoryStats
    {
        [JsonPropertyName("win")] public bool? Win { get; set; }
        [JsonPropertyName("kills")] public int? Kills { get; set; }
        [JsonPropertyName("deaths")] public int? Deaths { get; set; }
        [JsonPropertyName("assists")] public int? Assists { get; set; }
        [JsonPropertyName("goldEarned")] public int? GoldEarned { get; set; }
        [JsonPropertyName("champLevel")] public int? ChampLevel { get; set; }

        [JsonPropertyName("totalMinionsKilled")] public int? TotalMinionsKilled { get; set; }
        [JsonPropertyName("neutralMinionsKilled")] public int? NeutralMinionsKilled { get; set; }

        [JsonPropertyName("totalDamageDealtToChampions")] public int? TotalDamageDealtToChampions { get; set; }
        [JsonPropertyName("totalDamageTaken")] public int? TotalDamageTaken { get; set; }
        [JsonPropertyName("visionScore")] public int? VisionScore { get; set; }

        [JsonPropertyName("item0")] public int? Item0 { get; set; }
        [JsonPropertyName("item1")] public int? Item1 { get; set; }
        [JsonPropertyName("item2")] public int? Item2 { get; set; }
        [JsonPropertyName("item3")] public int? Item3 { get; set; }
        [JsonPropertyName("item4")] public int? Item4 { get; set; }
        [JsonPropertyName("item5")] public int? Item5 { get; set; }
        [JsonPropertyName("item6")] public int? Item6 { get; set; }

        [JsonPropertyName("perk0")] public int? Perk0 { get; set; } // 基石
        [JsonPropertyName("perkSubStyle")] public int? PerkSubStyle { get; set; } // 副系Style ID
    }
}