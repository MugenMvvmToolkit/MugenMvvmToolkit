using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Binding.Observation.Observers;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Binding.Members.Internal;
using Should;

namespace MugenMvvm.UnitTests.Binding.Observation.Observers
{
    public class RootMultiPathObserverTest : MultiPathObserverTestBase<RootMultiPathObserver>
    {
        #region Methods

        public override void ObserverShouldNotifyListenersLastMember(int count)
        {
            IEventListener? currentListener = null;
            IEventListener? lastListener = null;
            var root = this;
            var target1 = new object();
            var target2 = "";
            var accessorInfo1 = new TestAccessorMemberInfo
            {
                GetValue = (o, context) => target1
            };
            var accessorInfo2 = new TestAccessorMemberInfo
            {
                GetValue = (o, context) => target2
            };
            var accessorInfo3 = new TestAccessorMemberInfo
            {
                TryObserve = (o, listener, arg3) =>
                {
                    currentListener = listener;
                    lastListener = listener;
                    return new ActionToken((o1, o2) => currentListener = null);
                }
            };
            var component = new TestMemberManagerComponent
            {
                TryGetMembers = (t, m, f, r, meta) =>
                {
                    if (t == target2.GetType())
                        return accessorInfo3;
                    if (t == root.GetType())
                        return accessorInfo1;
                    if (t == target1.GetType())
                        return accessorInfo2;
                    throw new NotSupportedException();
                }
            };

            using var _ = MugenService.AddComponent(component);
            var observer = GetObserver(root, DefaultPath, MemberFlags.All, false, false);
            ObserverShouldManageListenerEvents(observer, ListenerMode.LastMember, count, () => lastListener?.TryHandle(this, this, DefaultMetadata), disposed => currentListener.ShouldBeNull(), 0);
        }

        protected override RootMultiPathObserver GetObserver(object target) => new RootMultiPathObserver(target, DefaultPath, MemberFlags.All, false, false);

        protected override RootMultiPathObserver GetObserver(object target, IMemberPath path, MemberFlags memberFlags, bool hasStablePath, bool optional) =>
            new RootMultiPathObserver(target, path, memberFlags, hasStablePath, optional);

        #endregion
    }
}