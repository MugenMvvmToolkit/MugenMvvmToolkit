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

        private readonly IBindingMemberObserverCallback _observer;
        public readonly object Member;

        #endregion

        #region Constructors

        public BindingMemberObserver(object member, IBindingMemberObserverCallback observer)
        {
            Member = member;
            _observer = observer;
        }

        #endregion

        #region Methods

        [Pure]
        public IDisposable? TryObserve(object? source, IBindingEventListener listener, IReadOnlyMetadataContext metadata)
        {
            return _observer?.TryObserve(Member, source, listener, metadata);
        }

        #endregion
    }
}