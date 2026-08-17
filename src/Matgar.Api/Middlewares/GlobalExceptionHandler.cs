using Matgar.Api.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Net;
using System.Security;
using System.Text.Json;

namespace Matgar.Api.Middlewares
{
    public class GlobalExceptionHandler(IProblemDetailsService _problemDetailsService, ILogger<GlobalExceptionHandler> _logger) : IExceptionHandler
    {

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "An unhandled exception occurred while processing the request.");

            var (statusCode, tittle) = MapException(exception);


            httpContext.Response.StatusCode = statusCode;

            var problemDetails = AppProblemDetailsFactory.Create(
                 httpContext, statusCode, tittle, GetSaferErrorMessage(exception, httpContext));

            problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
            problemDetails.Extensions["timestamp"] = DateTime.UtcNow;


            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problemDetails,
            });
        }

        private (int statusCode, string tittle) MapException(Exception exception)
           => exception switch
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

        private static string? GetSaferErrorMessage(Exception exception, HttpContext context)
        {
            var environment = context.RequestServices.GetRequiredService<IHostEnvironment>();

            return environment.IsDevelopment() ? exception.Message : null;
        }
    }
}
