﻿using System;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Observation.Observers;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using MugenMvvm.Tests.Bindings.Members;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Observation.Observers
{
    [Collection(SharedContext)]
    public class RootMultiPathObserverTest : MultiPathObserverTestBase<RootMultiPathObserver>
    {
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
                    return ActionToken.FromDelegate((o1, o2) => currentListener = null);
                }
            };

            MemberManager.AddComponent(new TestMemberManagerComponent
            {
                TryGetMembers = (_, t, m, f, r, meta) =>
                {
                    if (t == target2.GetType())
                        return accessorInfo3;
                    if (t == root.GetType())
                        return accessorInfo1;
                    if (t == target1.GetType())
                        return accessorInfo2;
                    throw new NotSupportedException();
                }
            });
            var observer = GetObserver(root, DefaultPath, MemberFlags.All, false, false);
            ObserverShouldManageListenerEvents(observer, ListenerMode.LastMember, count, () => lastListener?.TryHandle(this, this, DefaultMetadata),
                disposed => currentListener.ShouldBeNull(), 0);
        }

        protected override RootMultiPathObserver GetObserver(object target) => new(target, DefaultPath, MemberFlags.All, false, false);

        protected override RootMultiPathObserver GetObserver(object target, IMemberPath path, EnumFlags<MemberFlags> memberFlags, bool hasStablePath, bool optional) =>
            new(target, path, memberFlags, hasStablePath, optional);
    }
}