using HelperSylas.Models;
using System;
using System.Management; // 需安装 NuGet: System.Management
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HelperSylas
{
    public class LcuService : ILcuService
    {
        // 1. 通过命令行参数获取 LCU 端口和密码 (最稳健的方式)
        public async Task<LcuAuthInfo> GetAuthInfoAsync()
        {
            string query = "SELECT CommandLine FROM Win32_Process WHERE Name = 'LeagueClientUx.exe'";
            string? commandLine = null;

            using (var searcher = new ManagementObjectSearcher(query))
            using (var results = searcher.Get())
            {
                foreach (ManagementObject obj in results)
                {
                    commandLine = obj["CommandLine"]?.ToString();
                    if (!string.IsNullOrEmpty(commandLine)) break;
                }
            }

            if (string.IsNullOrEmpty(commandLine))
            {
                throw new Exception("未找到游戏进程，请确保游戏已启动并以管理员身份运行本工具。");
            }

            // 1. 匹配端口
            var portMatch = Regex.Match(commandLine, @"--app-port=([0-9]+)");
            // 2. 匹配密码
            var tokenMatch = Regex.Match(commandLine, @"--remoting-auth-token=([\w-]+)");
            // 3. 匹配 PID (可选，如果只是为了显示，匹配不到也没关系)
            var pidMatch = Regex.Match(commandLine, @"--app-pid=([0-9]+)"); // 注意：有时参数可能是 --app-pid

            // 检查必要参数
            if (!portMatch.Success || !tokenMatch.Success)
            {
                throw new Exception("无法解析启动参数 (Port/Token)");
            }

            return new LcuAuthInfo
            {
                Port = int.Parse(portMatch.Groups[1].Value),
                Password = tokenMatch.Groups[1].Value,
                // 如果匹配到了 PID 就赋值，没匹配到就给 0，防止崩溃
                Pid = pidMatch.Success ? int.Parse(pidMatch.Groups[1].Value) : 0,
                Protocol = "https"
            };
        }

        // 2. 获取召唤师信息 (修复了 UTF-8 中文乱码)
        public async Task<SummonerInfo?> GetSummonerInfoAsync(LcuAuthInfo authInfo)
        {
            return await GetAsync<SummonerInfo>(authInfo, "/lol-summoner/v1/current-summoner");
        }

        public async Task<SummonerInfo?> GetSummonerByNameAsync(LcuAuthInfo auth, string nameTag)
        {
            // LCU 查询接口需要 URL 编码
            // 接口格式：/lol-summoner/v1/summoners?name=名字%23标签
            string encodedName = System.Web.HttpUtility.UrlEncode(nameTag);
            // 注意：如果没有引用 System.Web，可以用 Uri.EscapeDataString(nameTag)

            return await GetAsync<SummonerInfo>(auth, $"/lol-summoner/v1/summoners?name={encodedName}");
        }

        public async Task<RankedStats?> GetRankedStatsAsync(LcuAuthInfo auth, string? puuid = null)
        {
            // 如果没有 puuid，查自己；如果有 puuid，查别人
            string endpoint = string.IsNullOrEmpty(puuid)
                ? "/lol-ranked/v1/current-ranked-stats"
                : $"/lol-ranked/v1/ranked-stats/{puuid}";

            return await GetAsync<RankedStats>(auth, endpoint);
        }

        public async Task<MatchHistoryRoot?> GetMatchHistoryAsync(LcuAuthInfo auth, string? puuid, int begIndex, int endIndex)
        {
            string idPart = string.IsNullOrEmpty(puuid) ? "current-summoner" : puuid;
            // URL 参数控制分页
            string endpoint = $"/lol-match-history/v1/products/lol/{idPart}/matches?begIndex={begIndex}&endIndex={endIndex}";
            return await GetAsync<MatchHistoryRoot>(auth, endpoint);
        }

        // 3. 获取排位信息
        public async Task<RankedStats?> GetRankedStatsAsync(LcuAuthInfo authInfo)
        {
            // 查自己不需要 puuid
            return await GetAsync<RankedStats>(authInfo, "/lol-ranked/v1/current-ranked-stats");
        }

        // 4. 获取 DataDragon 最新版本号
        public async Task<string> GetDataDragonVersionAsync()
        {
            try
            {
                using var client = new HttpClient();
                var json = await client.GetStringAsync("https://ddragon.leagueoflegends.com/api/versions.json");
                using var doc = JsonDocument.Parse(json);
                return doc.RootElement[0].ToString(); // 返回最新的版本号，如 "14.23.1"
            }
            catch
            {
                return "14.23.1"; // 默认值防止报错
            }
        }

        // 通用私有请求方法
        private async Task<T?> GetAsync<T>(LcuAuthInfo auth, string endpoint, bool isRawString = false)
        {
            var handler = new HttpClientHandler
            {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) => true
            };

            using var client = new HttpClient(handler);
            string url = $"{auth.Protocol}://127.0.0.1:{auth.Port}{endpoint}";
            var authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($"riot:{auth.Password}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

            try
            {
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode) return default;

                if (isRawString) // 如果只需要字符串，不进行 JSON 解析
                {
                    string str = await response.Content.ReadAsStringAsync();
                    // 这里的 str 会带引号，比如 "Lobby"，我们需要去掉引号
                    return (T)(object)str.Replace("\"", "");
                }

                byte[] bytes = await response.Content.ReadAsByteArrayAsync();
                string json = Encoding.UTF8.GetString(bytes);
                return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch { return default; }
        }

        // 通用 POST 请求方法
        private async Task PostAsync(LcuAuthInfo auth, string endpoint, string jsonBody)
        {
            var handler = new HttpClientHandler
            {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) => true
            };

            using var client = new HttpClient(handler);
            string url = $"{auth.Protocol}://127.0.0.1:{auth.Port}{endpoint}";
            var authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($"riot:{auth.Password}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            try { await client.PostAsync(url, content); } catch { /* 忽略报错 */ }
        }

        // 获取游戏流状态 (Lobby, ReadyCheck, ChampSelect, InGame)
        public async Task<string> GetGameFlowPhaseAsync(LcuAuthInfo authInfo)
        {
            // 这是一个返回纯字符串的接口
            string? result = await GetAsync<string>(authInfo, "/lol-gameflow/v1/gameflow-phase", isRawString: true);
            return result ?? "None";
        }

        // 接受对局
        public async Task AcceptMatchAsync(LcuAuthInfo authInfo)
        {
            // 这是一个 POST 请求，body 为空
            await PostAsync(authInfo, "/lol-matchmaking/v1/ready-check/accept", "");
        }

        // 修改实现方法的泛型
        public async Task<MatchHistoryRoot?> GetMatchHistoryAsync(LcuAuthInfo authInfo, int count = 5)
        {
            string endpoint = $"/lol-match-history/v1/products/lol/current-summoner/matches?begIndex=0&endIndex={count}";
            // 泛型改为 MatchHistoryRoot
            return await GetAsync<MatchHistoryRoot>(authInfo, endpoint);
        }
    }
}