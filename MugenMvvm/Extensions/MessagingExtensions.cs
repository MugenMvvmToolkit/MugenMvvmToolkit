using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static IMessageContext Publish(this IMessagePublisher publisher, object message, IReadOnlyMetadataContext? metadata = null)
        {
            return publisher.Publish(null, message, metadata);
        }

        public static IMessageContext Publish(this IMessagePublisher publisher, object? sender, object message, IReadOnlyMetadataContext? metadata = null)
        {
            var messageContext = publisher.GetMessageContext(sender, message, metadata);
            publisher.Publish(messageContext);
            return messageContext;
        }

        public static bool SubscribeWeak(this IMessenger messenger, object subscriber, ThreadExecutionMode? executionMode = null, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(messenger, nameof(messenger));
            Should.NotBeNull(subscriber, nameof(subscriber));
            return messenger.TrySubscribe(subscriber.ToWeakReference(), executionMode, metadata);
        }

        #endregion
    }
}