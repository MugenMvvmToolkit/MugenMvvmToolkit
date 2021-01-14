using System;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Messaging.Components;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTests.Messaging.Internal
{
    public class TestMessagePublisherComponent : IMessagePublisherComponent, IHasPriority
    {
        private readonly IMessenger? _messenger;

        public TestMessagePublisherComponent(IMessenger? messenger)
        {
            _messenger = messenger;
        }

        public Func<IMessageContext, bool>? TryPublish { get; set; }

        public int Priority { get; set; }

        bool IMessagePublisherComponent.TryPublish(IMessenger messenger, IMessageContext messageContext)
        {
            _messenger?.ShouldEqual(messenger);
            return TryPublish?.Invoke(messageContext) ?? false;
        }
    }
}