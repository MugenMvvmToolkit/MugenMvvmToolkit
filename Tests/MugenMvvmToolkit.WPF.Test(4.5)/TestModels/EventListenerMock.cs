using System;
using MugenMvvmToolkit.Binding.Interfaces.Models;

namespace MugenMvvmToolkit.Test.TestModels
{
    public class EventListenerMock : IEventListener
    {
        #region Properties

        public Action<object, object> Handle { get; set; }

        #endregion

        #region Implementation of IEventListener

        /// <summary>
        ///     Handles the message.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="message">Information about event.</param>
        void IEventListener.Handle(object sender, object message)
        {
            if (Handle != null)
                Handle(sender, message);
        }

        #endregion
    }
}