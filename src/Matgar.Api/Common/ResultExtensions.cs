using Matgar.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace Matgar.Api.Common
{
    public static class ResultExtensions
    {
        public static IActionResult ToActionResult<T>(this Result<T> result)
        {
            return result.IsSuccess
                ? new OkObjectResult(result.Value)
                : result.Errors.ToProblemDetailsResult();
        }

        public static IActionResult ToActionResult(this Result result)
        {
            return result.IsSuccess
                ? new NoContentResult()
                : result.Errors.ToProblemDetailsResult();
        }


        public static ObjectResult ToProblemDetailsResult(this IEnumerable<Error> errors)
        {
            if (errors == null || !errors.Any())
            {
                return new ObjectResult(new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Unknown Error",
                    Detail = "An unexpected error occurred without specific details.",
                    Type = $"https://httpstatuses.io/{StatusCodes.Status500InternalServerError}"
                })
                { StatusCode = StatusCodes.Status500InternalServerError };
            }

            var primaryError = errors.First();

            var statusCode = primaryError.errorType switch
            {
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status500InternalServerError
            };

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = BuildTitle(primaryError),


                Detail = primaryError.errorType == ErrorType.Validation
                    ? "Please refer to the errors property for additional validation details."
                    : primaryError.Message,

                Type = $"https://httpstatuses.io/{statusCode}",
            };

            problemDetails.Extensions["errors"] = errors.Select(x => new
            {
                x.Code,
                x.Message
            });

            return new ObjectResult(problemDetails) { StatusCode = statusCode };
        }

        private static string BuildTitle(Error error)
        => error.errorType switch
        {
            ErrorType.Validation =>
                "One or more validation errors occurred.",

            ErrorType.NotFound =>
                "The requested resource was not found.",

            ErrorType.Unauthorized =>
                "You are not authorized to perform this action.",

            ErrorType.Forbidden =>
                "You do not have permission to perform this action.",

            ErrorType.Conflict =>
                "A conflict occurred with the current state of the resource.",

            _ => "An error occurred."
        };
    }
}