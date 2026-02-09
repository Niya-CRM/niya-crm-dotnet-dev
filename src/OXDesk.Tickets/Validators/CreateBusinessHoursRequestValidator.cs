using FluentValidation;
using OXDesk.Core.Tickets;
using OXDesk.Core.Tickets.DTOs;
using OXDesk.Tickets.Services;

namespace OXDesk.Tickets.Validators;

/// <summary>
/// Validator for CreateBusinessHoursRequest.
/// </summary>
public class CreateBusinessHoursRequestValidator : AbstractValidator<CreateBusinessHoursRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateBusinessHoursRequestValidator"/> class.
    /// </summary>
    public CreateBusinessHoursRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Business hours name is required.")
            .MaximumLength(60).WithMessage("Business hours name cannot exceed 60 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(255).WithMessage("Description cannot exceed 255 characters.");

        RuleFor(x => x.TimeZone)
            .MinimumLength(3).When(x => !string.IsNullOrEmpty(x.TimeZone))
            .WithMessage("Time zone must be at least 3 characters.")
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.TimeZone))
            .WithMessage("Time zone cannot exceed 100 characters.");

        RuleFor(x => x.BusinessHoursType)
            .NotEmpty().WithMessage("Business hours type is required.")
            .Must(t => t == BusinessHoursConstant.BusinessHoursTypes.TwentyFourSeven
                    || t == BusinessHoursConstant.BusinessHoursTypes.Custom)
            .WithMessage(BusinessHoursConstant.ErrorMessages.InvalidBusinessHoursType);

        RuleForEach(x => x.CustomHours)
            .ChildRules(item =>
            {
                item.RuleFor(i => i.Day)
                    .NotEmpty().WithMessage("Day is required.")
                    .MaximumLength(10).WithMessage("Day cannot exceed 10 characters.")
                    .Must(d => BusinessHoursConstant.DayOfWeekNames.All.Contains(d))
                    .WithMessage(BusinessHoursConstant.ErrorMessages.InvalidDay);

                item.RuleFor(i => i.StartTime)
                    .LessThan(i => i.EndTime).WithMessage("Start time must be before end time.");
            })
            .When(x => x.CustomHours != null && x.CustomHours.Count > 0);

        RuleFor(x => x.CustomHours)
            .Must(items => items == null || BusinessHoursService.ValidateNoTimeOverlap(items))
            .WithMessage(BusinessHoursConstant.ErrorMessages.TimeOverlap);
    }
}
