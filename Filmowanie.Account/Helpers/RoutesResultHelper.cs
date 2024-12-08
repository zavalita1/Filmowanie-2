using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Account.Constants;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Account.Helpers;

internal static class RoutesResultHelper
{
    public static IResult UnwrapOperationResult<T>(OperationResult<T> result, IResult? onSuccess = null, Func<Error, IResult?>? overrideDefault = null)
    {
        if (result.Error == null)
            return onSuccess ?? TypedResults.Ok(result.Result);

        const string separator = ", ";

        IResult? unwrapped = result.Error!.Value.Type switch
        {
            ErrorType.IncomingDataIssue => TypedResults.BadRequest(result.Error!.Value.ErrorMessages.Concat(separator)),
            ErrorType.ValidationError => TypedResults.BadRequest(result.Error!.Value.ErrorMessages.Concat(separator)),
            ErrorType.AuthorizationIssue => TypedResults.Forbid(),
            ErrorType.AuthenticationIssue => TypedResults.Unauthorized(),
            ErrorType.Canceled => TypedResults.StatusCode(499),
            _ => null
        };

        if (result.Error!.Value.Type == ErrorType.AuthenticationIssue)
        {
            var message = result.Error!.Value.ErrorMessages.Contains(Messages.CookieExpired) ? "Cookie expired!" : "Please log in";
            return TypedResults.Problem(message, statusCode: 401);
        }

        var overriddenValue = overrideDefault?.Invoke(result.Error.Value);
        unwrapped = overriddenValue ?? unwrapped;

        if (unwrapped != null)
            return unwrapped;

        throw new InvalidOperationException($"Erroneous result! {result.Error.Value.ErrorMessages.Concat(separator)}.");
    }
}