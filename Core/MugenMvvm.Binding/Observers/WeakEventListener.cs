using System.Runtime.InteropServices;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Binding.Observers
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct WeakEventListener
    {
        #region Fields

        public readonly object Target;

        #endregion

        #region Constructors

        public WeakEventListener(IEventListener listener)
        {
            Target = GetTarget(listener);
        }

        #endregion

        #region Properties

        public bool IsEmpty => Target == null;

        public bool IsAlive => GetIsAlive(Target);

        public IEventListener? Listener => GetListener(Target);

        #endregion

        #region Methods

        public bool TryHandle(object sender, object? message)
        {
            return TryHandle(Target, sender, message);
        }

        public static object GetTarget(IEventListener listener)
        {
            Should.NotBeNull(listener, nameof(listener));
            if (listener.IsWeak)
                return listener;
            return listener.ToWeakReference();
        }

        public static bool GetIsAlive(object? target)
        {
            if (target == null)
                return false;
            if (target is IEventListener listener)
                return listener.IsAlive;
            listener = (IEventListener)((IWeakReference)target).Target!;
            return listener != null && listener.IsAlive;
        }

        public static IEventListener? GetListener(object? target)
        {
            if (target == null)
                return null;
            if (target is IEventListener listener)
                return listener;
            return (IEventListener?)((IWeakReference)target).Target;
        }

        public static bool TryHandle(object? target, object sender, object? message)
        {
            if (target == null)
                return false;

            if (target is IEventListener listener)
                return listener.TryHandle(sender, message);

            listener = (IEventListener)((IWeakReference)target).Target!;
            return listener != null && listener.TryHandle(sender, message);
        }

        #endregion
    }
}