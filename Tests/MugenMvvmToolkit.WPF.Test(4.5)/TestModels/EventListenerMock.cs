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

        public bool IsAlive { get; set; }

        public bool IsWeak { get; set; }

        public bool TryHandle(object sender, object message)
        {
            if (Handle != null)
                Handle(sender, message);
            return IsAlive;
        }

        #endregion
    }
}
