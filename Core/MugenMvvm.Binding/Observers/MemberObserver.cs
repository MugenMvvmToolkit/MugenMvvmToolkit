using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Observers
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct MemberObserver
    {
        #region Fields

        private readonly Func<object?, object, IEventListener, IReadOnlyMetadataContext?, ActionToken> _handler;
        public readonly object Member;

        #endregion

        #region Constructors

        public MemberObserver(Func<object?, object, IEventListener, IReadOnlyMetadataContext?, ActionToken> handler, object member)
        {
            Should.NotBeNull(handler, nameof(handler));
            Should.NotBeNull(member, nameof(member));
            Member = member;
            _handler = handler;
        }

        #endregion

        #region Properties

        public bool IsEmpty => _handler == null;

        #endregion

        #region Methods

        [Pure]
        public ActionToken TryObserve(object? target, IEventListener listener, IReadOnlyMetadataContext? metadata)
        {
            if (_handler == null)
                return default;
            return _handler.Invoke(target, Member, listener, metadata);
        }

        #endregion
    }
}