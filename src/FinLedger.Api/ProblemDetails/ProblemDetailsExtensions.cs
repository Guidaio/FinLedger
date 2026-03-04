using Microsoft.AspNetCore.Mvc;

namespace FinLedger.Api.ProblemDetails;

/// <summary>
/// Extension methods for returning RFC 7807 ProblemDetails from minimal API endpoints.
/// </summary>
public static class ProblemDetailsExtensions
{
    public static IResult BadRequest(string detail, string? title = "Bad Request", IDictionary<string, object?>? extensions = null)
    {
        var problem = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = title,
            Detail = detail
        };
        if (extensions is not null)
            foreach (var kv in extensions)
                problem.Extensions[kv.Key] = kv.Value;
        return Results.Json(problem, statusCode: problem.Status.Value, contentType: "application/problem+json");
    }

    public static IResult NotFound(string detail, string? title = "Not Found") =>
        Results.Problem(detail, statusCode: StatusCodes.Status404NotFound, title: title);

    public static IResult Conflict(string detail, string? title = "Conflict") =>
        Results.Problem(detail, statusCode: StatusCodes.Status409Conflict, title: title);

    public static IResult UnprocessableEntity(string detail, string? title = "Unprocessable Entity", IDictionary<string, object?>? extensions = null)
    {
        var problem = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = StatusCodes.Status422UnprocessableEntity,
            Title = title,
            Detail = detail
        };
        if (extensions is not null)
            foreach (var kv in extensions)
                problem.Extensions[kv.Key] = kv.Value;
        return Results.Json(problem, statusCode: problem.Status.Value, contentType: "application/problem+json");
    }

    public static IResult InternalServerError(string? requestId = null)
    {
        var problem = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Internal Server Error",
            Detail = "An unexpected error occurred."
        };
        if (!string.IsNullOrEmpty(requestId))
            problem.Extensions["requestId"] = requestId;
        return Results.Json(problem, statusCode: problem.Status.Value, contentType: "application/problem+json");
    }
}
