using Matgar.Application.Common.Results;
using Microsoft.AspNetCore.Identity;

namespace Matgar.Infrastructure.Identity.Services
{
    internal static class IdentityErrorMapper
    {
        public static Error Map(IdentityError error)
        {
            return error.Code switch
            {
                "DuplicateUserName"
               or "DuplicateEmail"
                   => Error.Conflict(
                       code: error.Code,
                       message: error.Description),


                "InvalidEmail"
                or "InvalidUserName"
                or "PasswordTooShort"
                or "PasswordRequiresDigit"
                or "PasswordRequiresUpper"
                or "PasswordRequiresLower"
                or "PasswordRequiresNonAlphanumeric"
                    => Error.Validation(
                        code: error.Code,
                        message: error.Description),
                "InvalidToken"
                => Error.Validation(
                code: error.Code,
                message: "Confirmation link is invalid or expired"),

                _ => Error.Failure(
                        code: error.Code,
                        message: error.Description)
            };
        }
    }
}
