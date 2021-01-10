using System;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Metadata;
using MugenMvvm.Bindings.Observation.Observers;
using MugenMvvm.Bindings.Observation.Paths;
using MugenMvvm.Enums;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Bindings.Members.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Observation.Observers
{
    public abstract class MultiPathObserverTestBase<TObserver> : ObserverBaseTest<TObserver> where TObserver : MultiPathObserverBase
    {
        #region Fields

        protected const string MemberPath1 = "Test1";
        protected const string MemberPath2 = "Test2";
        protected const string MemberPath3 = "Test3";
        protected static readonly MultiMemberPath DefaultPath = new($"{MemberPath1}.{MemberPath2}.{MemberPath3}");

        #endregion

        #region Methods

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(false, false, true)]
        [InlineData(false, true, false)]
        [InlineData(false, true, true)]
        [InlineData(true, false, false)]
        [InlineData(true, false, true)]
        [InlineData(true, true, false)]
        [InlineData(true, true, true)]
        public void GetMembersShouldReturnActualMembers(bool hasStablePath, bool optional, bool isStatic)
        {
            var memberFlags = MemberFlags.InstancePublic.SetInstanceOrStaticFlags(isStatic);
            var getMembersCount = 0;
            var canReturn = false;
            var root = this;
            var target1 = new object();
            var target2 = "";
            var target3 = 1;
            var path = DefaultPath;
            IEventListener? rootListener = null;
            var accessorInfo1 = new TestAccessorMemberInfo
            {
                GetValue = (o, context) =>
                {
                    if (isStatic)
                        o.ShouldBeNull();
                    else
                        o.ShouldEqual(root);
                    return target1;
                },
                TryObserve = (o, listener, arg3) =>
                {
                    if (isStatic)
                        o.ShouldBeNull();
                    else
                        o.ShouldEqual(root);
                    rootListener = listener;
                    return new ActionToken((o1, o2) => rootListener = null);
                }
            };
            var accessorInfo2 = new TestAccessorMemberInfo
            {
                GetValue = (o, context) =>
                {
                    o.ShouldEqual(target1);
                    return target2;
                }
            };
            var accessorInfo3 = new TestAccessorMemberInfo
            {
                GetValue = (o, context) =>
                {
                    o.ShouldEqual(target2);
                    return target3;
                }
            };

            var component = new TestMemberManagerComponent
            {
                TryGetMembers = (t, m, f, r, meta) =>
                {
                    ++getMembersCount;
                    if (t == target2.GetType())
                    {
                        f.ShouldEqual(memberFlags.SetInstanceOrStaticFlags(false));
                        m.ShouldEqual(MemberType.Accessor | MemberType.Event);
                        return accessorInfo3;
                    }

                    m.ShouldEqual(MemberType.Accessor);
                    if (t == root.GetType())
                    {
                        f.ShouldEqual(memberFlags);
                        return accessorInfo1;
                    }

                    if (!canReturn)
                        return default;
                    f.ShouldEqual(memberFlags.SetInstanceOrStaticFlags(false));
                    if (t == target1.GetType())
                        return accessorInfo2;
                    throw new NotSupportedException();
                }
            };

            using var _ = MugenService.AddComponent(component);
            var observer = GetObserver(root, path, memberFlags, hasStablePath, optional);
            var members = observer.GetMembers(DefaultMetadata);
            members.IsAvailable.ShouldBeFalse();
            members.Target.ShouldEqual(BindingMetadata.UnsetValue);
            if (optional)
                members.Error.ShouldBeNull();
            else
                members.Error.ShouldBeType<InvalidOperationException>();

            canReturn = true;
            getMembersCount = 0;
            rootListener.ShouldNotBeNull();
            rootListener!.TryHandle(this, this, DefaultMetadata);
            members = observer.GetMembers(DefaultMetadata);
            members.Members.ShouldEqual(new[] {accessorInfo1, accessorInfo2, accessorInfo3});
            members.IsAvailable.ShouldBeTrue();
            members.Target.ShouldEqual(root);
            getMembersCount.ShouldEqual(3);

            getMembersCount = 0;
            rootListener!.TryHandle(this, this, DefaultMetadata);
            members = observer.GetMembers(DefaultMetadata);
            members.Members.ShouldEqual(new[] {accessorInfo1, accessorInfo2, accessorInfo3});
            members.IsAvailable.ShouldBeTrue();
            members.Target.ShouldEqual(root);
            if (hasStablePath)
                getMembersCount.ShouldEqual(0);
            else
                getMembersCount.ShouldEqual(3);
        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(false, false, true)]
        [InlineData(false, true, false)]
        [InlineData(false, true, true)]
        [InlineData(true, false, false)]
        [InlineData(true, false, true)]
        [InlineData(true, true, false)]
        [InlineData(true, true, true)]
        public void GetLastMemberShouldReturnActualMembers(bool hasStablePath, bool optional, bool isStatic)
        {
            var memberFlags = MemberFlags.InstancePublic.SetInstanceOrStaticFlags(isStatic);
            var getMembersCount = 0;
            var canReturn = false;
            var root = this;
            var target1 = new object();
            var target2 = "";
            var target3 = 1;
            var path = DefaultPath;
            IEventListener? rootListener = null;
            var accessorInfo1 = new TestAccessorMemberInfo
            {
                GetValue = (o, context) =>
                {
                    if (isStatic)
                        o.ShouldBeNull();
                    else
                        o.ShouldEqual(root);
                    return target1;
                },
                TryObserve = (o, listener, arg3) =>
                {
                    if (isStatic)
                        o.ShouldBeNull();
                    else
                        o.ShouldEqual(root);
                    rootListener = listener;
                    return new ActionToken((o1, o2) => rootListener = null);
                }
            };
            var accessorInfo2 = new TestAccessorMemberInfo
            {
                GetValue = (o, context) =>
                {
                    o.ShouldEqual(target1);
                    return target2;
                }
            };
            var accessorInfo3 = new TestAccessorMemberInfo
            {
                GetValue = (o, context) =>
                {
                    o.ShouldEqual(target2);
                    return target3;
                }
            };

            var component = new TestMemberManagerComponent
            {
                TryGetMembers = (t, m, f, r, meta) =>
                {
                    ++getMembersCount;
                    if (t == target2.GetType())
                    {
                        f.ShouldEqual(memberFlags.SetInstanceOrStaticFlags(false));
                        m.ShouldEqual(MemberType.Accessor | MemberType.Event);
                        return accessorInfo3;
                    }

                    m.ShouldEqual(MemberType.Accessor);
                    if (t == root.GetType())
                    {
                        f.ShouldEqual(memberFlags);
                        return accessorInfo1;
                    }

                    if (!canReturn)
                        return default;
                    f.ShouldEqual(memberFlags.SetInstanceOrStaticFlags(false));
                    if (t == target1.GetType())
                        return accessorInfo2;
                    throw new NotSupportedException();
                }
            };

            using var _ = MugenService.AddComponent(component);
            var observer = GetObserver(root, path, memberFlags, hasStablePath, optional);
            var members = observer.GetLastMember(DefaultMetadata);
            members.IsAvailable.ShouldBeFalse();
            members.Member.ShouldEqual(ConstantMemberInfo.Unset);
            members.Target.ShouldEqual(BindingMetadata.UnsetValue);
            if (optional)
                members.Error.ShouldBeNull();
            else
                members.Error.ShouldBeType<InvalidOperationException>();

            canReturn = true;
            getMembersCount = 0;
            rootListener.ShouldNotBeNull();
            rootListener!.TryHandle(this, this, DefaultMetadata);
            members = observer.GetLastMember(DefaultMetadata);
            members.Member.ShouldEqual(accessorInfo3);
            members.IsAvailable.ShouldBeTrue();
            members.Target.ShouldEqual(target2);
            getMembersCount.ShouldEqual(3);

            getMembersCount = 0;
            rootListener!.TryHandle(this, this, DefaultMetadata);
            members = observer.GetLastMember(DefaultMetadata);
            members.Member.ShouldEqual(accessorInfo3);
            members.IsAvailable.ShouldBeTrue();
            members.Target.ShouldEqual(target2);
            if (hasStablePath)
                getMembersCount.ShouldEqual(0);
            else
                getMembersCount.ShouldEqual(3);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void GetMembersShouldReturnError(bool hasStablePath, bool optional)
        {
            var memberFlags = MemberFlags.All;
            var error = new Exception();
            var path = DefaultPath;
            var component = new TestMemberManagerComponent
            {
                TryGetMembers = (t, m, f, r, meta) => throw error
            };

            using var _ = MugenService.AddComponent(component);
            var observer = GetObserver(this, path, memberFlags, hasStablePath, optional);
            var members = observer.GetMembers(DefaultMetadata);
            members.IsAvailable.ShouldBeFalse();
            members.Target.ShouldEqual(BindingMetadata.UnsetValue);
            members.Error.ShouldEqual(error);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void GetLastMemberShouldReturnError(bool hasStablePath, bool optional)
        {
            var memberFlags = MemberFlags.All;
            var error = new Exception();
            var path = DefaultPath;
            var component = new TestMemberManagerComponent
            {
                TryGetMembers = (t, m, f, r, meta) => throw error
            };

            using var _ = MugenService.AddComponent(component);
            var observer = GetObserver(this, path, memberFlags, hasStablePath, optional);
            var members = observer.GetLastMember(DefaultMetadata);
            members.IsAvailable.ShouldBeFalse();
            members.Target.ShouldEqual(BindingMetadata.UnsetValue);
            members.Error.ShouldEqual(error);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ObserverShouldNotifyListenersPathMember(int count)
        {
            IEventListener? currentListener = null;
            IEventListener? lastListener = null;
            var root = this;
            var target1 = new object();
            var target2 = "";
            var accessorInfo1 = new TestAccessorMemberInfo
            {
                GetValue = (o, context) => target1,
                TryObserve = (o, listener, arg3) =>
                {
                    currentListener = listener;
                    lastListener = listener;
                    return new ActionToken((o1, o2) => currentListener = null);
                }
            };
            var accessorInfo2 = new TestAccessorMemberInfo
            {
                GetValue = (o, context) => target2
            };
            var accessorInfo3 = new TestAccessorMemberInfo();
            var component = new TestMemberManagerComponent
            {
                TryGetMembers = (t, m, f, r, meta) =>
                {
                    if (t == target2.GetType())
                        return accessorInfo3;
                    if (t == root.GetType())
                        return accessorInfo1;
                    if (t == target1.GetType())
                        return accessorInfo2;
                    throw new NotSupportedException();
                }
            };

            using var _ = MugenService.AddComponent(component);
            var observer = GetObserver(root, DefaultPath, MemberFlags.All, false, false);
            ObserverShouldManageListenerEvents(observer, ListenerMode.Members, count, () => lastListener?.TryHandle(this, this, DefaultMetadata), disposed =>
            {
                if (disposed)
                    currentListener.ShouldBeNull();
            });
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public virtual void ObserverShouldNotifyListenersLastMember(int count)
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
                    return new ActionToken((o1, o2) => currentListener = null);
                }
            };
            var component = new TestMemberManagerComponent
            {
                TryGetMembers = (t, m, f, r, meta) =>
                {
                    if (t == target2.GetType())
                        return accessorInfo3;
                    if (t == root.GetType())
                        return accessorInfo1;
                    if (t == target1.GetType())
                        return accessorInfo2;
                    throw new NotSupportedException();
                }
            };

            using var _ = MugenService.AddComponent(component);
            var observer = GetObserver(root, DefaultPath, MemberFlags.All, false, false);
            ObserverShouldManageListenerEvents(observer, ListenerMode.LastMember, count, () => lastListener?.TryHandle(this, this, DefaultMetadata), disposed => currentListener.ShouldBeNull(), ignoreFirstMember: false);
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

            var observer = GetObserver(this, DefaultPath, MemberFlags.All, false, false);
            ObserverShouldManageListenerEvents(observer, ListenerMode.Error, count, () => observer.GetMembers(), disposed => currentListener.ShouldBeNull());
        }

        protected abstract TObserver GetObserver(object target, IMemberPath path, EnumFlags<MemberFlags> memberFlags, bool hasStablePath, bool optional);

        #endregion
    }
}