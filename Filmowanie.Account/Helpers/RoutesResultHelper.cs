using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Account.Helpers;

internal static class RoutesResultHelper
{
    public static IResult UnwrapOperationResult<T>(OperationResult<T> result, IResult? onSuccess = null)
    {
        if (result.Error == null)
            return onSuccess ?? TypedResults.Ok(result.Result);

        const string separator = ", ";

        IResult? unwrapped = result.Error!.Value.Type switch
        {
            ErrorType.IncomingDataIssue => TypedResults.BadRequest(result.Error!.Value.ErrorMessages.Concat(separator)),
            ErrorType.ValidationError => TypedResults.BadRequest(result.Error!.Value.ErrorMessages.Concat(separator)),
            ErrorType.AuthorizationIssue2 => TypedResults.Forbid(),
            ErrorType.AuthenticationIssue => TypedResults.Unauthorized(),
            ErrorType.Canceled => TypedResults.StatusCode(499),
            _ => null
        };

        if (unwrapped != null)
            return unwrapped;

        throw new InvalidOperationException($"Erroneous result! {result.Error.Value.ErrorMessages.Concat(separator)}.");
    }
}