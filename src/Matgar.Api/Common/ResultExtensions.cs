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

        public static ObjectResult ToProblemDetailsResult(this List<Error> errors)
        {

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
                Title = primaryError.errorType.ToString(),
                Detail = primaryError.Message,
                Type = $"https://httpstatuses.io/{statusCode}",
            };

            problemDetails.Extensions["errors"] = errors.Select(x => new
            {
                x.Code,
                x.Message
            });

            return new ObjectResult(problemDetails) { StatusCode = statusCode };
        }
    }
}
