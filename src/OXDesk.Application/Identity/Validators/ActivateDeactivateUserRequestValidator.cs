using FluentValidation;
using OXDesk.Core.Identity.DTOs;

namespace OXDesk.Application.Identity.Validators
{
    public class ActivateDeactivateUserRequestValidator : AbstractValidator<ActivateDeactivateUserRequest>
    {
        public ActivateDeactivateUserRequestValidator()
        {
            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Reason is required.")
                .MaximumLength(255).WithMessage("Reason must be less than 255 characters.");
        }
    }
}
