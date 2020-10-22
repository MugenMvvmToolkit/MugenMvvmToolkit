using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Binding.Observation.Observers;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Binding.Members.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Binding.Observation.Observers
{
    public class MethodSinglePathObserverTest : SinglePathObserverTest
    {
        #region Methods

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ObserverShouldNotifyListenersMethodMember(int count)
        {
            var lastMemberTarget = "";
            IEventListener? currentListener = null;
            IEventListener? lastListener = null;
            var accessorInfo = new TestAccessorMemberInfo
            {
                GetValue = (o, context) =>
                {
                    o.ShouldEqual(this);
                    return lastMemberTarget;
                }
            };
            var methodMember = new TestMethodMemberInfo
            {
                TryObserve = (o, listener, arg3) =>
                {
                    o.ShouldEqual(lastMemberTarget);
                    currentListener = listener;
                    lastListener = listener;
                    return new ActionToken((o1, o2) => currentListener = null);
                }
            };
            var component = new TestMemberManagerComponent
            {
                TryGetMembers = (t, m, f, r, meta) =>
                {
                    if (r.Equals(MethodName))
                    {
                        t.ShouldEqual(lastMemberTarget.GetType());
                        m.ShouldEqual(MemberType.Method);
                        f.ShouldEqual(MemberFlags.All.ClearInstanceOrStaticFlags(false));
                        return methodMember;
                    }

                    return accessorInfo;
                }
            };
            using var _ = MugenService.AddComponent(component);

            var observer = GetObserver(this, DefaultPath, MemberFlags.All, false);
            ObserverShouldManageListenerEvents(observer, ListenerMode.LastMember, count, () => lastListener?.TryHandle(this, this, DefaultMetadata), disposed => currentListener.ShouldBeNull());
        }

        protected override SinglePathObserver GetObserver(object target) => new MethodSinglePathObserver(MethodName, target, DefaultPath, MemberFlags.InstancePublic, true);

        protected override SinglePathObserver GetObserver(object target, IMemberPath path, MemberFlags memberFlags, bool optional) => new MethodSinglePathObserver(MethodName, target, path, memberFlags, optional);

        #endregion
    }
}