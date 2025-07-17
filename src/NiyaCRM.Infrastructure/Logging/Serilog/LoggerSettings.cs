namespace NiyaCRM.Infrastructure.Logging.Serilog
{
    public class LoggerSettings
    {
        public bool WriteToFile { get; set; }
        public string LogFilePath { get; set; } = "logs/";
        public bool CompactConsoleLogging { get; set; }
        public string? ElasticSearchUrl { get; set; }
        public string? S3Bucket { get; set; }
        public LogLevelSettings LogLevel { get; set; } = new();

        public class LogLevelSettings
        {
            public string Default { get; set; } = "Information";
            public string? Microsoft { get; set; } = "Warning";
            public string? MicrosoftAspNetCore { get; set; } = "Warning";
            public string? MicrosoftHostingLifetime { get; set; } = "Warning";
            public string? MicrosoftAspNetCoreAuthentication { get; set; } = "Warning";
            public string? MicrosoftAspNetCoreMvcInfrastructure { get; set; } = "Warning";
            public string? MicrosoftEntityFrameworkCore { get; set; } = "Warning";
        }
    }
}