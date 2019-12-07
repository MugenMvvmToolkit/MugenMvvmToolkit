using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Binding.Observers
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

        protected override void SubscribeLastMember(object target, IMemberInfo? lastMember)
        {
            base.SubscribeLastMember(target, lastMember);
            this.AddMethodObserver(target, lastMember, ref _unsubscriber, ref _lastValueRef);
        }

        protected override void OnLastMemberChanged()
        {
            base.OnLastMemberChanged();
            var lastMember = GetLastMember();
            this.AddMethodObserver(lastMember.Target, lastMember.Member, ref _unsubscriber, ref _lastValueRef);
        }

        protected override void UnsubscribeLastMember()
        {
            _unsubscriber.Dispose();
            base.UnsubscribeLastMember();
        }

        #endregion
    }
}