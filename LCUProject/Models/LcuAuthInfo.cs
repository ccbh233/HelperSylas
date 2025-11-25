namespace HelperSylas.Models
{
    public class LcuAuthInfo
    {
        public string? ProcessName { get; set; }
        public int Pid { get; set; }
        public int Port { get; set; }
        public string? Password { get; set; }
        public string Protocol { get; set; } = "https";
    }
}