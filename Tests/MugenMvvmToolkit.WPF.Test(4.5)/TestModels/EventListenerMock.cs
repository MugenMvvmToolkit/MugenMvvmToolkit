using System;
using MugenMvvmToolkit.Binding.Interfaces.Models;

namespace MugenMvvmToolkit.Test.TestModels
{
    public class EventListenerMock : IEventListener
    {
        #region Constructors

        public EventListenerMock()
        {
            IsAlive = true;
        }

        #endregion

        #region Properties

        public Action<object, object> Handle { get; set; }

        #endregion

        #region Implementation of IEventListener

        /// <summary>
        ///     Gets an indication whether the object referenced by the current <see cref="IEventListener" /> object has
        ///     been garbage collected.
        /// </summary>
        /// <returns>
        ///     true if the object referenced by the current <see cref="IEventListener" /> object has not been garbage
        ///     collected and is still accessible; otherwise, false.
        /// </returns>
        public bool IsAlive { get; set; }

        /// <summary>
        ///     Gets the value that indicates that the listener is weak.
        ///     <c>true</c> the listener can be used without <c>WeakReference</c>/>.
        /// </summary>
        public bool IsWeak { get; set; }

        /// <summary>
        ///     Handles the message.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="message">Information about event.</param>
        /// <returns>
        ///     true if the object referenced by the current <see cref="IEventListener" /> object has not been garbage
        ///     collected and is still accessible; otherwise, false.
        /// </returns>
        public bool TryHandle(object sender, object message)
        {
            if (Handle != null)
                Handle(sender, message);
            return IsAlive;
        }

        #endregion
    }
}