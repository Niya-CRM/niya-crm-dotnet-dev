using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Json;
using System;

namespace NiyaCRM.Infrastructure.Logging.Serilog
{`
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
                    logging.LoggingFields = HttpLoggingFields.All;
                    logging.RequestHeaders.Add("x-correlation-id");
                    logging.RequestHeaders.Add("X-Forwarded-For");
                    logging.RequestHeaders.Add("X-Forwarded-Proto");
                    logging.RequestHeaders.Add("X-Forwarded-Port");
                    logging.RequestHeaders.Add("X-Forwarded-Host");
                    logging.RequestHeaders.Add("X-Forwarded-Server");
                    logging.RequestHeaders.Add("X-Amzn-Trace-Id");
                    logging.RequestHeaders.Add("Upgrade-Insecure-Requests");
                    logging.RequestHeaders.Add("sec-ch-ua");
                    logging.RequestHeaders.Add("sec-ch-ua-mobile");

                    logging.ResponseHeaders.Add("x-correlation-id");
                    logging.ResponseHeaders.Add("Pragma");
                    logging.ResponseHeaders.Add("Cache-Control");
                    logging.ResponseHeaders.Add("max-age");
                });
            });

            _ = hostBuilder.UseSerilog((context, sp, configuration) =>
            {
                var loggerSettings = sp.GetRequiredService<IOptions<LoggerSettings>>().Value;

                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
                string applicationName = context.Configuration["ApplicationName"]?.Replace(".", "-").Replace(" ", "-") ?? "MyDotNetApplication";

                string outputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u4}] [{Environment}] [{Application}] [{MachineName}] [{ClientIp}] [{CorrelationId}] [{SourceContext}] [{Host}] [{RequestPath}] [{ThreadId}] [{Message} {Exception}]{NewLine}";

                var minLogLevel = context.Configuration["LoggerSettings:LogLevel:Default"] ?? "Information";
                SetMinimumLogLevel(configuration, minLogLevel);

                OverideMinimumLogLevel(configuration, loggerSettings);

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
                .Enrich.WithCorrelationId(headerName: "X-Correlation-Id", addValueIfHeaderAbsence: true)
                .Enrich.WithMachineName()
                .Enrich.WithProperty("Application", applicationName)
                .Enrich.WithProperty("Environment", environment);
        }

        private static void ConfigureConsoleLogging(LoggerConfiguration configuration, bool CompactConsoleLogging, string outputTemplate)
        {
            if (CompactConsoleLogging)
            {
                // Output template cannot be customized with compact JSON formatter
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
                     .MinimumLevel.Override("Microsoft", GetLoggingLevel(loggerSettings.LogLevel.Microsoft ?? "Warning"))
                     .MinimumLevel.Override("Microsoft.AspNetCore", GetLoggingLevel(loggerSettings.LogLevel.MicrosoftAspNetCore ?? "Warning"))
                     .MinimumLevel.Override("\"Microsoft.Hosting.Lifetime", GetLoggingLevel(loggerSettings.LogLevel.MicrosoftHostingLifetime ?? "Information"))
                     .MinimumLevel.Override("\"\"Microsoft.AspNetCore.Authentication", GetLoggingLevel(loggerSettings.LogLevel.MicrosoftAspNetCoreAuthentication ?? "Warning"))
                     .MinimumLevel.Override("\"Microsoft.AspNetCore.Mvc.Infrastructure", GetLoggingLevel(loggerSettings.LogLevel.MicrosoftAspNetCoreMvcInfrastructure ?? "Warning"))
                     .MinimumLevel.Override("Microsoft.EntityFrameworkCore", GetLoggingLevel(loggerSettings.LogLevel.MicrosoftEntityFrameworkCore ?? "Warning"));
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
}
