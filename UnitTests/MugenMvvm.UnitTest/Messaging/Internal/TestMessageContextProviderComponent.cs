using System;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Messaging.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTest.Messaging.Internal
{
    public class TestMessageContextProviderComponent : IMessageContextProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IMessenger? _messenger;

        #endregion

        #region Constructors

        public TestMessageContextProviderComponent(IMessenger? messenger)
        {
            _messenger = messenger;
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        public Func<object?, object, IReadOnlyMetadataContext?, IMessageContext?>? TryGetMessageContext { get; set; }

        #endregion

        #region Implementation of interfaces

        IMessageContext? IMessageContextProviderComponent.TryGetMessageContext(IMessenger messenger, object? sender, object message, IReadOnlyMetadataContext? metadata)
        {
            _messenger?.ShouldEqual(messenger);
            return TryGetMessageContext?.Invoke(sender, message, metadata);
        }

        #endregion
    }
}