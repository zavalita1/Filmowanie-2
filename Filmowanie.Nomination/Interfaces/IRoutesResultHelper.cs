using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Maybe;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Nomination.Interfaces;

internal interface IRoutesResultHelper
{
    IResult UnwrapOperationResult<T>(Maybe<T> result, IResult? onSuccess = null, Func<ErrorType, IResult?>? overrideDefault = null);
}