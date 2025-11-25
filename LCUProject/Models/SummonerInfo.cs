using System.Text.Json.Serialization;

namespace HelperSylas.Models
{
    public class SummonerInfo
    {
        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("gameName")]
        public string? GameName { get; set; }

        [JsonPropertyName("tagLine")]
        public string? TagLine { get; set; }

        [JsonPropertyName("summonerLevel")]
        public int SummonerLevel { get; set; }

        [JsonPropertyName("profileIconId")]
        public int ProfileIconId { get; set; }

        [JsonPropertyName("puuid")]
        public string? Puuid { get; set; }

        // 经验值
        [JsonPropertyName("xpSinceLastLevel")]
        public long XpSinceLastLevel { get; set; }
        [JsonPropertyName("xpUntilNextLevel")]
        public long XpUntilNextLevel { get; set; }
    }
}