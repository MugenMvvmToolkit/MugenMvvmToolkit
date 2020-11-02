using System;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Observation.Observers;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Bindings.Members.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Observation.Observers
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
                GetValue = (o, context) =>
                {
                    o.ShouldEqual(target2);
                    return target3;
                }
            };
            var methodMember = new TestMethodMemberInfo
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
                TryGetMembers = (t, m, f, r, meta) =>
                {
                    if (r.Equals(MethodName))
                    {
                        t.ShouldEqual(target3.GetType());
                        m.ShouldEqual(MemberType.Method);
                        f.ShouldEqual(MemberFlags.All.ClearInstanceOrStaticFlags(false));
                        return ItemOrList.FromItem<IMemberInfo>(methodMember);
                    }

                    if (t == target2.GetType())
                        return ItemOrList.FromItem<IMemberInfo>(accessorInfo3);
                    if (t == root.GetType())
                        return ItemOrList.FromItem<IMemberInfo>(accessorInfo1);
                    if (t == target1.GetType())
                        return ItemOrList.FromItem<IMemberInfo>(accessorInfo2);
                    throw new NotSupportedException();
                }
            };

            using var _ = MugenService.AddComponent(component);
            var observer = GetObserver(root, DefaultPath, MemberFlags.All, false, false);
            ObserverShouldManageListenerEvents(observer, ListenerMode.LastMember, count, () => lastListener?.TryHandle(this, this, DefaultMetadata), disposed => currentListener.ShouldBeNull(),
                isValueType ? 0 : 1, false);
        }

        protected override MultiPathObserver GetObserver(object target, IMemberPath path, MemberFlags memberFlags, bool hasStablePath, bool optional) =>
            new MethodMultiPathObserver(MethodName, target, path, memberFlags, hasStablePath, optional);

        protected override MultiPathObserver GetObserver(object target) => new MethodMultiPathObserver(MethodName, target, DefaultPath, MemberFlags.All, false, false);

        #endregion
    }
}