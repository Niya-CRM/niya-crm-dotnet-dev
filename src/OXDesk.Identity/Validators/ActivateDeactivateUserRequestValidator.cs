using FluentValidation;
using OXDesk.Core.Identity.DTOs;

namespace OXDesk.Identity.Validators;

/// <summary>
/// Validator for activate/deactivate user requests.
/// </summary>
public class ActivateDeactivateUserRequestValidator : AbstractValidator<ActivateDeactivateUserRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ActivateDeactivateUserRequestValidator"/> class.
    /// </summary>
    public ActivateDeactivateUserRequestValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required.")
            .MaximumLength(255).WithMessage("Reason must be less than 255 characters.");
    }
}
