using System;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Messaging.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Messaging
{
    public class TestMessageContextProviderComponent : IMessageContextProviderComponent, IHasPriority
    {
        public Func<IMessenger, object?, object, IReadOnlyMetadataContext?, IMessageContext?>? TryGetMessageContext { get; set; }

        public int Priority { get; set; }

        IMessageContext? IMessageContextProviderComponent.TryGetMessageContext(IMessenger messenger, object? sender, object message, IReadOnlyMetadataContext? metadata) =>
            TryGetMessageContext?.Invoke(messenger, sender, message, metadata);
    }
}