using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Notification.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Notification.Services;

internal sealed class RoutesResultHelper : IRoutesResultHelper
{
    public IResult UnwrapOperationResult<T>(OperationResult<T> result)
    {
        if (result.Error == null)
            return TypedResults.Ok(result.Result);

        const string separator = ", ";

        IResult? unwrapped = result.Error!.Value.Type switch
        {
            ErrorType.IncomingDataIssue => TypedResults.BadRequest(string.Join(separator, result.Error!.Value.ErrorMessages)),
            ErrorType.ValidationError => TypedResults.BadRequest(string.Join(separator, result.Error!.Value.ErrorMessages)),
            ErrorType.AuthorizationIssue => TypedResults.Forbid(),
            ErrorType.AuthenticationIssue => TypedResults.Unauthorized(),
            ErrorType.Canceled => TypedResults.StatusCode(499),
            _ => null
        };

        if (unwrapped != null)
            return unwrapped;

        throw new InvalidOperationException($"Erroneous result! {string.Join(separator, result.Error.Value.ErrorMessages)}.");
    }
}