namespace NiyaCRM.Core.Configurations
{
    public class RedisSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Password { get; set; } = string.Empty;
        public int Database { get; set; }
        public bool Ssl { get; set; }
        public int ConnectTimeout { get; set; }
    }
}
