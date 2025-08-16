using FluentValidation;
using OXDesk.Core.Tenants.DTOs;

namespace OXDesk.Application.Tenants.Validators
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
