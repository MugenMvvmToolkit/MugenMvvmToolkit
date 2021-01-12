using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Observation
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct MemberObserver
    {
        #region Fields

        public static readonly MemberObserver NoDo = new((_, __, ___, ____) => default, "");

        private readonly Func<object?, object, IEventListener, IReadOnlyMetadataContext?, ActionToken>? _handler;
        private readonly object? _member;

        #endregion

        #region Constructors

        public MemberObserver(Func<object?, object, IEventListener, IReadOnlyMetadataContext?, ActionToken> handler, object member)
        {
            Should.NotBeNull(handler, nameof(handler));
            Should.NotBeNull(member, nameof(member));
            _member = member;
            _handler = handler;
        }

        #endregion

        #region Properties

        public bool IsEmpty => _handler == null;

        #endregion

        #region Methods

        public void Deconstruct(out Func<object?, object, IEventListener, IReadOnlyMetadataContext?, ActionToken>? handler, out object? member)
        {
            handler = _handler;
            member = _member;
        }

        public MemberObserver NoDoIfEmpty() => IsEmpty ? NoDo : this;

        [Pure]
        public ActionToken TryObserve(object? target, IEventListener listener, IReadOnlyMetadataContext? metadata)
        {
            if (_handler == null)
                return default;
            return _handler.Invoke(target, _member!, listener, metadata);
        }

        #endregion
    }
}