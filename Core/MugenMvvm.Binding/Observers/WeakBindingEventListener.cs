using System.Runtime.InteropServices;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Binding.Observers
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct WeakBindingEventListener
    {
        #region Fields

        public readonly object Source;

        #endregion

        #region Constructors

        public WeakBindingEventListener(IBindingEventListener listener)
        {
            if (listener.IsWeak)
                Source = listener;
            else
                Source = listener.ToWeakReference();
        }

        #endregion

        #region Properties

        public bool IsEmpty => Source == null;

        public bool IsAlive
        {
            get
            {
                if (Source == null)
                    return false;
                if (Source is IBindingEventListener listener)
                    return listener.IsAlive;
                listener = (IBindingEventListener)((IWeakReference)Source).Target!;
                return listener != null && listener.IsAlive;
            }
        }

        public IBindingEventListener? Listener
        {
            get
            {
                if (Source == null)
                    return null;
                if (Source is IBindingEventListener listener)
                    return listener;
                return (IBindingEventListener?)((IWeakReference)Source).Target;
            }
        }

        public bool TryHandle(object sender, object? message)
        {
            if (Source == null)
                return false;

            if (Source is IBindingEventListener listener)
                return listener.TryHandle(sender, message);

            listener = (IBindingEventListener)((IWeakReference)Source).Target!;
            return listener != null && listener.TryHandle(sender, message);
        }

        #endregion
    }
}