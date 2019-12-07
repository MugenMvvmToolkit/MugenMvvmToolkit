using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Members;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Observers
{
    public sealed class ObservableMethodEmptyPathObserver : ObserverBase, IEventListener, IValueHolder<IWeakReference>
    {
        #region Fields

        private readonly MemberFlags _memberFlags;
        private readonly string _method;

        private ActionToken _unsubscriber;

        #endregion

        #region Constructors

        public ObservableMethodEmptyPathObserver(string method, object target, MemberFlags memberFlags) : base(target)
        {
            Should.NotBeNull(method, nameof(method));
            _memberFlags = memberFlags;
            _method = method;
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

            return new MemberPathMembers(target, ConstantMemberInfo.TargetArray);
        }

        public override MemberPathLastMember GetLastMember(IReadOnlyMetadataContext? metadata = null)
        {
            var target = Target;
            if (target == null)
                return default;

            return new MemberPathLastMember(target, ConstantMemberInfo.Target);
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
                _unsubscriber = ActionToken.NoDoToken;
            else
            {
                var member = MugenBindingService.MemberProvider.GetMember(_memberFlags.GetTargetType(target), _method, MemberType.Method, _memberFlags);
                if (member is IObservableMemberInfo observable)
                    _unsubscriber = observable.TryObserve(target, this);
                if (_unsubscriber.IsEmpty)
                    _unsubscriber = ActionToken.NoDoToken;
            }
        }

        protected override void OnListenersRemoved()
        {
            _unsubscriber.Dispose();
        }

        #endregion
    }
}