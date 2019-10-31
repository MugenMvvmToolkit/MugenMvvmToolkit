using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Messaging.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Messaging.Components
{
    public sealed class MessengerHandlerSubscriberComponent : IMessengerSubscriberComponent
    {
        #region Fields

        private readonly bool _isWeak;

        private readonly IReflectionDelegateProvider? _reflectionDelegateProvider;

        public static readonly MessengerHandlerSubscriberComponent Instance = new MessengerHandlerSubscriberComponent(false);
        public static readonly MessengerHandlerSubscriberComponent InstanceWeak = new MessengerHandlerSubscriberComponent(true);

        #endregion

        #region Constructors

        public MessengerHandlerSubscriberComponent(bool isWeak, IReflectionDelegateProvider? reflectionDelegateProvider = null)
        {
            _reflectionDelegateProvider = reflectionDelegateProvider;
            _isWeak = isWeak;
        }

        #endregion

        #region Implementation of interfaces

        public object? TryGetSubscriber(object subscriber, ThreadExecutionMode executionMode, IReadOnlyMetadataContext? metadata)
        {
            if (subscriber is IMessengerHandler handler)
                return new MessengerHandlerSubscriber(handler, _isWeak, _reflectionDelegateProvider);
            return null;
        }

        #endregion
    }
}