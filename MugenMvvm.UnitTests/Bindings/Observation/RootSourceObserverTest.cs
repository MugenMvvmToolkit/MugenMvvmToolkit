using System;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using MugenMvvm.Tests.Bindings.Members;
using MugenMvvm.Tests.Bindings.Observation;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Observation
{
    [Collection(SharedContext)]
    public class RootSourceObserverTest : UnitTestBase
    {
        public RootSourceObserverTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            RegisterDisposeToken(WithGlobalService(MemberManager));
            RegisterDisposeToken(WithGlobalService(GlobalValueConverter));
            RegisterDisposeToken(WithGlobalService(AttachedValueManager));
            RegisterDisposeToken(WithGlobalService(WeakReferenceManager));
        }

        [Fact]
        public void ShouldListenParent()
        {
            var parent = new object();
            var target = new object();
            IEventListener? parentListener = null;
            var canReturnParent = false;
            var parentMember = new TestAccessorMemberInfo
            {
                GetValue = (o, context) =>
                {
                    if (!canReturnParent)
                        return null;
                    if (o == target)
                        return parent;
                    return null;
                },
                TryObserve = (o, listener, arg3) =>
                {
                    if (o == target)
                    {
                        if (parentListener != null)
                            throw new NotSupportedException();
                        parentListener = listener;
                        return ActionToken.FromDelegate((o1, o2) => parentListener = null);
                    }

                    return default;
                }
            };
            MemberManager.AddComponent(new TestMemberManagerComponent
            {
                TryGetMembers = (_, type, memberType, arg3, arg4, arg5) =>
                {
                    if (BindableMembers.For<object>().Parent().Name.Equals(arg4))
                        return parentMember;
                    return default;
                }
            });

            var changedCount = 0;
            var observer = RootSourceObserver.GetOrAdd(target);
            var listener = new TestWeakEventListener
            {
                TryHandle = (o, o1, arg3) =>
                {
                    ++changedCount;
                    return true;
                }
            };
            observer.Add(listener);
            observer.Get(Metadata).ShouldEqual(target);
            observer.Get(target, Metadata).ShouldEqual(target);
            changedCount.ShouldEqual(0);

            canReturnParent = true;
            parentListener?.TryHandle(parent, this, Metadata);
            observer.Get(Metadata).ShouldEqual(parent);
            observer.Get(target, Metadata).ShouldEqual(parent);
            changedCount.ShouldEqual(1);

            canReturnParent = false;
            var parentObserver = RootSourceObserver.GetOrAdd(parent);
            parentObserver.Raise(parent, parentObserver, Metadata);
            canReturnParent = true;
            observer.Get(Metadata).ShouldEqual(parent);
            observer.Get(target, Metadata).ShouldEqual(parent);
            changedCount.ShouldEqual(2);

            canReturnParent = false;
            parentListener?.TryHandle(parent, this, Metadata);
            observer.Get(Metadata).ShouldEqual(target);
            observer.Get(target, Metadata).ShouldEqual(target);
            changedCount.ShouldEqual(3);

            parentObserver.Raise(parent, parentObserver, Metadata);
            changedCount.ShouldEqual(3);

            canReturnParent = true;
            parentListener?.TryHandle(parent, this, Metadata);
            observer.Get(Metadata).ShouldEqual(parent);
            observer.Get(target, Metadata).ShouldEqual(parent);
            changedCount.ShouldEqual(4);

            observer.Remove(listener);
            parentListener.ShouldBeNull();

            observer.Add(listener);
            parentListener.ShouldNotBeNull();

            RootSourceObserver.Clear(target);
            parentListener.ShouldBeNull();
            parentObserver.Raise(parent, this, Metadata);
            changedCount.ShouldEqual(4);
            observer.Get(Metadata).ShouldEqual(parent);
            observer.Get(target, Metadata).ShouldEqual(parent);
        }

        [Fact]
        public void ShouldReturnSelfNoParent()
        {
            var parentMember = new TestAccessorMemberInfo
            {
                GetValue = (o, context) => null,
                TryObserve = (o, listener, arg3) => default
            };
            MemberManager.AddComponent(new TestMemberManagerComponent
            {
                TryGetMembers = (_, type, memberType, arg3, arg4, arg5) =>
                {
                    if (BindableMembers.For<object>().Parent().Name.Equals(arg4))
                        return parentMember;
                    return default;
                }
            });

            var target = new object();
            var observer = RootSourceObserver.GetOrAdd(target);
            observer.Get(Metadata).ShouldEqual(target);
            observer.Get(target, Metadata).ShouldEqual(target);
        }

        protected override IMemberManager GetMemberManager() => new MemberManager(ComponentCollectionManager);
    }
}