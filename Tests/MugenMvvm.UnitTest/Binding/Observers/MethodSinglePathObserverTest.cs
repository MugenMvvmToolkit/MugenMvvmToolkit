using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Observers.PathObservers;
using MugenMvvm.UnitTest.Binding.Members;
using MugenMvvm.UnitTest.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Observers
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
            var accessorInfo = new TestMemberAccessorInfo
            {
                GetValue = (o, context) =>
                {
                    o.ShouldEqual(this);
                    return lastMemberTarget;
                }
            };
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
                    var request = (MemberManagerRequest)r;
                    if (request.Name == MethodName)
                    {
                        request.Type.ShouldEqual(lastMemberTarget.GetType());
                        request.MemberTypes.ShouldEqual(MemberType.Method);
                        request.Flags.ShouldEqual(MemberFlags.All.ClearInstanceOrStaticFlags(false));
                        return methodMember;
                    }
                    return accessorInfo;
                }
            };
            using var _ = TestComponentSubscriber.Subscribe(component);

            var observer = GetObserver(this, DefaultPath, MemberFlags.All, false);
            ObserverShouldManageListenerEvents(observer, ListenerMode.LastMember, count, () => lastListener?.TryHandle(this, null), disposed => currentListener.ShouldBeNull());
        }

        protected override SinglePathObserver GetObserver(object target)
        {
            return new MethodSinglePathObserver(MethodName, target, DefaultPath, MemberFlags.InstancePublic, true);
        }

        protected override SinglePathObserver GetObserver(object target, IMemberPath path, MemberFlags memberFlags, bool optional)
        {
            return new MethodSinglePathObserver(MethodName, target, path, memberFlags, optional);
        }

        #endregion
    }
}