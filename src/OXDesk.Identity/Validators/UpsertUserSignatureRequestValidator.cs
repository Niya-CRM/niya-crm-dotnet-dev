using FluentValidation;
using OXDesk.Core.Identity.DTOs;

namespace OXDesk.Identity.Validators;

/// <summary>
/// Validator for UpsertUserSignatureRequest.
/// </summary>
public sealed class UpsertUserSignatureRequestValidator : AbstractValidator<UpsertUserSignatureRequest>
{
    public UpsertUserSignatureRequestValidator()
    {
        RuleFor(x => x.ComplimentaryClose)
            .MaximumLength(100).WithMessage("Complimentary close must be 100 characters or fewer.");

        RuleFor(x => x.FullName)
            .MaximumLength(100).WithMessage("Full name must be 100 characters or fewer.");

        RuleFor(x => x.JobTitle)
            .MaximumLength(100).WithMessage("Job title must be 100 characters or fewer.");

        RuleFor(x => x.Company)
            .MaximumLength(200).WithMessage("Company must be 200 characters or fewer.");

        RuleFor(x => x.Department)
            .MaximumLength(100).WithMessage("Department must be 100 characters or fewer.");

        RuleFor(x => x.AddressLine1)
            .MaximumLength(200).WithMessage("Address line 1 must be 200 characters or fewer.");

        RuleFor(x => x.AddressLine2)
            .MaximumLength(200).WithMessage("Address line 2 must be 200 characters or fewer.");

        RuleFor(x => x.AddressLine3)
            .MaximumLength(200).WithMessage("Address line 3 must be 200 characters or fewer.");

        RuleFor(x => x.Telephone)
            .MaximumLength(30).WithMessage("Telephone must be 30 characters or fewer.");

        RuleFor(x => x.Mobile)
            .MaximumLength(30).WithMessage("Mobile must be 30 characters or fewer.");

        RuleFor(x => x.Email)
            .MaximumLength(255).WithMessage("Email must be 255 characters or fewer.")
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("Email must be a valid email address.");

        RuleFor(x => x.Website)
            .MaximumLength(255).WithMessage("Website must be 255 characters or fewer.");

        RuleFor(x => x.FreeStyleSignature)
            .MaximumLength(30000).WithMessage("Free-style signature must be 1000 characters or fewer.");
    }
}
