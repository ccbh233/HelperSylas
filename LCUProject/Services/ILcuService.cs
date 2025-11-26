using HelperSylas.Models;
using System.Threading.Tasks;

namespace HelperSylas.Services
{
    public interface ILcuService
    {
        Task<LcuAuthInfo> GetAuthInfoAsync();
        Task<SummonerInfo?> GetSummonerInfoAsync(LcuAuthInfo authInfo);
        Task<RankedStats?> GetRankedStatsAsync(LcuAuthInfo auth, string? puuid = null);
        Task<string> GetDataDragonVersionAsync(); // 获取最新版本号
        Task<string> GetGameFlowPhaseAsync(LcuAuthInfo authInfo); // 获取当前游戏状态
        Task AcceptMatchAsync(LcuAuthInfo authInfo); // 执行接受对局
        Task<MatchHistoryRoot?> GetMatchHistoryAsync(LcuAuthInfo auth, string? puuid, int begIndex, int endIndex);
        Task<SummonerInfo?> GetSummonerByNameAsync(LcuAuthInfo auth, string nameTag);
        Task<MatchHistoryGame?> GetGameDetailAsync(LcuAuthInfo auth, long gameId);
    }
}