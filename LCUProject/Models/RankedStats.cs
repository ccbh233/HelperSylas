using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HelperSylas.Models
{
    public class RankedStats
    {
        [JsonPropertyName("queues")]
        public List<QueueInfo>? Queues { get; set; }
    }

    public class QueueInfo
    {
        [JsonPropertyName("queueType")]
        public string? QueueType { get; set; }

        [JsonPropertyName("tier")]
        public string? Tier { get; set; }

        [JsonPropertyName("division")]
        public string? Division { get; set; }

        [JsonPropertyName("leaguePoints")]
        public int LeaguePoints { get; set; }

        [JsonPropertyName("wins")]
        public int Wins { get; set; }

        [JsonPropertyName("losses")]
        public int Losses { get; set; }

        // 自动翻译逻辑
        public string RankTextCN
        {
            get
            {
                if (string.IsNullOrEmpty(Tier)) return "未定级";

                string tierCn = Tier.ToUpper() switch
                {
                    "IRON" => "坚韧黑铁",
                    "BRONZE" => "英勇黄铜",
                    "SILVER" => "不屈白银",
                    "GOLD" => "荣耀黄金",
                    "PLATINUM" => "华贵铂金",
                    "EMERALD" => "流光翡翠",
                    "DIAMOND" => "璀璨钻石",
                    "MASTER" => "超凡大师",
                    "GRANDMASTER" => "傲世宗师",
                    "CHALLENGER" => "最强王者",
                    _ => Tier
                };

                // 大师以上通常不需要显示级数，这里为了统一还是显示
                return $"{tierCn} {Division}";
            }
        }
    }
}