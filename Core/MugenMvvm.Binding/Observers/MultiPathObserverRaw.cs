using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;

namespace MugenMvvm.Binding.Observers
{
    public sealed class MultiPathObserverRaw : MultiPathObserverBase
    {
        #region Constructors

        public MultiPathObserverRaw(object target, IMemberPath path, MemberFlags memberFlags, bool hasStablePath, bool optional)
            : base(target, path, memberFlags, hasStablePath, optional)
        {
        }

        #endregion

        #region Methods

        protected override void SubscribeMember(int index, object target, IObservableMemberInfo member)
        {
        }

        protected override void SubscribeLastMember(object target, IMemberInfo? lastMember)
        {
        }

        protected override void UnsubscribeLastMember()
        {
        }

        protected override void ClearListeners()
        {
        }

        #endregion
    }
}