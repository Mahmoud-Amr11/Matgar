using Matgar.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace Matgar.Api.Common
{

    public static class ResultExtensions
    {
        public static IActionResult ToActionResult(this Result result, ControllerBase controller)
            => result.IsSuccess
                ? controller.Ok()
                : CreateProblemResult(result.Errors, controller.HttpContext);

        public static IActionResult ToActionResult<TValue>(this Result<TValue> result, ControllerBase controller)
            => result.IsSuccess
                ? controller.Ok(result.Value)
                : CreateProblemResult(result.Errors, controller.HttpContext);

        private static IActionResult CreateProblemResult(List<Error> errors, HttpContext httpContext)
        {
            var errorType = errors[0].errorType ?? ErrorType.Failure;
            var statusCode = MapStatusCode(errorType);

            var groupedErrors = errors
                .GroupBy(e => e.Code ?? "Error")
                .ToDictionary(g => g.Key, g => g.Select(e => e.Message ?? string.Empty).ToArray());

            var problemDetails = AppProblemDetailsFactory.Create(
                httpContext,
                statusCode,
                title: MapTitle(errorType),
                errors: groupedErrors);

            return new ObjectResult(problemDetails) { StatusCode = statusCode };
        }

        private static int MapStatusCode(ErrorType type) => type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.Failure => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        private static string MapTitle(ErrorType type) => type switch
        {
            ErrorType.Validation => "One or more validation errors occurred.",
            ErrorType.NotFound => "Resource not found.",
            ErrorType.Conflict => "Conflict occurred.",
            ErrorType.Unauthorized => "Unauthorized.",
            ErrorType.Forbidden => "Forbidden.",
            _ => "An error occurred."
        };
    }
}
