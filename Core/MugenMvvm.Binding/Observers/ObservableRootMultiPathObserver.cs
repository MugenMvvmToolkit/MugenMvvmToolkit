using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;

namespace MugenMvvm.Binding.Observers
{
    public sealed class ObservableRootMultiPathObserver : MultiPathObserverBase
    {
        #region Fields

        private ActionToken _unsubscriber;

        #endregion

        #region Constructors

        public ObservableRootMultiPathObserver(object target, IMemberPath path, BindingMemberFlags memberFlags, bool hasStablePath, bool optional)
            : base(target, path, memberFlags, hasStablePath, optional)
        {
        }

        #endregion

        #region Methods

        protected override void SubscribeMember(int index, object target, IObservableBindingMemberInfo member)
        {
            if (index == 0)
            {
                _unsubscriber.Dispose();
                _unsubscriber = member.TryObserve(target, this);
            }
        }

        protected override void SubscribeLastMember(object target, IBindingMemberInfo? lastMember)
        {
        }

        protected override void UnsubscribeLastMember()
        {
        }

        protected override void ClearListeners()
        {
            _unsubscriber.Dispose();
            _unsubscriber = default;
        }

        #endregion
    }
}