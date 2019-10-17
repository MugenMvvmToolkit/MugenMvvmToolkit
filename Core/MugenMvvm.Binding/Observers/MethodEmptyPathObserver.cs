using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Members;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Observers
{
    public sealed class MethodEmptyPathObserver : ObserverBase, IEventListener, IWeakReferenceHolder
    {
        #region Fields

        private readonly MemberFlags _memberFlags;
        private readonly string _observableMethodName;

        private IDisposable? _unsubscriber;

        #endregion

        #region Constructors

        public MethodEmptyPathObserver(string observableMethodName, object source, MemberFlags memberFlags) : base(source)
        {
            Should.NotBeNull(observableMethodName, nameof(observableMethodName));
            _memberFlags = memberFlags;
            _observableMethodName = observableMethodName;
        }

        #endregion

        #region Properties

        public override IMemberPath Path => EmptyMemberPath.Instance;

        public bool IsWeak => false;

        IWeakReference? IWeakReferenceHolder.WeakReference { get; set; }

        #endregion

        #region Implementation of interfaces

        bool IEventListener.TryHandle(object sender, object? message)
        {
            OnLastMemberChanged();
            return true;
        }

        #endregion

        #region Methods

        public override MemberPathMembers GetMembers(IReadOnlyMetadataContext? metadata = null)
        {
            var source = Source;
            if (source == null)
                return default;

            return new MemberPathMembers(Path, source, source, ConstantBindingMemberInfo.NullArray, ConstantBindingMemberInfo.Null);
        }

        public override MemberPathLastMember GetLastMember(IReadOnlyMetadataContext? metadata = null)
        {
            var source = Source;
            if (source == null)
                return default;

            return new MemberPathLastMember(Path, source, ConstantBindingMemberInfo.Null);
        }

        protected override void OnDisposed()
        {
            OnListenersRemoved();
        }

        protected override void OnListenerAdded(IMemberPathObserverListener listener)
        {
            if (_unsubscriber != null)
                return;
            var source = Source;
            if (source == null)
                _unsubscriber = Default.Disposable;
            else
            {
                var member = MugenBindingService.MemberProvider.GetMember(source as Type ?? source.GetType(), _observableMethodName, BindingMemberType.Method, _memberFlags);
                _unsubscriber = (member as IObservableBindingMemberInfo)?.TryObserve(source, this) ?? Default.Disposable;
            }
        }

        protected override void OnListenersRemoved()
        {
            _unsubscriber?.Dispose();
            _unsubscriber = null;
        }

        #endregion
    }
}