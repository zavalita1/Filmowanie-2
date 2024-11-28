using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Voting.Helpers;

internal static class RoutesResultHelper
{
    public static IResult UnwrapOperationResult<T>(OperationResult<T> result, IResult? onSuccess = null, Func<ErrorType, IResult?>? overrideDefault = null)
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

        var overriddenValue = overrideDefault?.Invoke(result.Error!.Value.Type);
        unwrapped = overriddenValue ?? unwrapped;

        if (unwrapped != null)
            return unwrapped;

        throw new InvalidOperationException($"Erroneous result! {result.Error.Value.ErrorMessages.Concat(separator)}.");
    }
}