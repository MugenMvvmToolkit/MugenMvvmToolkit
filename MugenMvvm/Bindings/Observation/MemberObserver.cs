using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Observation
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct MemberObserver : IEquatable<MemberObserver>
    {
        public static readonly MemberObserver NoDo = new((_, __, ___, ____) => default, "");

        private readonly Func<object?, object, IEventListener, IReadOnlyMetadataContext?, ActionToken>? _handler;
        private readonly object? _member;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemberObserver(Func<object?, object, IEventListener, IReadOnlyMetadataContext?, ActionToken> handler, object member)
        {
            Should.NotBeNull(handler, nameof(handler));
            Should.NotBeNull(member, nameof(member));
            _member = member;
            _handler = handler;
        }

        [MemberNotNullWhen(false, nameof(_handler))]
        public bool IsEmpty => _handler == null;

        public void Deconstruct(out Func<object?, object, IEventListener, IReadOnlyMetadataContext?, ActionToken>? handler, out object? member)
        {
            handler = _handler;
            member = _member;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemberObserver NoDoIfEmpty() => IsEmpty ? NoDo : this;

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ActionToken TryObserve(object? target, IEventListener listener, IReadOnlyMetadataContext? metadata)
        {
            if (_handler == null)
                return default;
            return _handler.Invoke(target, _member!, listener, metadata);
        }

        public bool Equals(MemberObserver other) => Equals(_handler, other._handler) && Equals(_member, other._member);

        public override bool Equals(object? obj) => obj is MemberObserver other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(_handler, _member);
    }
}