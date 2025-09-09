using Filmowanie.Abstractions.Maybe;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Notification.Interfaces;

internal interface IRoutesResultHelper
{
    IResult UnwrapOperationResult<T>(Maybe<T> result);
}