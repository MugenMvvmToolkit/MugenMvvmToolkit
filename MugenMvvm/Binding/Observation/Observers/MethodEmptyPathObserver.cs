using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Observation.Paths;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Observation.Observers
{
    public sealed class MethodEmptyPathObserver : ObserverBase, IEventListener, IValueHolder<IWeakReference>
    {
        #region Fields

        private readonly MemberFlags _memberFlags;
        private readonly string _method;

        private ActionToken _unsubscriber;

        #endregion

        #region Constructors

        public MethodEmptyPathObserver(string method, object target, MemberFlags memberFlags) : base(target)
        {
            Should.NotBeNull(method, nameof(method));
            _memberFlags = memberFlags;
            _method = method;
            CanDispose = true;
        }

        #endregion

        #region Properties

        public override IMemberPath Path => EmptyMemberPath.Instance;

        public override bool CanDispose { get; set; }

        IWeakReference? IValueHolder<IWeakReference>.Value { get; set; }

        #endregion

        #region Implementation of interfaces

        bool IEventListener.TryHandle(object? sender, object? message, IReadOnlyMetadataContext? metadata)
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

        protected override void OnListenersAdded()
        {
            if (!_unsubscriber.IsEmpty)
                return;
            var target = Target;
            if (target == null)
                _unsubscriber = ActionToken.NoDoToken;
            else
            {
                var member = MugenBindingService.MemberManager.TryGetMember(_memberFlags.GetTargetType(ref target), MemberType.Method, _memberFlags, _method, TryGetMetadata());
                if (member is IObservableMemberInfo observable)
                    _unsubscriber = observable.TryObserve(target, this, TryGetMetadata());
                if (_unsubscriber.IsEmpty)
                    _unsubscriber = ActionToken.NoDoToken;
            }
        }

        protected override void OnListenersRemoved() => _unsubscriber.Dispose();

        #endregion
    }
}