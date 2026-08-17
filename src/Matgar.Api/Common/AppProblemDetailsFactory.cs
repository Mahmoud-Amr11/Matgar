using Microsoft.AspNetCore.Mvc;

namespace Matgar.Api.Common
{
    public static class AppProblemDetailsFactory
    {
        public static ProblemDetails Create(
            HttpContext httpContext,
            int statusCode,
            string title,
            string? detail = null,
            IDictionary<string, string[]>? errors = null)
        {
            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Type = GetProblemType(statusCode),
                Instance = httpContext.Request.Path,
                Detail = detail
            };

            problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
            problemDetails.Extensions["timestamp"] = DateTime.UtcNow;

            if (errors is { Count: > 0 })
                problemDetails.Extensions["errors"] = errors;

            return problemDetails;
        }

        public static string GetProblemType(int statusCode) => statusCode switch
        {
            400 => "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            401 => "https://tools.ietf.org/html/rfc9110#section-15.5.2",
            403 => "https://tools.ietf.org/html/rfc9110#section-15.5.4",
            404 => "https://tools.ietf.org/html/rfc9110#section-15.5.5",
            409 => "https://tools.ietf.org/html/rfc9110#section-15.5.10",
            _ => "https://tools.ietf.org/html/rfc9110#section-15.6.1"
        };
    }
}
