using FluentValidation;
using OXDesk.Core.Tickets;
using OXDesk.Core.Tickets.DTOs;
using OXDesk.Tickets.Services;

namespace OXDesk.Tickets.Validators;

/// <summary>
/// Validator for PatchBusinessHoursRequest.
/// </summary>
public class PatchBusinessHoursRequestValidator : AbstractValidator<PatchBusinessHoursRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PatchBusinessHoursRequestValidator"/> class.
    /// </summary>
    public PatchBusinessHoursRequestValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(60).When(x => x.Name != null)
            .WithMessage("Business hours name cannot exceed 60 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(255).When(x => x.Description != null)
            .WithMessage("Description cannot exceed 255 characters.");

        RuleFor(x => x.TimeZone)
            .MinimumLength(3).When(x => x.TimeZone != null)
            .WithMessage("Time zone must be at least 3 characters.")
            .MaximumLength(100).When(x => x.TimeZone != null)
            .WithMessage("Time zone cannot exceed 100 characters.");

        RuleFor(x => x.BusinessHoursType)
            .Must(t => t == null
                    || t == BusinessHoursConstant.BusinessHoursTypes.TwentyFourSeven
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
