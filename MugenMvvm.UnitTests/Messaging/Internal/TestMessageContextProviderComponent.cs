using System;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Messaging.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTests.Messaging.Internal
{
    public class TestMessageContextProviderComponent : IMessageContextProviderComponent, IHasPriority
    {
        private readonly IMessenger? _messenger;

        public TestMessageContextProviderComponent(IMessenger? messenger)
        {
            _messenger = messenger;
        }

        public Func<object?, object, IReadOnlyMetadataContext?, IMessageContext?>? TryGetMessageContext { get; set; }

        public int Priority { get; set; }

        IMessageContext? IMessageContextProviderComponent.TryGetMessageContext(IMessenger messenger, object? sender, object message, IReadOnlyMetadataContext? metadata)
        {
            _messenger?.ShouldEqual(messenger);
            return TryGetMessageContext?.Invoke(sender, message, metadata);
        }
    }
}