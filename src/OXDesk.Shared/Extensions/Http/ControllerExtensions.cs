using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OXDesk.Core.Common;
using System.Security.Claims;

namespace OXDesk.Shared.Extensions.Http;

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

    /// <summary>
    /// Creates a 405 Method Not Allowed response with ProblemDetails.
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <param name="message">The error message.</param>
    /// <returns>An ObjectResult with 405 status and ProblemDetails.</returns>
    public static ObjectResult CreateMethodNotAllowedProblem(this ControllerBase controller, string message)
    {
        return controller.StatusCode(StatusCodes.Status405MethodNotAllowed, new ProblemDetails
        {
            Title = "Method Not Allowed",
            Detail = message,
            Status = StatusCodes.Status405MethodNotAllowed
        });
    }

    /// <summary>
    /// Extracts the current user's ID (int) from common JWT claims.
    /// Looks for "sub", "nameid", or ClaimTypes.NameIdentifier. Returns null if not a valid int.
    /// </summary>
    public static int? GetCurrentUserId(this ControllerBase controller)
    {
        var sub = controller.User.FindFirst("sub")?.Value
                  ?? controller.User.FindFirst("nameid")?.Value
                  ?? controller.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(sub, out var id) ? id : (int?)null;
    }
}
