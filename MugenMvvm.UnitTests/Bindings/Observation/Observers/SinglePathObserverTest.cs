using System;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Metadata;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Bindings.Observation.Observers;
using MugenMvvm.Enums;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Bindings.Members.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Observation.Observers
{
    public class SinglePathObserverTest : ObserverBaseTest<SinglePathObserver>
    {
        protected static readonly IMemberPath DefaultPath = MemberPath.Get("test");

        [Fact]
        public void NonOptionalShouldReturnError()
        {
            var memberFlags = MemberFlags.All;
            var path = DefaultPath;
            var component = new TestMemberManagerComponent
            {
                TryGetMembers = (t, m, f, r, meta) => default
            };

            using var _ = MugenService.AddComponent(component);
            var singlePathObserver = GetObserver(this, path, memberFlags, false);
            var lastMember = singlePathObserver.GetLastMember(DefaultMetadata);
            lastMember.IsAvailable.ShouldBeFalse();
            lastMember.Target.ShouldEqual(BindingMetadata.UnsetValue);
            lastMember.Error.ShouldBeType<InvalidOperationException>();

            var members = singlePathObserver.GetMembers(DefaultMetadata);
            members.IsAvailable.ShouldBeFalse();
            members.Target.ShouldEqual(BindingMetadata.UnsetValue);
            members.Error.ShouldBeType<InvalidOperationException>();
        }

        [Fact]
        public void OptionalShouldIgnoreNullMember()
        {
            var memberFlags = MemberFlags.All;
            var path = DefaultPath;
            var component = new TestMemberManagerComponent
            {
                TryGetMembers = (t, m, f, r, meta) => default
            };

            using var _ = MugenService.AddComponent(component);
            var singlePathObserver = GetObserver(this, path, memberFlags, true);
            var lastMember = singlePathObserver.GetLastMember(DefaultMetadata);
            lastMember.IsAvailable.ShouldBeFalse();
            lastMember.Target.ShouldEqual(BindingMetadata.UnsetValue);
            lastMember.Error.ShouldBeNull();

            var members = singlePathObserver.GetMembers(DefaultMetadata);
            members.Members.Item.ShouldEqual(ConstantMemberInfo.Unset);
            members.IsAvailable.ShouldBeFalse();
            members.Target.ShouldEqual(BindingMetadata.UnsetValue);
            members.Error.ShouldBeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetMembersShouldReturnActualMembers(bool optional)
        {
            var accessorInfo = new TestAccessorMemberInfo();
            var memberFlags = MemberFlags.All;
            var path = DefaultPath;
            var component = new TestMemberManagerComponent
            {
                TryGetMembers = (t, m, f, r, meta) =>
                {
                    m.ShouldEqual(MemberType.Accessor | MemberType.Event);
                    t.ShouldEqual(GetType());
                    f.ShouldEqual(memberFlags);
                    r.ShouldEqual(path.Path);
                    return accessorInfo;
                }
            };

            using var _ = MugenService.AddComponent(component);
            var singlePathObserver = GetObserver(this, path, memberFlags, optional);
            var members = singlePathObserver.GetMembers(DefaultMetadata);
            members.Members.Item.ShouldEqual(accessorInfo);
            members.IsAvailable.ShouldBeTrue();
            members.Target.ShouldEqual(this);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetMembersShouldReturnError(bool optional)
        {
            var memberFlags = MemberFlags.All;
            var error = new Exception();
            var path = DefaultPath;
            var component = new TestMemberManagerComponent
            {
                TryGetMembers = (t, m, f, r, meta) => throw error
            };

            using var _ = MugenService.AddComponent(component);
            var singlePathObserver = GetObserver(this, path, memberFlags, optional);
            var members = singlePathObserver.GetMembers(DefaultMetadata);
            members.IsAvailable.ShouldBeFalse();
            members.Target.ShouldEqual(BindingMetadata.UnsetValue);
            members.Error.ShouldEqual(error);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetLastMemberShouldReturnActualMembers(bool optional)
        {
            var accessorInfo = new TestAccessorMemberInfo();
            var memberFlags = MemberFlags.All;
            var path = DefaultPath;
            var component = new TestMemberManagerComponent
            {
                TryGetMembers = (t, m, f, r, meta) =>
                {
                    m.ShouldEqual(MemberType.Accessor | MemberType.Event);
                    t.ShouldEqual(GetType());
                    f.ShouldEqual(memberFlags);
                    r.ShouldEqual(path.Path);
                    return accessorInfo;
                }
            };

            using var _ = MugenService.AddComponent(component);
            var singlePathObserver = GetObserver(this, path, memberFlags, optional);
            var members = singlePathObserver.GetLastMember(DefaultMetadata);
            members.Member.ShouldEqual(accessorInfo);
            members.IsAvailable.ShouldBeTrue();
            members.Target.ShouldEqual(this);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetLastMemberShouldReturnError(bool optional)
        {
            var memberFlags = MemberFlags.All;
            var error = new Exception();
            var path = DefaultPath;
            var component = new TestMemberManagerComponent
            {
                TryGetMembers = (t, m, f, r, meta) => throw error
            };

            using var _ = MugenService.AddComponent(component);
            var singlePathObserver = GetObserver(this, path, memberFlags, optional);
            var members = singlePathObserver.GetLastMember(DefaultMetadata);
            members.IsAvailable.ShouldBeFalse();
            members.Target.ShouldEqual(BindingMetadata.UnsetValue);
            members.Error.ShouldEqual(error);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ObserverShouldNotifyListenersLastMember(int count)
        {
            IEventListener? currentListener = null;
            IEventListener? lastListener = null;
            var accessorInfo = new TestAccessorMemberInfo
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
                TryGetMembers = (t, m, f, r, meta) => accessorInfo
            };
            using var _ = MugenService.AddComponent(component);

            var observer = GetObserver(this, DefaultPath, MemberFlags.All, false);
            ObserverShouldManageListenerEvents(observer, ListenerMode.LastMember, count, () => lastListener?.TryHandle(this, this, DefaultMetadata),
                disposed => currentListener.ShouldBeNull());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ObserverShouldNotifyListenersError(int count)
        {
            IEventListener? currentListener = null;
            var component = new TestMemberManagerComponent
            {
                TryGetMembers = (t, m, f, r, meta) => default
            };
            using var _ = MugenService.AddComponent(component);

            var observer = GetObserver(this, DefaultPath, MemberFlags.All, false);
            ObserverShouldManageListenerEvents(observer, ListenerMode.Error, count, () => observer.GetMembers(), disposed => currentListener.ShouldBeNull());
        }

        protected virtual SinglePathObserver GetObserver(object target, IMemberPath path, EnumFlags<MemberFlags> memberFlags, bool optional) =>
            new(target, path, memberFlags, optional);

        protected override SinglePathObserver GetObserver(object target) => new(target, DefaultPath, MemberFlags.InstancePublic, true);
    }
}