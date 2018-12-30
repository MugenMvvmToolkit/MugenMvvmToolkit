using System;
using MugenMvvm.Models;

namespace MugenMvvm.Interfaces.Messaging
{
    public interface IMessengerSubscriber : IEquatable<IMessengerSubscriber>//todo add memento to all handlers
    {
        SubscriberResult Handle(object sender, object message, IMessengerContext messengerContext);
    }
}