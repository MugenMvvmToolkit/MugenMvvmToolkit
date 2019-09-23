using System.Runtime.InteropServices;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Binding.Observers
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct WeakEventListener
    {
        #region Fields

        public readonly object Source;

        #endregion

        #region Constructors

        public WeakEventListener(IEventListener listener)
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
                if (Source is IEventListener listener)
                    return listener.IsAlive;
                listener = (IEventListener)((IWeakReference)Source).Target!;
                return listener != null && listener.IsAlive;
            }
        }

        public IEventListener? Listener
        {
            get
            {
                if (Source == null)
                    return null;
                if (Source is IEventListener listener)
                    return listener;
                return (IEventListener?)((IWeakReference)Source).Target;
            }
        }

        public bool TryHandle(object sender, object? message)
        {
            if (Source == null)
                return false;

            if (Source is IEventListener listener)
                return listener.TryHandle(sender, message);

            listener = (IEventListener)((IWeakReference)Source).Target!;
            return listener != null && listener.TryHandle(sender, message);
        }

        #endregion
    }
}