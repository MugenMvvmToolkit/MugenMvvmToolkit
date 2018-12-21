using System.Collections.Generic;
using MugenMvvm.Models;

namespace MugenMvvm.Interfaces.Messaging
{
    public interface IMessenger
    {
        IReadOnlyList<IMessengerSubscriber> GetSubscribers();

        IMessengerContext GetContext(IContext? metadata);

        void Publish(object sender, object message, IMessengerContext? messengerContext = null);

        void Subscribe(IMessengerSubscriber subscriber, ThreadExecutionMode? executionMode = null);

        void Unsubscribe(IMessengerSubscriber subscriber);
    }
}