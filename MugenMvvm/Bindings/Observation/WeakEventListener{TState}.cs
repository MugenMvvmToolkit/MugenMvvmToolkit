using System.Runtime.InteropServices;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Observation
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct WeakEventListener<TState>
    {
        public readonly TState State;
        public readonly object? Target;

        public WeakEventListener(IEventListener listener, TState state)
        {
            Target = WeakEventListener.GetTarget(listener);
            State = state;
        }

        public bool IsEmpty => Target == null;

        public bool IsAlive => WeakEventListener.GetIsAlive(Target);

        public IEventListener? Listener => WeakEventListener.GetListener(Target);

        public bool TryHandle(object? sender, object? message, IReadOnlyMetadataContext? metadata) => WeakEventListener.TryHandle(Target, sender, message, metadata);
    }
}