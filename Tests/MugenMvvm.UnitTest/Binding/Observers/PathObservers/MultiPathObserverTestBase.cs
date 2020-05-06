using System;
using System.Linq;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Metadata;
using MugenMvvm.Binding.Observers.MemberPaths;
using MugenMvvm.Binding.Observers.PathObservers;
using MugenMvvm.UnitTest.Binding.Members.Internal;
using MugenMvvm.UnitTest.Internal.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Observers.PathObservers
{
    public abstract class MultiPathObserverTestBase<TObserver> : ObserverBaseTest<TObserver> where TObserver : MultiPathObserverBase
    {
        #region Fields

        protected const string MemberPath1 = "Test1";
        protected const string MemberPath2 = "Test2";
        protected const string MemberPath3 = "Test3";
        protected static readonly MultiMemberPath DefaultPath = new MultiMemberPath($"{MemberPath1}.{MemberPath2}.{MemberPath3}");

        #endregion

        #region Methods

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void GetMembersShouldReturnActualMembers(bool hasStablePath, bool optional)
        {
            var getMembersCount = 0;
            var canReturn = false;
            var root = this;
            var target1 = new object();
            var target2 = "";
            var target3 = 1;
            var path = DefaultPath;
            IEventListener? rootListener = null;
            var accessorInfo1 = new TestMemberAccessorInfo
            {
                GetValue = (o, context) =>
                {
                    o.ShouldEqual(root);
                    return target1;
                },
                TryObserve = (o, listener, arg3) =>
                {
                    o.ShouldEqual(root);
                    rootListener = listener;
                    return new ActionToken((o1, o2) => rootListener = null);
                }
            };
            var accessorInfo2 = new TestMemberAccessorInfo
            {
                GetValue = (o, context) =>
                {
                    o.ShouldEqual(target1);
                    return target2;
                }
            };
            var accessorInfo3 = new TestMemberAccessorInfo
            {
                GetValue = (o, context) =>
                {
                    o.ShouldEqual(target2);
                    return target3;
                }
            };
            var memberFlags = MemberFlags.All;

            var component = new TestMemberManagerComponent
            {
                TryGetMembers = (r, type, arg3) =>
                {
                    ++getMembersCount;
                    var request = (MemberManagerRequest)r;
                    if (request.Type == target2.GetType())
                    {
                        request.Flags.ShouldEqual(memberFlags.ClearInstanceOrStaticFlags(false));
                        request.MemberTypes.ShouldEqual(MemberType.Accessor | MemberType.Event);
                        return accessorInfo3;
                    }

                    request.MemberTypes.ShouldEqual(MemberType.Accessor);
                    if (request.Type == root.GetType())
                    {
                        request.Flags.ShouldEqual(memberFlags);
                        return accessorInfo1;
                    }

                    if (!canReturn)
                        return default;
                    request.Flags.ShouldEqual(memberFlags.ClearInstanceOrStaticFlags(false));
                    if (request.Type == target1.GetType())
                        return accessorInfo2;
                    throw new NotSupportedException();
                }
            };

            using var _ = TestComponentSubscriber.Subscribe(component);
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
            rootListener!.TryHandle(this, null);
            members = observer.GetMembers(DefaultMetadata);
            members.Members.SequenceEqual(new[] { accessorInfo1, accessorInfo2, accessorInfo3 }).ShouldBeTrue();
            members.IsAvailable.ShouldBeTrue();
            members.Target.ShouldEqual(root);
            getMembersCount.ShouldEqual(3);

            getMembersCount = 0;
            rootListener.TryHandle(this, null);
            members = observer.GetMembers(DefaultMetadata);
            members.Members.SequenceEqual(new[] { accessorInfo1, accessorInfo2, accessorInfo3 }).ShouldBeTrue();
            members.IsAvailable.ShouldBeTrue();
            members.Target.ShouldEqual(root);
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
        public void GetLastMemberShouldReturnActualMembers(bool hasStablePath, bool optional)
        {
            var getMembersCount = 0;
            var canReturn = false;
            var root = this;
            var target1 = new object();
            var target2 = "";
            var target3 = 1;
            var path = DefaultPath;
            IEventListener? rootListener = null;
            var accessorInfo1 = new TestMemberAccessorInfo
            {
                GetValue = (o, context) =>
                {
                    o.ShouldEqual(root);
                    return target1;
                },
                TryObserve = (o, listener, arg3) =>
                {
                    o.ShouldEqual(root);
                    rootListener = listener;
                    return new ActionToken((o1, o2) => rootListener = null);
                }
            };
            var accessorInfo2 = new TestMemberAccessorInfo
            {
                GetValue = (o, context) =>
                {
                    o.ShouldEqual(target1);
                    return target2;
                }
            };
            var accessorInfo3 = new TestMemberAccessorInfo
            {
                GetValue = (o, context) =>
                {
                    o.ShouldEqual(target2);
                    return target3;
                }
            };
            var memberFlags = MemberFlags.All;

            var component = new TestMemberManagerComponent
            {
                TryGetMembers = (r, type, arg3) =>
                {
                    ++getMembersCount;
                    var request = (MemberManagerRequest)r;
                    if (request.Type == target2.GetType())
                    {
                        request.Flags.ShouldEqual(memberFlags.ClearInstanceOrStaticFlags(false));
                        request.MemberTypes.ShouldEqual(MemberType.Accessor | MemberType.Event);
                        return accessorInfo3;
                    }

                    request.MemberTypes.ShouldEqual(MemberType.Accessor);
                    if (request.Type == root.GetType())
                    {
                        request.Flags.ShouldEqual(memberFlags);
                        return accessorInfo1;
                    }

                    if (!canReturn)
                        return default;
                    request.Flags.ShouldEqual(memberFlags.ClearInstanceOrStaticFlags(false));
                    if (request.Type == target1.GetType())
                        return accessorInfo2;
                    throw new NotSupportedException();
                }
            };

            using var _ = TestComponentSubscriber.Subscribe(component);
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
            rootListener!.TryHandle(this, null);
            members = observer.GetLastMember(DefaultMetadata);
            members.Member.ShouldEqual(accessorInfo3);
            members.IsAvailable.ShouldBeTrue();
            members.Target.ShouldEqual(target2);
            getMembersCount.ShouldEqual(3);

            getMembersCount = 0;
            rootListener.TryHandle(this, null);
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
                TryGetMembers = (r, type, arg3) => throw error
            };

            using var _ = TestComponentSubscriber.Subscribe(component);
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
                TryGetMembers = (r, type, arg3) => throw error
            };

            using var _ = TestComponentSubscriber.Subscribe(component);
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
            var accessorInfo1 = new TestMemberAccessorInfo
            {
                GetValue = (o, context) => target1,
                TryObserve = (o, listener, arg3) =>
                {
                    currentListener = listener;
                    lastListener = listener;
                    return new ActionToken((o1, o2) => currentListener = null);
                }
            };
            var accessorInfo2 = new TestMemberAccessorInfo
            {
                GetValue = (o, context) => target2
            };
            var accessorInfo3 = new TestMemberAccessorInfo();
            var component = new TestMemberManagerComponent
            {
                TryGetMembers = (r, type, arg3) =>
                {
                    var request = (MemberManagerRequest)r;
                    if (request.Type == target2.GetType())
                        return accessorInfo3;
                    if (request.Type == root.GetType())
                        return accessorInfo1;
                    if (request.Type == target1.GetType())
                        return accessorInfo2;
                    throw new NotSupportedException();
                }
            };

            using var _ = TestComponentSubscriber.Subscribe(component);
            var observer = GetObserver(root, DefaultPath, MemberFlags.All, false, false);
            ObserverShouldManageListenerEvents(observer, ListenerMode.Members, count, () => lastListener?.TryHandle(this, null), disposed =>
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
                TryObserve = (o, listener, arg3) =>
                {
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
                    if (request.Type == target2.GetType())
                        return accessorInfo3;
                    if (request.Type == root.GetType())
                        return accessorInfo1;
                    if (request.Type == target1.GetType())
                        return accessorInfo2;
                    throw new NotSupportedException();
                }
            };

            using var _ = TestComponentSubscriber.Subscribe(component);
            var observer = GetObserver(root, DefaultPath, MemberFlags.All, false, false);
            ObserverShouldManageListenerEvents(observer, ListenerMode.LastMember, count, () => lastListener?.TryHandle(this, null), disposed => currentListener.ShouldBeNull(), ignoreFirstMember: false);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ObserverShouldNotifyListenersError(int count)
        {
            IEventListener? currentListener = null;
            var component = new TestMemberManagerComponent
            {
                TryGetMembers = (r, type, arg3) => default
            };
            using var _ = TestComponentSubscriber.Subscribe(component);

            var observer = GetObserver(this, DefaultPath, MemberFlags.All, false, false);
            ObserverShouldManageListenerEvents(observer, ListenerMode.Error, count, () => observer.GetMembers(), disposed => currentListener.ShouldBeNull());
        }

        protected abstract TObserver GetObserver(object target, IMemberPath path, MemberFlags memberFlags, bool hasStablePath, bool optional);

        #endregion
    }
}