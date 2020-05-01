using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Observers.PathObservers
{
    public sealed class ObservableMethodSinglePathObserver : SinglePathObserver, ObserverBase.IMethodPathObserver
    {
        #region Fields

        private readonly string _method;

        private IWeakReference? _lastValueRef;
        private ActionToken _unsubscriber;

        #endregion

        #region Constructors

        public ObservableMethodSinglePathObserver(string method, object target, IMemberPath path, MemberFlags memberFlags, bool optional)
            : base(target, path, memberFlags, true, optional)
        {
            Should.NotBeNull(method, nameof(method));
            _method = method;
        }

        #endregion

        #region Properties

        MemberFlags IMethodPathObserver.MemberFlags => MemberFlags;

        string IMethodPathObserver.Method => _method;

        #endregion

        #region Implementation of interfaces

        IEventListener IMethodPathObserver.GetMethodListener()
        {
            return this;
        }

        #endregion

        #region Methods

        protected override void SubscribeLastMember(object target, IMemberInfo? lastMember, IReadOnlyMetadataContext? metadata)
        {
            base.SubscribeLastMember(target, lastMember, metadata);
            this.AddMethodObserver(target, lastMember, metadata, ref _unsubscriber, ref _lastValueRef);
        }

        protected override void OnLastMemberChanged()
        {
            base.OnLastMemberChanged();
            var lastMember = GetLastMember();
            this.AddMethodObserver(lastMember.Target, lastMember.Member, TryGetMetadata(), ref _unsubscriber, ref _lastValueRef);
        }

        protected override void UnsubscribeLastMember()
        {
            _unsubscriber.Dispose();
            base.UnsubscribeLastMember();
        }

        #endregion
    }
}