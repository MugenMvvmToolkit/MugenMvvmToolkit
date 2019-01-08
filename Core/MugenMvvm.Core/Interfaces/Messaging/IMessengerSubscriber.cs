using System;
using MugenMvvm.Enums;

namespace MugenMvvm.Interfaces.Messaging
{
    public interface IMessengerSubscriber : IEquatable<IMessengerSubscriber>//todo add memento to all handlers
    {
        SubscriberResult Handle(object sender, object message, IMessengerContext messengerContext);
    }
}