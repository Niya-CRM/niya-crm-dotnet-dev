﻿using System.Reflection;

namespace NiyaCRM.Api.Configurations
{
    internal static class Startup
    {
        internal static WebApplicationBuilder AddConfigurations(this WebApplicationBuilder builder)
        {
            const string configurationsDirectory = "Configurations";
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)

                    .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true, reloadOnChange: true)

                    .AddJsonFile($"{configurationsDirectory}/logger.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"{configurationsDirectory}/logger.{env}.json", optional: true, reloadOnChange: true)

                    .AddJsonFile($"{configurationsDirectory}/postgresql.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"{configurationsDirectory}/postgresql.{env}.json", optional: true, reloadOnChange: true)

                    .AddJsonFile($"{configurationsDirectory}/redis.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"{configurationsDirectory}/redis.{env}.json", optional: true, reloadOnChange: true)

                    //.AddJsonFile($"{configurationsDirectory}/hangfire.json", optional: false, reloadOnChange: true)
                    //.AddJsonFile($"{configurationsDirectory}/hangfire.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                    //.AddJsonFile($"{configurationsDirectory}/cache.json", optional: false, reloadOnChange: true)
                    //.AddJsonFile($"{configurationsDirectory}/cache.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                    //.AddJsonFile($"{configurationsDirectory}/cors.json", optional: false, reloadOnChange: true)
                    //.AddJsonFile($"{configurationsDirectory}/cors.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                    //.AddJsonFile($"{configurationsDirectory}/database.json", optional: false, reloadOnChange: true)
                    //.AddJsonFile($"{configurationsDirectory}/database.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                    //.AddJsonFile($"{configurationsDirectory}/mail.json", optional: false, reloadOnChange: true)
                    //.AddJsonFile($"{configurationsDirectory}/mail.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                    //.AddJsonFile($"{configurationsDirectory}/middleware.json", optional: false, reloadOnChange: true)
                    //.AddJsonFile($"{configurationsDirectory}/middleware.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                    //.AddJsonFile($"{configurationsDirectory}/security.json", optional: false, reloadOnChange: true)
                    //.AddJsonFile($"{configurationsDirectory}/security.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                    //.AddJsonFile($"{configurationsDirectory}/openapi.json", optional: false, reloadOnChange: true)
                    //.AddJsonFile($"{configurationsDirectory}/openapi.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                    //.AddJsonFile($"{configurationsDirectory}/signalr.json", optional: false, reloadOnChange: true)
                    //.AddJsonFile($"{configurationsDirectory}/signalr.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                    //.AddJsonFile($"{configurationsDirectory}/securityheaders.json", optional: false, reloadOnChange: true)
                    //.AddJsonFile($"{configurationsDirectory}/securityheaders.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                    //.AddJsonFile($"{configurationsDirectory}/localization.json", optional: false, reloadOnChange: true)
                    //.AddJsonFile($"{configurationsDirectory}/localization.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables();
            return builder;
        }
    }
}
