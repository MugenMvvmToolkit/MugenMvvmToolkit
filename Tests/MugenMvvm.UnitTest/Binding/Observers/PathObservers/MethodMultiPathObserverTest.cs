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
    public class MethodMultiPathObserverTest : MultiPathObserverTest
    {
        #region Methods

        [Theory]
        [InlineData(1, false)]
        [InlineData(1, true)]
        [InlineData(10, false)]
        [InlineData(10, true)]
        public void ObserverShouldNotifyListenersMethodMember(int count, bool isValueType)
        {
            IEventListener? currentListener = null;
            IEventListener? lastListener = null;
            var root = this;
            var target1 = new object();
            var target2 = "";
            var target3 = isValueType ? 1 : new object();
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
                GetValue = (o, context) =>
                {
                    o.ShouldEqual(target2);
                    return target3;
                }
            };
            var methodMember = new TestMethodInfo
            {
                TryObserve = (o, listener, arg3) =>
                {
                    o.ShouldEqual(target3);
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
                        t.ShouldEqual(target3.GetType());
                        m.ShouldEqual(MemberType.Method);
                        f.ShouldEqual(MemberFlags.All.ClearInstanceOrStaticFlags(false));
                        return methodMember;
                    }

                    if (t == target2.GetType())
                        return accessorInfo3;
                    if (t == root.GetType())
                        return accessorInfo1;
                    if (t == target1.GetType())
                        return accessorInfo2;
                    throw new NotSupportedException();
                }
            };

            using var _ = TestComponentSubscriber.Subscribe(component);
            var observer = GetObserver(root, DefaultPath, MemberFlags.All, false, false);
            ObserverShouldManageListenerEvents(observer, ListenerMode.LastMember, count, () => lastListener?.TryHandle(this, null), disposed => currentListener.ShouldBeNull(),
                isValueType ? 0 : 1, false);
        }

        protected override MultiPathObserver GetObserver(object target, IMemberPath path, MemberFlags memberFlags, bool hasStablePath, bool optional)
        {
            return new MethodMultiPathObserver(MethodName, target, path, memberFlags, hasStablePath, optional);
        }

        protected override MultiPathObserver GetObserver(object target)
        {
            return new MethodMultiPathObserver(MethodName, target, DefaultPath, MemberFlags.All, false, false);
        }

        #endregion
    }
}