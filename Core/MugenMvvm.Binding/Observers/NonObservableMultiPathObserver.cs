using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Observers
{
    public sealed class NonObservableMultiPathObserver : MultiPathObserverBase
    {
        #region Constructors

        public NonObservableMultiPathObserver(object target, IMemberPath path, MemberFlags memberFlags, bool hasStablePath, bool optional)
            : base(target, path, memberFlags, hasStablePath, optional)
        {
        }

        #endregion

        #region Methods

        protected override void SubscribeMember(int index, object target, IObservableMemberInfo member, IReadOnlyMetadataContext? metadata)
        {
        }

        protected override void SubscribeLastMember(object target, IMemberInfo? lastMember, IReadOnlyMetadataContext? metadata)
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