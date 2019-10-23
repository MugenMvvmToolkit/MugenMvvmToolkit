using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Members;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Observers
{
    public sealed class MethodEmptyPathObserver : ObserverBase, IEventListener, IValueHolder<IWeakReference>
    {
        #region Fields

        private readonly MemberFlags _memberFlags;
        private readonly string _observableMethodName;

        private Unsubscriber _unsubscriber;

        #endregion

        #region Constructors

        public MethodEmptyPathObserver(string observableMethodName, object target, MemberFlags memberFlags) : base(target)
        {
            Should.NotBeNull(observableMethodName, nameof(observableMethodName));
            _memberFlags = memberFlags;
            _observableMethodName = observableMethodName;
        }

        #endregion

        #region Properties

        public override IMemberPath Path => EmptyMemberPath.Instance;

        public bool IsWeak => false;

        IWeakReference? IValueHolder<IWeakReference>.Value { get; set; }

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
            var target = Target;
            if (target == null)
                return default;

            return new MemberPathMembers(target, ConstantBindingMemberInfo.NullArray);
        }

        public override MemberPathLastMember GetLastMember(IReadOnlyMetadataContext? metadata = null)
        {
            var target = Target;
            if (target == null)
                return default;

            return new MemberPathLastMember(target, ConstantBindingMemberInfo.Null);
        }

        protected override void OnDisposed()
        {
            OnListenersRemoved();
            this.ReleaseWeakReference();
        }

        protected override void OnListenerAdded(IMemberPathObserverListener listener)
        {
            if (!_unsubscriber.IsEmpty)
                return;
            var target = Target;
            if (target == null)
                _unsubscriber = Unsubscriber.NoDoUnsubscriber;
            else
            {
                var member = MugenBindingService.MemberProvider.GetMember(GetTargetType(target, _memberFlags), _observableMethodName, BindingMemberType.Method, _memberFlags);
                if (member is IObservableBindingMemberInfo observable)
                    _unsubscriber = observable.TryObserve(target, this);
                if (_unsubscriber.IsEmpty)
                    _unsubscriber = Unsubscriber.NoDoUnsubscriber;
            }
        }

        protected override void OnListenersRemoved()
        {
            _unsubscriber.Unsubscribe();
            _unsubscriber = default;
        }

        #endregion
    }
}