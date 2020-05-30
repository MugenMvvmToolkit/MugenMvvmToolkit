using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Observers.PathObservers;
using MugenMvvm.Internal;
using MugenMvvm.UnitTest.Binding.Members.Internal;
using MugenMvvm.UnitTest.Internal.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Observers.PathObservers
{
    public class MethodEmptyPathObserverTest : ObserverBaseTest<MethodEmptyPathObserver>
    {
        #region Methods

        [Fact]
        public void GetMembersShouldReturnActualMembers()
        {
            var observer = new MethodEmptyPathObserver(MethodName, this, MemberFlags.All);
            var members = observer.GetMembers(DefaultMetadata);
            members.Members.ShouldEqual(ConstantMemberInfo.TargetArray);
            members.Target.ShouldEqual(this);
        }

        [Fact]
        public void GetLastMemberShouldReturnActualMembers()
        {
            var observer = new MethodEmptyPathObserver(MethodName, this, MemberFlags.All);
            var members = observer.GetLastMember(DefaultMetadata);
            members.Member.ShouldEqual(ConstantMemberInfo.Target);
            members.Target.ShouldEqual(this);
        }


        [Theory]
        [InlineData(1, true)]
        [InlineData(1, false)]
        [InlineData(10, true)]
        [InlineData(10, false)]
        public void ObserverShouldNotifyListenersMethodMember(int count, bool isStatic)
        {
            var flags = MemberFlags.Public.SetInstanceOrStaticFlags(isStatic);
            var lastMemberTarget = this;
            IEventListener? currentListener = null;
            IEventListener? lastListener = null;
            var methodMember = new TestMethodInfo
            {
                TryObserve = (o, listener, arg3) =>
                {
                    if (isStatic)
                        o.ShouldBeNull();
                    else
                        o.ShouldEqual(lastMemberTarget);
                    currentListener = listener;
                    lastListener = listener;
                    return new ActionToken((o1, o2) => currentListener = null);
                }
            };
            var component = new TestMemberManagerComponent
            {
                TryGetMembers = (t, m, f, r, tt, meta) =>
                {
                    if (r.Equals(MethodName))
                    {
                        t.ShouldEqual(lastMemberTarget.GetType());
                        m.ShouldEqual(MemberType.Method);
                        f.ShouldEqual(flags);
                        return methodMember;
                    }

                    throw new NotSupportedException();
                }
            };
            using var _ = TestComponentSubscriber.Subscribe(component);

            var observer = new MethodEmptyPathObserver(MethodName, this, flags);
            ObserverShouldManageListenerEvents(observer, ListenerMode.LastMember, count, () => lastListener?.TryHandle(this, null), disposed => currentListener.ShouldBeNull(), ignoreFirstMember: false);
        }

        protected override MethodEmptyPathObserver GetObserver(object target)
        {
            return new MethodEmptyPathObserver(MethodName, target, MemberFlags.All);
        }

        #endregion
    }
}