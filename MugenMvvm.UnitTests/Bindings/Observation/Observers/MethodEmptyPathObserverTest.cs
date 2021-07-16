using System;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Observation.Observers;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using MugenMvvm.Tests.Bindings.Members;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Observation.Observers
{
    [Collection(SharedContext)]
    public class MethodEmptyPathObserverTest : ObserverBaseTest<MethodEmptyPathObserver>
    {
        [Fact]
        public void GetLastMemberShouldReturnActualMembers()
        {
            var observer = new MethodEmptyPathObserver(MethodName, this, MemberFlags.All);
            var members = observer.GetLastMember(Metadata);
            members.Member.ShouldEqual(ConstantMemberInfo.Target);
            members.Target.ShouldEqual(this);
        }

        [Fact]
        public void GetMembersShouldReturnActualMembers()
        {
            var observer = new MethodEmptyPathObserver(MethodName, this, MemberFlags.All);
            var members = observer.GetMembers(Metadata);
            members.Members.Item.ShouldEqual(ConstantMemberInfo.Target);
            members.Target.ShouldEqual(this);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(1, false)]
        [InlineData(10, true)]
        [InlineData(10, false)]
        public void ObserverShouldNotifyListenersMethodMember(int count, bool isStatic)
        {
            var flags = MemberFlags.Public.AsFlags().SetInstanceOrStaticFlags(isStatic);
            var lastMemberTarget = this;
            IEventListener? currentListener = null;
            IEventListener? lastListener = null;
            var methodMember = new TestMethodMemberInfo
            {
                TryObserve = (o, listener, arg3) =>
                {
                    if (isStatic)
                        o.ShouldBeNull();
                    else
                        o.ShouldEqual(lastMemberTarget);
                    currentListener = listener;
                    lastListener = listener;
                    return ActionToken.FromDelegate((o1, o2) => currentListener = null);
                }
            };

            MemberManager.AddComponent(new TestMemberManagerComponent
            {
                TryGetMembers = (_, t, m, f, r, meta) =>
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
            });

            var observer = new MethodEmptyPathObserver(MethodName, this, flags);
            ObserverShouldManageListenerEvents(observer, ListenerMode.LastMember, count, () => lastListener?.TryHandle(this, this, Metadata),
                disposed => currentListener.ShouldBeNull(), ignoreFirstMember: false);
        }

        protected override MethodEmptyPathObserver GetObserver(object target) => new(MethodName, target, MemberFlags.All);
    }
}