using System;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Messaging.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Messaging
{
    public class TestMessagePublisherComponent : IMessagePublisherComponent, IHasPriority
    {
        #region Properties

        public Action<IMessageContext>? TryPublish { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        void IMessagePublisherComponent.TryPublish(IMessageContext messageContext)
        {
            TryPublish?.Invoke(messageContext);
        }

        #endregion
    }
}