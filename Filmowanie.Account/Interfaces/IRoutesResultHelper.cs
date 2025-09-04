using Filmowanie.Abstractions.OperationResult;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Account.Interfaces;

internal interface IRoutesResultHelper
{
    IResult UnwrapOperationResult<T>(Maybe<T> result, IResult? onSuccess = null, Func<Error, IResult?>? overrideDefault = null);
}