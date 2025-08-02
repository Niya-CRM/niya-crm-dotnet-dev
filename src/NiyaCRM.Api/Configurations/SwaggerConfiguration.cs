using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace NiyaCRM.Api.Configurations
{
    /// <summary>
    /// Extension methods for configuring Swagger/OpenAPI
    /// </summary>
    public static class SwaggerConfiguration
    {
        // Static readonly fields for arrays that are used repeatedly
        private static readonly string[] _emptyStringArray = Array.Empty<string>();
        private static readonly string[] _authTagName = new[] { "Auth" };
        private static readonly string[] _otherTagName = new[] { "Other" };
        
        /// <summary>
        /// Adds and configures Swagger services to the service collection
        /// </summary>
        public static IServiceCollection AddSwaggerServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Niya CRM API",
                    Version = "v1",
                    Description = "API for Niya CRM system",
                    Contact = new OpenApiContact
                    {
                        Name = "Niya CRM Team",
                        Email = "support@niyacrm.com"
                    }
                });
                
                // Add JWT Authentication to Swagger UI
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        _emptyStringArray
                    }
                });
                
                // Filter out controllers that shouldn't appear in API documentation
                options.DocInclusionPredicate((docName, apiDesc) =>
                {
                    if (apiDesc.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
                    {
                        // Exclude AuthController and SetupController from Swagger documentation
                        if (controllerActionDescriptor.ControllerName == "Auth" || 
                            controllerActionDescriptor.ControllerName == "ApplicationSetup")
                        {
                            return false;
                        }
                    }
                    return true;
                });
                
                // Custom controller naming for Swagger tags
                options.TagActionsBy(api =>
                {
                    if (api.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
                    {
                        // Rename ApiAuth controller to Auth in Swagger UI
                        if (controllerActionDescriptor.ControllerName == "ApiAuth")
                        {
                            return _authTagName;
                        }
                        
                        // Create a controller-specific tag array
                        return new[] { controllerActionDescriptor.ControllerName };
                    }
                    
                    return _otherTagName;
                });
                
                // Order actions by tag name (with Auth first, then AuditLog) and then alphabetically by path
                options.OrderActionsBy(apiDesc => {
                    var tag = apiDesc.GroupName ?? apiDesc.ActionDescriptor.RouteValues["controller"];
                    
                    return $"{tag}_{apiDesc.RelativePath}";
                });
                
                // Include XML comments if available
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
            });
            
            return services;
        }

        /// <summary>
        /// Configures the Swagger middleware
        /// </summary>
        public static IApplicationBuilder UseSwaggerMiddleware(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (!env.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Niya CRM API v1");
                    options.RoutePrefix = "swagger";
                    options.DocumentTitle = "Niya CRM API Documentation";
                    options.DefaultModelsExpandDepth(0); // Hide schemas section by default
                });
            }
            
            return app;
        }
    }
}
