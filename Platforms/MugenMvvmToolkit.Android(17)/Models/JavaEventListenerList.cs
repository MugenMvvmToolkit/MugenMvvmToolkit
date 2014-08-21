using System;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Collections;
using MugenMvvmToolkit.Interfaces.Models;
using Object = Java.Lang.Object;

namespace MugenMvvmToolkit.Models
{
    internal abstract class JavaEventListenerList : Object
    {
        #region Fields

        private readonly EventListenerList _listeners;

        #endregion

        #region Constructors

        protected JavaEventListenerList()
        {
            _listeners = new EventListenerList();
        }

        #endregion

        #region Methods

        public IDisposable AddListner(IEventListener listener)
        {
            return _listeners.AddWithUnsubscriber(listener);
        }

        public void Raise<TArg>(object sender, TArg args)
        {
            _listeners.Raise(sender, args);
        }

        #endregion
    }
}