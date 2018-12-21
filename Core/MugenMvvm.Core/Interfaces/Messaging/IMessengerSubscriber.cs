using System;
using MugenMvvm.Models;

namespace MugenMvvm.Interfaces.Messaging
{
    public interface IMessengerSubscriber : IEquatable<IMessengerSubscriber>
    {
        SubscriberResult Handle(object sender, object message, IMessengerContext messengerContext);
    }
}