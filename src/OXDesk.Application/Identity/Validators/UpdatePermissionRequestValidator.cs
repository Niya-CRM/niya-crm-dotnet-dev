using FluentValidation;
using OXDesk.Core.Identity.DTOs;

namespace OXDesk.Application.Identity.Validators
{
    public sealed class UpdatePermissionRequestValidator : AbstractValidator<UpdatePermissionRequest>
    {
        public UpdatePermissionRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Permission name is required.")
                .MaximumLength(20).WithMessage("Permission name must be 20 characters or fewer.");
        }
    }
}
