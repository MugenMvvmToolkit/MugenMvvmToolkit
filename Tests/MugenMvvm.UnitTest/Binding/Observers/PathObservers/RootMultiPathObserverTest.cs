using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Observers.PathObservers;
using MugenMvvm.UnitTest.Binding.Members.Internal;
using MugenMvvm.UnitTest.Internal.Internal;
using Should;

namespace MugenMvvm.UnitTest.Binding.Observers.PathObservers
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
            var accessorInfo1 = new TestMemberAccessorInfo
            {
                GetValue = (o, context) => target1
            };
            var accessorInfo2 = new TestMemberAccessorInfo
            {
                GetValue = (o, context) => target2
            };
            var accessorInfo3 = new TestMemberAccessorInfo
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
                TryGetMembers = (r, type, arg3) =>
                {
                    var request = (MemberManagerRequest) r;
                    if (request.Type == target2.GetType())
                        return accessorInfo3;
                    if (request.Type == root.GetType())
                        return accessorInfo1;
                    if (request.Type == target1.GetType())
                        return accessorInfo2;
                    throw new NotSupportedException();
                }
            };

            using var _ = TestComponentSubscriber.Subscribe(component);
            var observer = GetObserver(root, DefaultPath, MemberFlags.All, false, false);
            ObserverShouldManageListenerEvents(observer, ListenerMode.LastMember, count, () => lastListener?.TryHandle(this, null), disposed => currentListener.ShouldBeNull(), 0);
        }

        protected override RootMultiPathObserver GetObserver(object target)
        {
            return new RootMultiPathObserver(target, DefaultPath, MemberFlags.All, false, false);
        }

        protected override RootMultiPathObserver GetObserver(object target, IMemberPath path, MemberFlags memberFlags, bool hasStablePath, bool optional)
        {
            return new RootMultiPathObserver(target, path, memberFlags, hasStablePath, optional);
        }

        #endregion
    }
}