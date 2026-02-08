using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using OXDesk.Core.Identity;
using OXDesk.Core.Identity.DTOs;

namespace OXDesk.Identity.Validators
{
    public sealed class CreatePermissionRequestValidator : AbstractValidator<CreatePermissionRequest>
    {
        private readonly IPermissionService _permissionService;

        public CreatePermissionRequestValidator(IPermissionService permissionService)
        {
            _permissionService = permissionService;

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Permission name is required.")
                .MaximumLength(20).WithMessage("Permission name must be 20 characters or fewer.")
                .MustAsync(BeUniqueName).WithMessage("A permission with the same name already exists.");
        }

        private async Task<bool> BeUniqueName(string name, CancellationToken ct)
        {
            var normalized = name?.Trim().ToUpperInvariant() ?? string.Empty;
            var existing = await _permissionService.GetPermissionByNameAsync(normalized);
            return existing == null;
        }
    }
}
