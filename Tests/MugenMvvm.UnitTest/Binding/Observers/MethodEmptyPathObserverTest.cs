using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Observers.PathObservers;
using MugenMvvm.UnitTest.Binding.Members;
using MugenMvvm.UnitTest.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Observers
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
        [InlineData(1)]
        [InlineData(10)]
        public void ObserverShouldNotifyListenersMethodMember(int count)
        {
            var lastMemberTarget = this;
            IEventListener? currentListener = null;
            IEventListener? lastListener = null;
            var methodMember = new TestMethodInfo
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
                TryGetMembers = (r, type, arg3) =>
                {
                    var request = (MemberManagerRequest) r;
                    if (request.Name == MethodName)
                    {
                        request.Type.ShouldEqual(lastMemberTarget.GetType());
                        request.MemberTypes.ShouldEqual(MemberType.Method);
                        request.Flags.ShouldEqual(MemberFlags.All);
                        return methodMember;
                    }

                    throw new NotSupportedException();
                }
            };
            using var _ = TestComponentSubscriber.Subscribe(component);

            var observer = new MethodEmptyPathObserver(MethodName, this, MemberFlags.All);
            ObserverShouldManageListenerEvents(observer, ListenerMode.LastMember, count, () => lastListener?.TryHandle(this, null), disposed => currentListener.ShouldBeNull(), ignoreFirstMember: false);
        }

        protected override MethodEmptyPathObserver GetObserver(object target)
        {
            return new MethodEmptyPathObserver(MethodName, target, MemberFlags.All);
        }

        #endregion
    }
}