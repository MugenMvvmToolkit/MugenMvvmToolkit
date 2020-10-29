using System.Runtime.InteropServices;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Observation
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

        public bool TryHandle(object? sender, object? message, IReadOnlyMetadataContext? metadata) => TryHandle(Target, sender, message, metadata);

        public static object GetTarget(IEventListener listener)
        {
            Should.NotBeNull(listener, nameof(listener));
            if (listener is IWeakEventListener weakEventListener && weakEventListener.IsWeak)
                return listener;
            return listener.ToWeakReference();
        }

        public static bool GetIsAlive(object? target)
        {
            if (target == null)
                return false;
            if (target is IWeakReference reference)
            {
                target = reference.Target;
                return target != null && (!(target is IWeakItem item) || item.IsAlive);
            }

            if (target is IWeakItem weakItem)
                return weakItem.IsAlive;
            return true;
        }

        public static IEventListener? GetListener(object? target)
        {
            if (target == null)
                return null;
            if (target is IEventListener listener)
                return listener;
            return (IEventListener?) ((IWeakReference) target).Target;
        }

        public static bool TryHandle(object? target, object? sender, object? message, IReadOnlyMetadataContext? metadata)
        {
            if (target == null)
                return false;

            if (target is IEventListener listener)
                return listener.TryHandle(sender, message, metadata);

            listener = (IEventListener) ((IWeakReference) target).Target!;
            return listener != null && listener.TryHandle(sender, message, metadata);
        }

        #endregion
    }
}