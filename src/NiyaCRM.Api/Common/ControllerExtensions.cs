using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NiyaCRM.Core.Common;

namespace NiyaCRM.Api.Common;

/// <summary>
/// Extension methods for API controllers to standardize responses.
/// </summary>
public static class ControllerExtensions
{
    /// <summary>
    /// Creates a BadRequest response with ProblemDetails.
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <param name="message">The error message.</param>
    /// <returns>A BadRequest result with ProblemDetails.</returns>
    public static BadRequestObjectResult CreateBadRequestProblem(this ControllerBase controller, string message)
    {
        return controller.BadRequest(new ProblemDetails
        {
            Title = CommonConstant.MESSAGE_INVALID_REQUEST,
            Detail = message,
            Status = StatusCodes.Status400BadRequest
        });
    }

    /// <summary>
    /// Creates a Conflict response with ProblemDetails.
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <param name="message">The error message.</param>
    /// <returns>A Conflict result with ProblemDetails.</returns>
    public static ConflictObjectResult CreateConflictProblem(this ControllerBase controller, string message)
    {
        return controller.Conflict(new ProblemDetails
        {
            Title = CommonConstant.MESSAGE_CONFLICT,
            Detail = message,
            Status = StatusCodes.Status409Conflict
        });
    }

    /// <summary>
    /// Creates a NotFound response with ProblemDetails.
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <param name="message">The error message.</param>
    /// <returns>A NotFound result with ProblemDetails.</returns>
    public static NotFoundObjectResult CreateNotFoundProblem(this ControllerBase controller, string message)
    {
        return controller.NotFound(new ProblemDetails
        {
            Title = "Resource Not Found",
            Detail = message,
            Status = StatusCodes.Status404NotFound
        });
    }
}
