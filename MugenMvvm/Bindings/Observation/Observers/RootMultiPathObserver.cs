using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Observation.Observers
{
    public sealed class RootMultiPathObserver : MultiPathObserverBase
    {
        private ActionToken _unsubscriber;

        public RootMultiPathObserver(object target, IMemberPath path, EnumFlags<MemberFlags> memberFlags, bool hasStablePath, bool optional)
            : base(target, path, memberFlags, hasStablePath, optional)
        {
        }

        protected override void SubscribeMember(int index, object? target, IObservableMemberInfo member, IReadOnlyMetadataContext? metadata)
        {
            if (index == 0)
            {
                _unsubscriber.Dispose();
                _unsubscriber = member.TryObserve(target, this, metadata);
            }
        }

        protected override void SubscribeLastMember(object? target, IMemberInfo? lastMember, IReadOnlyMetadataContext? metadata)
        {
        }

        protected override void UnsubscribeLastMember()
        {
        }

        protected override void ClearListeners() => _unsubscriber.Dispose();
    }
}