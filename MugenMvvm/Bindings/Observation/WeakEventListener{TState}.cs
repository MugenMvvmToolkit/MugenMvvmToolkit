using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Observation
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct WeakEventListener<TState> : IEquatable<WeakEventListener<TState>>
    {
        public readonly TState State;
        public readonly object? Target;

        public WeakEventListener(IEventListener listener, TState state)
        {
            Target = WeakEventListener.GetTarget(listener);
            State = state;
        }

        [MemberNotNullWhen(false, nameof(Target))]
        public bool IsEmpty => Target == null;

        public bool IsAlive => WeakEventListener.GetIsAlive(Target);

        public IEventListener? Listener => WeakEventListener.GetListener(Target);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryHandle(object? sender, object? message, IReadOnlyMetadataContext? metadata) => WeakEventListener.TryHandle(Target, sender, message, metadata);

        public bool Equals(WeakEventListener<TState> other) => EqualityComparer<TState>.Default.Equals(State, other.State) && Equals(Target, other.Target);

        public override bool Equals(object? obj) => obj is WeakEventListener<TState> other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(State, Target);
    }
}