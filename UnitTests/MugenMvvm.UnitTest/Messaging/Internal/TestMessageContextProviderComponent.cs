using System;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Messaging.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Messaging.Internal
{
    public class TestMessageContextProviderComponent : IMessageContextProviderComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        public Func<IMessenger, object?, object, IReadOnlyMetadataContext?, IMessageContext?>? TryGetMessageContext { get; set; }

        #endregion

        #region Implementation of interfaces

        IMessageContext? IMessageContextProviderComponent.TryGetMessageContext(IMessenger messenger, object? sender, object message, IReadOnlyMetadataContext? metadata)
        {
            return TryGetMessageContext?.Invoke(messenger, sender, message, metadata);
        }

        #endregion
    }
}