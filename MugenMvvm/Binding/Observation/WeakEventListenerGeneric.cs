using System.Runtime.InteropServices;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Observation
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct WeakEventListener<TState>
    {
        #region Fields

        public readonly TState State;
        public readonly object Target;

        #endregion

        #region Constructors

        public WeakEventListener(IEventListener listener, TState state)
        {
            Target = WeakEventListener.GetTarget(listener);
            State = state;
        }

        #endregion

        #region Properties

        public bool IsEmpty => Target == null;

        public bool IsAlive => WeakEventListener.GetIsAlive(Target);

        public IEventListener? Listener => WeakEventListener.GetListener(Target);

        #endregion

        #region Methods

        public bool TryHandle(object? sender, object? message, IReadOnlyMetadataContext? metadata)
        {
            return WeakEventListener.TryHandle(Target, sender, message, metadata);
        }

        #endregion
    }
}