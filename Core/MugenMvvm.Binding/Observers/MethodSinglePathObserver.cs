using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Binding.Observers
{
    public sealed class MethodSinglePathObserver : SinglePathObserver, ObserverBase.IMethodPathObserver
    {
        #region Fields

        private readonly string _method;

        private IWeakReference? _lastValueRef;
        private Unsubscriber _unsubscriber;

        #endregion

        #region Constructors

        public MethodSinglePathObserver(string method, object target, IMemberPath path, BindingMemberFlags memberFlags, bool optional)
            : base(target, path, memberFlags, true, optional)
        {
            Should.NotBeNull(method, nameof(method));
            _method = method;
        }

        #endregion

        #region Properties

        BindingMemberFlags IMethodPathObserver.MemberFlags => MemberFlags;

        string IMethodPathObserver.Method => _method;

        #endregion

        #region Implementation of interfaces

        IEventListener IMethodPathObserver.GetMethodListener()
        {
            return this;
        }

        #endregion

        #region Methods

        protected override void SubscribeLastMember(object target, IBindingMemberInfo? lastMember)
        {
            base.SubscribeLastMember(target, lastMember);
            this.AddMethodObserver(target, lastMember, ref _unsubscriber, ref _lastValueRef);
        }

        protected override void OnLastMemberChanged()
        {
            base.OnLastMemberChanged();
            var lastMember = GetLastMember();
            this.AddMethodObserver(lastMember.Target, lastMember.LastMember, ref _unsubscriber, ref _lastValueRef);
        }

        protected override void UnsubscribeLastMember()
        {
            _unsubscriber.Unsubscribe();
            _unsubscriber = default;
        }

        #endregion
    }
}