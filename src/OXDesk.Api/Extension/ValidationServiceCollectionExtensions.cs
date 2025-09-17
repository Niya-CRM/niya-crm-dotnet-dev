using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OXDesk.Api.Validators.Tenants;
using OXDesk.Application.Identity.Validators;
using OXDesk.Application.Tenants.Validators;
using OXDesk.Core.Identity.DTOs;
using OXDesk.Core.Tenants.DTOs;

namespace OXDesk.Api.Extension
{
    /// <summary>
    /// Service registration helpers for FluentValidation validators used by the OXDesk API.
    /// </summary>
    public static class ValidationServiceCollectionExtensions
    {
        /// <summary>
        /// Registers all FluentValidation validators for API request DTOs.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddOxDeskValidators(this IServiceCollection services)
        {
            // Tenant validators
            services.AddScoped<IValidator<CreateTenantRequest>, CreateTenantRequestValidator>();
            services.AddScoped<IValidator<ActivateDeactivateTenantRequest>, ActivateDeactivateTenantRequestValidator>();

            // Identity validators
            services.AddScoped<IValidator<CreatePermissionRequest>, CreatePermissionRequestValidator>();
            services.AddScoped<IValidator<UpdatePermissionRequest>, UpdatePermissionRequestValidator>();
            services.AddScoped<IValidator<CreateRoleRequest>, CreateRoleRequestValidator>();
            services.AddScoped<IValidator<UpdateRoleRequest>, UpdateRoleRequestValidator>();
            services.AddScoped<IValidator<ActivateDeactivateUserRequest>, ActivateDeactivateUserRequestValidator>();

            return services;
        }
    }
}
