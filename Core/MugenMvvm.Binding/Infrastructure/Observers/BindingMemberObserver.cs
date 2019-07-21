using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Infrastructure.Observers
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct BindingMemberObserver
    {
        #region Fields

        private readonly IHandler _handler;
        public readonly object Member;

        #endregion

        #region Constructors

        public BindingMemberObserver(IHandler handler, object member)
        {
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
        public IDisposable? TryObserve(object? target, IBindingEventListener listener, IReadOnlyMetadataContext? metadata)
        {
            return _handler?.TryObserve(target, Member, listener, metadata);
        }

        #endregion

        #region Nested types

        public interface IHandler
        {
            IDisposable? TryObserve(object? target, object member, IBindingEventListener listener, IReadOnlyMetadataContext? metadata);
        }

        #endregion
    }
}