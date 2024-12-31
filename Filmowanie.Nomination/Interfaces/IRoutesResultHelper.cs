using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.OperationResult;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Nomination.Interfaces;

internal interface IRoutesResultHelper
{
    IResult UnwrapOperationResult<T>(OperationResult<T> result, IResult? onSuccess = null, Func<ErrorType, IResult?>? overrideDefault = null);
}