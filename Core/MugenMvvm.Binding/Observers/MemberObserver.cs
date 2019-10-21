﻿using System.Runtime.InteropServices;
using JetBrains.Annotations;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Observers
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct MemberObserver
    {
        #region Fields

        private readonly IHandler _handler;
        public readonly object Member;

        #endregion

        #region Constructors

        public MemberObserver(IHandler handler, object member)
        {
            Member = member;
            _handler = handler;
        }

        #endregion

        #region Properties

        public bool IsEmpty => _handler == null;

        #endregion

        #region Methods

        [Pure]
        public Unsubscriber TryObserve(object? source, IEventListener listener, IReadOnlyMetadataContext? metadata)
        {
            if (_handler == null)
                return default;
            return _handler.TryObserve(source, Member, listener, metadata);
        }

        #endregion

        #region Nested types

        public interface IHandler
        {
            Unsubscriber TryObserve(object? source, object member, IEventListener listener, IReadOnlyMetadataContext? metadata);
        }

        #endregion
    }
}