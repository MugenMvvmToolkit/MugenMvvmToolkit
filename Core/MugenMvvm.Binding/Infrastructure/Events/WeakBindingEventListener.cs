using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using MugenMvvm.Binding.Interfaces.Events;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Binding.Infrastructure.Events
{
    public class WeakBindingEventListener
    {
        #region Fields

        private object _item;
        private bool _isWeak;

        #endregion

        #region Constructors

        public WeakBindingEventListener(IBindingEventListener listener)
        {
            _isWeak = listener.IsWeak;
            if (_isWeak)
                _item = listener;
            else
                _item = Service<IWeakReferenceProvider>.Instance.GetWeakReference(listener, Default.Metadata);
        }

        #endregion

        #region Properties

        public bool IsEmpty => _item == null;

        public object Source => _item;

        public bool TryHandle(object sender, object message)
        {

        }

        #endregion
    }
}
