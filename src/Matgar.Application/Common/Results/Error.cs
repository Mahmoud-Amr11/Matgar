namespace Matgar.Application.Common.Results
{
    public sealed record Error(string? Code, string? Message, ErrorType? errorType)
    {
        public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);
        public static Error NotFound(
              string code = "NotFound",
              string message = "Resource was not found")
              => new(code, message, ErrorType.NotFound);


        public static Error Validation(
            string code = "Validation",
            string message = "Validation error occurred")
            => new(code, message, ErrorType.Validation);


        public static Error Conflict(
            string code = "Conflict",
            string message = "Conflict error occurred")
            => new(code, message, ErrorType.Conflict);


        public static Error Unauthorized(
            string code = "Unauthorized",
            string message = "Unauthorized request")
            => new(code, message, ErrorType.Unauthorized);


        public static Error Failure(
            string code = "Failure",
            string message = "An error occurred")
            => new(code, message, ErrorType.Failure);

        public static Error Forbidden(
            string code = "Forbidden",
            string message = "You do not have permission to perform this action")
               => new Error(code, message, ErrorType.Forbidden);


    }
}
