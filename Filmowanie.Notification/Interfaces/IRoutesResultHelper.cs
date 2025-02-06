using Filmowanie.Abstractions.OperationResult;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Notification.Interfaces;

internal interface IRoutesResultHelper
{
    IResult UnwrapOperationResult<T>(OperationResult<T> result);
}