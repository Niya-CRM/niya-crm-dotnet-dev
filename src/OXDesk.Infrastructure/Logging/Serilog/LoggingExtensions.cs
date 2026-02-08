using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Compliance.Redaction;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OXDesk.Core.Common;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Enrichers.Sensitive;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Json;
using Serilog.Redaction;
using System;
using System.Diagnostics;

namespace OXDesk.Infrastructure.Logging.Serilog
{
    public static class LoggingExtensions
    {
        // Extension method to register Serilog
        public static void RegisterSerilog(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((context, services) =>
            {
                services.AddOptions<LoggerSettings>().BindConfiguration("LoggerSettings");

                services.AddHttpLogging(logging =>
                {
                    // Log headers, properties, and bodies
                    logging.LoggingFields = HttpLoggingFields.All;
                    logging.RequestBodyLogLimit = 1024 * 1024; // 1 MB
                    logging.ResponseBodyLogLimit = 1024 * 1024; // 1 MB
                    logging.CombineLogs = true;

                    // Whitelist safe request headers
                    logging.RequestHeaders.Add("X-Forwarded-For");
                    logging.RequestHeaders.Add("X-Forwarded-Proto");
                    logging.RequestHeaders.Add("X-Forwarded-Port");
                    logging.RequestHeaders.Add("X-Forwarded-Host");
                    logging.RequestHeaders.Add("X-Forwarded-Server");
                    logging.RequestHeaders.Add("X-Amzn-Trace-Id");
                    logging.RequestHeaders.Add("Upgrade-Insecure-Requests");
                    logging.RequestHeaders.Add("sec-ch-ua");
                    logging.RequestHeaders.Add("sec-ch-ua-mobile");
                    logging.RequestHeaders.Add("Authorization");

                    // Whitelist safe response headers
                    logging.ResponseHeaders.Add("Pragma");
                    logging.ResponseHeaders.Add("Cache-Control");
                    logging.ResponseHeaders.Add("max-age");
                    logging.ResponseHeaders.Add("X-RequestBody-Log");

                    // Explicitly ensure sensitive headers are not logged
                    //logging.RequestHeaders.Remove("Authorization");
                    logging.RequestHeaders.Remove("Cookie");
                    logging.ResponseHeaders.Remove("Set-Cookie");
                });
            });

            _ = hostBuilder.UseSerilog((context, sp, configuration) =>
            {
                var loggerSettings = sp.GetRequiredService<IOptions<LoggerSettings>>().Value;

                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
                string applicationName = context.Configuration["ApplicationName"]?.Replace(".", "-").Replace(" ", "-") ?? "MyDotNetApplication";

                string outputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u4}] [{Environment}] [{Application}] [{MachineName}] [{ClientIp}] [{TraceParent}] [{SourceContext}] [{Domain}] [{RequestPath}] [{ThreadId}] [{Message} {Exception}]{NewLine}";

                var minLogLevel = context.Configuration["LoggerSettings:LogLevel:Default"] ?? "Information";
                SetMinimumLogLevel(configuration, minLogLevel);

                OverideMinimumLogLevel(configuration, loggerSettings);

                configuration.Destructure.WithRedaction(sp.GetRequiredService<IRedactorProvider>());

                // Filter out any event logging request bodies for auth paths as defense-in-depth
                /* configuration.Filter.ByExcluding(logEvent =>
                {
                    // Match message-based logs that include the token 'RequestBody'
                    var template = logEvent.MessageTemplate?.Text ?? string.Empty;
                    if (!template.Contains("password", StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }

                    // Try to read RequestPath property (added by UseSerilogRequestLogging)
                    if (logEvent.Properties.TryGetValue("RequestPath", out var requestPathProp))
                    {
                        var scalar = requestPathProp as global::Serilog.Events.ScalarValue;
                        var requestPath = scalar?.Value as string ?? string.Empty;
                        if (requestPath.StartsWith("/api/auth", StringComparison.OrdinalIgnoreCase) ||
                            requestPath.StartsWith("/auth", StringComparison.OrdinalIgnoreCase))
                        {
                            return true; // exclude
                        }
                    }
                    return false;
                }); */

                ConfigureEnrichers(configuration, applicationName, environment);

                bool CompactConsoleLogging = loggerSettings.CompactConsoleLogging;
                ConfigureConsoleLogging(configuration, CompactConsoleLogging, outputTemplate);

                bool writeToFile = loggerSettings.WriteToFile;
                string logFilePath = loggerSettings.LogFilePath;
                ConfigureWriteToFile(configuration, writeToFile, logFilePath, outputTemplate, applicationName, environment);

                string? elasticSearchUrl = loggerSettings.ElasticSearchUrl; // TO DO
                string? s3Bucket = loggerSettings.S3Bucket; // TO DO

            });
        }

        private static void SetMinimumLogLevel(LoggerConfiguration configuration, string minLogLevel)
        {
            switch (minLogLevel.ToLower())
            {
                case "debug":
                    configuration.MinimumLevel.Debug();
                    break;
                case "information":
                    configuration.MinimumLevel.Information();
                    break;
                case "warning":
                    configuration.MinimumLevel.Warning();
                    break;
                default:
                    configuration.MinimumLevel.Information();
                    break;
            }
        }

        private static void ConfigureEnrichers(LoggerConfiguration configuration, string applicationName, string environment)
        {
            configuration
                .Enrich.FromLogContext()
                .Enrich.WithThreadId()
                .Enrich.WithThreadName()
                .Enrich.WithClientIp()
                .Enrich.With<TraceIdEnricher>()
                .Enrich.WithMachineName()
                .Enrich.WithProperty("Application", applicationName)
                .Enrich.WithProperty("Environment", environment)
                .Enrich.WithSensitiveDataMasking(
                    options =>
                    {
                        options.MaskingOperators = new List<IMaskingOperator>
                        {
                            new EmailAddressMaskingOperator(),
                            new CreditCardMaskingOperator(),
                            new PasswordValueMaskingOperator(),
                            new TokenMaskingOperator(),
                            new RefreshTokenMaskingOperator()
                        };
                        options.MaskProperties.Add(
                            new MaskProperty 
                            {
                                Name = "*email",
                                Options = new MaskOptions 
                                {
                                    WildcardMatch = true
                                }
                            }
                       );
                        options.MaskProperties.Add(
                            new MaskProperty 
                            {
                                Name = "*password",
                                Options = new MaskOptions 
                                {
                                    WildcardMatch = true
                                }
                            }
                        );
                        options.MaskProperties.Add(
                            new MaskProperty 
                            {
                                Name = "*token",
                                Options = new MaskOptions 
                                {
                                    WildcardMatch = true
                                }
                            }
                        );
                    }
                );
        }

        private static void ConfigureConsoleLogging(LoggerConfiguration configuration, bool CompactConsoleLogging, string outputTemplate)
        {
            if (CompactConsoleLogging)
            {
                configuration.WriteTo.Console(new CompactJsonFormatter());
            }
            else
            {
                configuration.WriteTo.Console(outputTemplate: outputTemplate);
            }
        }

        private static void ConfigureWriteToFile(LoggerConfiguration configuration, bool writeToFile, string logFilePath, string outputTemplate, string applicationName, string environment)
        {
            if (writeToFile)
            {
                string logFile = $"{logFilePath}log_{Environment.MachineName}_{applicationName}_{environment.ToLower()}_.log";

                configuration.WriteTo.File(
                    path: logFile,
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true,
                    outputTemplate: outputTemplate
                );
            }
        }

        private static void OverideMinimumLogLevel(LoggerConfiguration configuration, LoggerSettings loggerSettings)
        {
            configuration
                     .MinimumLevel.Override("Microsoft", GetLoggingLevel(loggerSettings.LogLevel.Microsoft ?? CommonConstant.ERROR_LEVEL_WARNING))
                     .MinimumLevel.Override("Microsoft.AspNetCore", GetLoggingLevel(loggerSettings.LogLevel.MicrosoftAspNetCore ?? CommonConstant.ERROR_LEVEL_WARNING))
                     .MinimumLevel.Override("Microsoft.Hosting.Lifetime", GetLoggingLevel(loggerSettings.LogLevel.MicrosoftHostingLifetime ?? CommonConstant.ERROR_LEVEL_INFORMATION))
                     .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", GetLoggingLevel(loggerSettings.LogLevel.MicrosoftAspNetCoreAuthentication ?? CommonConstant.ERROR_LEVEL_WARNING))
                     .MinimumLevel.Override("Microsoft.AspNetCore.Mvc.Infrastructure", GetLoggingLevel(loggerSettings.LogLevel.MicrosoftAspNetCoreMvcInfrastructure ?? CommonConstant.ERROR_LEVEL_WARNING))
                     .MinimumLevel.Override("Microsoft.EntityFrameworkCore", GetLoggingLevel(loggerSettings.LogLevel.MicrosoftEntityFrameworkCore ?? CommonConstant.ERROR_LEVEL_WARNING))
                     .MinimumLevel.Override("Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware", GetLoggingLevel(CommonConstant.ERROR_LEVEL_INFORMATION))
                     // Suppress request body logging coming from our middleware below Warning level
                     .MinimumLevel.Override("OXDesk.Api.Middleware.RequestBodyRedactionMiddleware", GetLoggingLevel(CommonConstant.ERROR_LEVEL_WARNING));
        }

        private static LoggingLevelSwitch GetLoggingLevel(string logLevel)
        {
            return logLevel.ToLower() switch
            {
                "debug" => new LoggingLevelSwitch(LogEventLevel.Debug),
                "information" => new LoggingLevelSwitch(LogEventLevel.Information),
                "warning" => new LoggingLevelSwitch(LogEventLevel.Warning),
                "error" => new LoggingLevelSwitch(LogEventLevel.Error),
                "fatal" => new LoggingLevelSwitch(LogEventLevel.Fatal),
                _ => new LoggingLevelSwitch(LogEventLevel.Information)
            };
        }
    }


    /// <summary>
    /// Enriches logs with the current activity TraceId.
    /// </summary>
    public class TraceIdEnricher : ILogEventEnricher
    {
        /// <summary>
        /// Adds the TraceId property to the log event when available.
        /// </summary>
        /// <param name="logEvent">The log event to enrich.</param>
        /// <param name="propertyFactory">Factory to create log event properties.</param>
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            string? traceId = Activity.Current?.Id;
            if (string.IsNullOrWhiteSpace(traceId))
            {
                return;
            }

            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TraceParent", traceId));
        }
    }

    public class PasswordValueMaskingOperator : RegexMaskingOperator
    {
        // Match the value between quotes for a JSON password property
        // Examples matched: "password":"value", "Password" :  "value"
        private const string Pattern = "(?i)(?<=\\\"password\\\"\\s*:\\s*\\\")[^\\\"]+(?=\\\")";

        public PasswordValueMaskingOperator() : base(Pattern)
        {
        }

        protected override string PreprocessInput(string input)
        {
            return input;
        }

        protected override bool ShouldMaskInput(string input)
        {
            return input.Contains("password", StringComparison.OrdinalIgnoreCase);
        }
    }

    public class RefreshTokenMaskingOperator : RegexMaskingOperator
    {
        // Combined alternation to support both camelCase and snake_case keys
        private const string Pattern = "(?i)(?<=\\\"refreshToken\\\"\\s*:\\s*\\\")[^\\\"]+(?=\\\")|(?<=\\\"refresh_token\\\"\\s*:\\s*\\\")[^\\\"]+(?=\\\")";

        public RefreshTokenMaskingOperator() : base(Pattern)
        {
        }

        protected override string PreprocessInput(string input)
        {
            return input;
        }

        protected override bool ShouldMaskInput(string input)
        {
            return input.Contains("refreshToken", StringComparison.OrdinalIgnoreCase) ||
                   input.Contains("refresh_token", StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Masks only the value part of JSON property named "token" in free-form text log messages.
    /// </summary>
    public class TokenMaskingOperator : RegexMaskingOperator
    {
        private const string Pattern = "(?i)(?<=\\\"token\\\"\\s*:\\s*\\\")[^\\\"]+(?=\\\")";

        public TokenMaskingOperator() : base(Pattern)
        {
        }

        protected override string PreprocessInput(string input)
        {
            return input;
        }

        protected override bool ShouldMaskInput(string input)
        {
            return input.Contains("token", StringComparison.OrdinalIgnoreCase);
        }
    }
}
