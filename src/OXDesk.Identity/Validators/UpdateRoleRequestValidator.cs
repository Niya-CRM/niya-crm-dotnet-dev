using FluentValidation;
using OXDesk.Core.Identity.DTOs;

namespace OXDesk.Identity.Validators
{
    public sealed class UpdateRoleRequestValidator : AbstractValidator<UpdateRoleRequest>
    {
        public UpdateRoleRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Role name is required.")
                .MaximumLength(256).WithMessage("Role name must be 256 characters or fewer.");
        }
    }
}
