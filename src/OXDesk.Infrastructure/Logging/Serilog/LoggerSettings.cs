using OXDesk.Core.Common;
namespace OXDesk.Infrastructure.Logging.Serilog
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
            public string Default { get; set; } = CommonConstant.ERROR_LEVEL_INFORMATION;
            public string? Microsoft { get; set; } = CommonConstant.ERROR_LEVEL_WARNING;
            public string? MicrosoftAspNetCore { get; set; } = CommonConstant.ERROR_LEVEL_WARNING;
            public string? MicrosoftHostingLifetime { get; set; } = CommonConstant.ERROR_LEVEL_INFORMATION;
            public string? MicrosoftAspNetCoreAuthentication { get; set; } = CommonConstant.ERROR_LEVEL_WARNING;
            public string? MicrosoftAspNetCoreMvcInfrastructure { get; set; } = CommonConstant.ERROR_LEVEL_WARNING;
            public string? MicrosoftEntityFrameworkCore { get; set; } = CommonConstant.ERROR_LEVEL_WARNING;
        }
    }
}