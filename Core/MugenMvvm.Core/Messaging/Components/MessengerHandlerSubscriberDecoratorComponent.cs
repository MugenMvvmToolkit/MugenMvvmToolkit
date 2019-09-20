using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Messaging.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Messaging.Components
{
    public sealed class MessengerHandlerSubscriberDecoratorComponent : IMessengerSubscriberDecoratorComponent
    {
        #region Fields

        private readonly bool _isWeak;

        public static readonly MessengerHandlerSubscriberDecoratorComponent Instance = new MessengerHandlerSubscriberDecoratorComponent(false);
        public static readonly MessengerHandlerSubscriberDecoratorComponent InstanceWeak = new MessengerHandlerSubscriberDecoratorComponent(true);

        #endregion

        #region Constructors

        public MessengerHandlerSubscriberDecoratorComponent(bool isWeak)
        {
            _isWeak = isWeak;
        }

        #endregion

        #region Implementation of interfaces

        public object? OnSubscribing(object subscriber, ThreadExecutionMode executionMode, IReadOnlyMetadataContext? metadata)
        {
            if (subscriber is IMessengerHandler handler)
                return new MessengerHandlerSubscriber(handler, _isWeak);
            return subscriber;
        }

        #endregion
    }
}