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
            Source = GetSource(listener);
        }

        #endregion

        #region Properties

        public bool IsEmpty => Source == null;

        public bool IsAlive => GetIsAlive(Source);

        public IEventListener? Listener => GetListener(Source);

        #endregion

        #region Methods

        public bool TryHandle(object sender, object? message) => TryHandle(Source, sender, message);

        public static object GetSource(IEventListener listener)
        {
            if (listener.IsWeak)
                return listener;
            return listener.ToWeakReference();
        }

        public static bool GetIsAlive(object? source)
        {
            if (source == null)
                return false;
            if (source is IEventListener listener)
                return listener.IsAlive;
            listener = (IEventListener)((IWeakReference)source).Target!;
            return listener != null && listener.IsAlive;
        }

        public static IEventListener? GetListener(object? source)
        {
            if (source == null)
                return null;
            if (source is IEventListener listener)
                return listener;
            return (IEventListener?)((IWeakReference)source).Target;
        }

        public static bool TryHandle(object? source, object sender, object? message)
        {
            if (source == null)
                return false;

            if (source is IEventListener listener)
                return listener.TryHandle(sender, message);

            listener = (IEventListener)((IWeakReference)source).Target!;
            return listener != null && listener.TryHandle(sender, message);
        }

        #endregion
    }
}