using System;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Messaging.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Messaging
{
    public class TestMessagePublisherComponent : IMessagePublisherComponent, IHasPriority
    {
        public Func<IMessenger, IMessageContext, bool>? TryPublish { get; set; }

        public int Priority { get; set; }

        bool IMessagePublisherComponent.TryPublish(IMessenger messenger, IMessageContext messageContext) => TryPublish?.Invoke(messenger, messageContext) ?? false;
    }
}