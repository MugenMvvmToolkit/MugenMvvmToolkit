using System;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Messaging.Components;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTest.Messaging.Internal
{
    public class TestMessagePublisherComponent : IMessagePublisherComponent, IHasPriority
    {
        #region Fields

        private readonly IMessenger? _messenger;

        #endregion

        #region Constructors

        public TestMessagePublisherComponent(IMessenger? messenger)
        {
            _messenger = messenger;
        }

        #endregion

        #region Properties

        public Func<IMessageContext, bool>? TryPublish { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        bool IMessagePublisherComponent.TryPublish(IMessenger messenger, IMessageContext messageContext)
        {
            _messenger?.ShouldEqual(messenger);
            return TryPublish?.Invoke(messageContext) ?? false;
        }

        #endregion
    }
}