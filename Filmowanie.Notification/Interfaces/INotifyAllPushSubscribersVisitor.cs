using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Interfaces;

namespace Filmowanie.Notification.Interfaces;

internal interface INotifyAllPushSubscribersVisitor : IOperationAsyncVisitor<(TenantId, string Message), object>;