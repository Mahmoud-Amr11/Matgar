using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Net;
using System.Security;
using System.Text.Json;

namespace Matgar.Api.Middlewares
{
    public class GlobalExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            logger.LogError(exception, "An unhandled exception occurred. TraceId: {TraceId}", httpContext.TraceIdentifier);

            var (statusCode, title) = MapException(exception);


            httpContext.Response.StatusCode = statusCode;
            var problemDetails = new ProblemDetails
            {
                Title = title,
                Status = statusCode,
                Type = GetProblemType(statusCode),
                Instance = httpContext.Request.Path,
                Detail = GetSaferErrorMessage(exception, httpContext)
            };
            problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
            problemDetails.Extensions["timestamp"] = DateTime.UtcNow;

            return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problemDetails
            });
        }
        private static (int StatusCode, string Title) MapException(Exception exception) => exception switch
        {
            DbUpdateException => ((int)HttpStatusCode.Conflict, "DATABASE_CONFLICT"),
            DbException => ((int)HttpStatusCode.InternalServerError, "DATABASE_ERROR"),
            HttpRequestException => ((int)HttpStatusCode.BadGateway, "EXTERNAL_SERVICE_ERROR"),
            TaskCanceledException => ((int)HttpStatusCode.RequestTimeout, "REQUEST_TIMEOUT"),
            TimeoutException => ((int)HttpStatusCode.RequestTimeout, "TIMEOUT"),
            JsonException => ((int)HttpStatusCode.BadRequest, "INVALID_JSON"),
            FileNotFoundException => ((int)HttpStatusCode.NotFound, "FILE_NOT_FOUND"),
            IOException => ((int)HttpStatusCode.InternalServerError, "IO_ERROR"),
            UnauthorizedAccessException => ((int)HttpStatusCode.Forbidden, "ACCESS_DENIED"),
            SecurityException => ((int)HttpStatusCode.Forbidden, "SECURITY_ERROR"),
            _ => ((int)HttpStatusCode.InternalServerError, "INTERNAL_ERROR")
        };
        private static string GetProblemType(int statusCode) => statusCode switch
        {
            400 => "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            401 => "https://tools.ietf.org/html/rfc9110#section-15.5.2",
            403 => "https://tools.ietf.org/html/rfc9110#section-15.5.4",
            404 => "https://tools.ietf.org/html/rfc9110#section-15.5.5",
            409 => "https://tools.ietf.org/html/rfc9110#section-15.5.10",
            _ => "https://tools.ietf.org/html/rfc9110#section-15.6.1"
        };

        private static string? GetSaferErrorMessage(Exception exception, HttpContext context)
        {
            var environment = context.RequestServices.GetRequiredService<IHostEnvironment>();

            return environment.IsDevelopment() ? exception.Message : null;
        }
    }
}

