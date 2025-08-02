using FluentValidation;
using NiyaCRM.Core.Tenants.DTOs;

namespace NiyaCRM.Api.Validators.Tenants;

/// <summary>
/// Validator for CreateTenantRequest
/// </summary>
public class CreateTenantRequestValidator : AbstractValidator<CreateTenantRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateTenantRequestValidator"/> class.
    /// </summary>
    public CreateTenantRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tenant name is required.")
            .MaximumLength(100).WithMessage("Tenant name cannot exceed 100 characters.");

        RuleFor(x => x.Host)
            .NotEmpty().WithMessage("Tenant host is required.")
            .MaximumLength(100).WithMessage("Tenant host cannot exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Tenant email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.TimeZone)
            .MinimumLength(3).When(x => !string.IsNullOrEmpty(x.TimeZone))
            .WithMessage("Time zone must be at least 3 characters.")
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.TimeZone))
            .WithMessage("Time zone cannot exceed 100 characters.");

        RuleFor(x => x.DatabaseName)
            .NotEmpty().WithMessage("Database name is required.")
            .MaximumLength(100).WithMessage("Database name cannot exceed 100 characters.");
    }
}
