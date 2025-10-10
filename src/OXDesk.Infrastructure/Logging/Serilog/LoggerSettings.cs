using OXDesk.Core.Common;
namespace OXDesk.Infrastructure.Logging.Serilog
{
    /// <summary>
    /// Configuration settings for Serilog logger.
    /// </summary>
    public class LoggerSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether to write logs to file.
        /// </summary>
        public bool WriteToFile { get; set; }
        
        /// <summary>
        /// Gets or sets the log file path.
        /// </summary>
        public string LogFilePath { get; set; } = "logs/";
        
        /// <summary>
        /// Gets or sets a value indicating whether to use compact console logging.
        /// </summary>
        public bool CompactConsoleLogging { get; set; }
        
        /// <summary>
        /// Gets or sets the ElasticSearch URL for log shipping.
        /// </summary>
        public string? ElasticSearchUrl { get; set; }
        
        /// <summary>
        /// Gets or sets the S3 bucket for log storage.
        /// </summary>
        public string? S3Bucket { get; set; }
        
        /// <summary>
        /// Gets or sets the log level settings.
        /// </summary>
        public LogLevelSettings LogLevel { get; set; } = new();

        /// <summary>
        /// Log level configuration for different namespaces.
        /// </summary>
        public class LogLevelSettings
        {
            /// <summary>
            /// Gets or sets the default log level.
            /// </summary>
            public string Default { get; set; } = CommonConstant.ERROR_LEVEL_INFORMATION;
            
            /// <summary>
            /// Gets or sets the log level for Microsoft namespace.
            /// </summary>
            public string? Microsoft { get; set; } = CommonConstant.ERROR_LEVEL_WARNING;
            
            /// <summary>
            /// Gets or sets the log level for Microsoft.AspNetCore namespace.
            /// </summary>
            public string? MicrosoftAspNetCore { get; set; } = CommonConstant.ERROR_LEVEL_WARNING;
            
            /// <summary>
            /// Gets or sets the log level for Microsoft.Hosting.Lifetime namespace.
            /// </summary>
            public string? MicrosoftHostingLifetime { get; set; } = CommonConstant.ERROR_LEVEL_INFORMATION;
            
            /// <summary>
            /// Gets or sets the log level for Microsoft.AspNetCore.Authentication namespace.
            /// </summary>
            public string? MicrosoftAspNetCoreAuthentication { get; set; } = CommonConstant.ERROR_LEVEL_WARNING;
            
            /// <summary>
            /// Gets or sets the log level for Microsoft.AspNetCore.Mvc.Infrastructure namespace.
            /// </summary>
            public string? MicrosoftAspNetCoreMvcInfrastructure { get; set; } = CommonConstant.ERROR_LEVEL_WARNING;
            
            /// <summary>
            /// Gets or sets the log level for Microsoft.EntityFrameworkCore namespace.
            /// </summary>
            public string? MicrosoftEntityFrameworkCore { get; set; } = CommonConstant.ERROR_LEVEL_WARNING;
        }
    }
}