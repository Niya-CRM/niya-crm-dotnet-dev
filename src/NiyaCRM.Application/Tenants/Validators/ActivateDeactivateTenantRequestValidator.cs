using FluentValidation;
using NiyaCRM.Core.Tenants.DTOs;

namespace NiyaCRM.Application.Tenants.Validators
{
    public class ActivateDeactivateTenantRequestValidator : AbstractValidator<ActivateDeactivateTenantRequest>
    {
        public ActivateDeactivateTenantRequestValidator()
        {
            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Reason is required.")
                .MaximumLength(255).WithMessage("Reason must be less than 255 characters.");
        }
    }
}
