using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Documents.Api.ErrorMapping;

public static class ErrorMappingExtensions
{
    public static IActionResult ToProblem(this List<Error> errors)
    {
        if (errors.Count == 0)
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);

        if (errors.All(error => error.Type == ErrorType.Validation))
            return ValidationProblem(errors);

        var first = errors[0];
        var statusCode = MapStatusCode(first.Type);

        return new ObjectResult(new ProblemDetails
        {
            Status = statusCode,
            Title = ReasonPhrase(statusCode),
            Detail = statusCode >= StatusCodes.Status500InternalServerError
                ? "Internal server error occurred."
                : first.Description
        })
        {
            StatusCode = statusCode
        };
    }

    private static IActionResult ValidationProblem(List<Error> errors)
    {
        var failures = errors
            .GroupBy(error => error.Code)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.Description).ToArray());

        return new ObjectResult(new ValidationProblemDetails(failures)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Bad Request"
        })
        {
            StatusCode = StatusCodes.Status400BadRequest
        };
    }

    private static int MapStatusCode(ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError
    };

    private static string ReasonPhrase(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "Bad Request",
        StatusCodes.Status401Unauthorized => "Unauthorized",
        StatusCodes.Status403Forbidden => "Forbidden",
        StatusCodes.Status404NotFound => "Not Found",
        StatusCodes.Status409Conflict => "Conflict",
        _ => "Internal Server Error"
    };
}
