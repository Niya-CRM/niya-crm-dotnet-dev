using FluentValidation;
using OXDesk.Core.Tickets.DTOs;

namespace OXDesk.Tickets.Validators;

/// <summary>
/// Validator for CreateHolidayRequest.
/// </summary>
public class CreateHolidayRequestValidator : AbstractValidator<CreateHolidayRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateHolidayRequestValidator"/> class.
    /// </summary>
    public CreateHolidayRequestValidator()
    {
        RuleFor(x => x.BusinessHourId)
            .GreaterThan(0).WithMessage("Business hour ID must be greater than 0.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Holiday name is required.")
            .MaximumLength(60).WithMessage("Holiday name cannot exceed 60 characters.");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Holiday date is required.");
    }
}
