using FluentValidation;
using OXDesk.Core.Identity.DTOs;

namespace OXDesk.Application.Identity.Validators
{
    public sealed class CreateRoleRequestValidator : AbstractValidator<CreateRoleRequest>
    {
        public CreateRoleRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Role name is required.")
                .MaximumLength(256).WithMessage("Role name must be 256 characters or fewer.");
        }
    }
}
