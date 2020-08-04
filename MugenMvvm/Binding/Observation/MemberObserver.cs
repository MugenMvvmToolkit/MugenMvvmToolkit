using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Observation
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct MemberObserver
    {
        #region Fields

        public static readonly MemberObserver NoDo = new MemberObserver((_, __, ___, ____) => default, Default.Metadata);

        public readonly Func<object?, object, IEventListener, IReadOnlyMetadataContext?, ActionToken> Handler;
        public readonly object Member;

        #endregion

        #region Constructors

        public MemberObserver(Func<object?, object, IEventListener, IReadOnlyMetadataContext?, ActionToken> handler, object member)
        {
            Should.NotBeNull(handler, nameof(handler));
            Should.NotBeNull(member, nameof(member));
            Member = member;
            Handler = handler;
        }

        #endregion

        #region Properties

        public bool IsEmpty => Handler == null;

        #endregion

        #region Methods

        public MemberObserver NoDoIfEmpty() => IsEmpty ? NoDo : this;

        [Pure]
        public ActionToken TryObserve(object? target, IEventListener listener, IReadOnlyMetadataContext? metadata)
        {
            if (Handler == null)
                return default;
            return Handler.Invoke(target, Member, listener, metadata);
        }

        #endregion
    }
}