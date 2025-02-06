using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Notification.DTOs.Incoming;

namespace Filmowanie.Notification.Interfaces;

internal interface ISavePushNotificationVisitor : IOperationAsyncVisitor<(PushSubscriptionDTO, DomainUser), object>;