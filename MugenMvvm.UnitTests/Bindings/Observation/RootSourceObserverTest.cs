﻿using System;
using MugenMvvm.Bindings.Interfaces.Convert;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Bindings.Members.Internal;
using MugenMvvm.UnitTests.Bindings.Observation.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Observation
{
    [Collection(SharedContext)]
    public class RootSourceObserverTest : UnitTestBase, IDisposable
    {
        public RootSourceObserverTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            MugenService.Configuration.InitializeInstance<IMemberManager>(new MemberManager(ComponentCollectionManager));
            MugenService.Configuration.InitializeInstance<IGlobalValueConverter>(GlobalValueConverter);
            MugenService.Configuration.InitializeInstance<IAttachedValueManager>(AttachedValueManager);
        }

        public void Dispose()
        {
            MugenService.Configuration.Clear<IMemberManager>();
            MugenService.Configuration.Clear<IGlobalValueConverter>();
            MugenService.Configuration.Clear<IAttachedValueManager>();
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
            MugenService.AddComponent(new TestMemberManagerComponent
            {
                TryGetMembers = (type, memberType, arg3, arg4, arg5) =>
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
            observer.Get(DefaultMetadata).ShouldEqual(target);
            observer.Get(target, DefaultMetadata).ShouldEqual(target);
            changedCount.ShouldEqual(0);

            canReturnParent = true;
            parentListener?.TryHandle(parent, this, DefaultMetadata);
            observer.Get(DefaultMetadata).ShouldEqual(parent);
            observer.Get(target, DefaultMetadata).ShouldEqual(parent);
            changedCount.ShouldEqual(1);

            canReturnParent = false;
            var parentObserver = RootSourceObserver.GetOrAdd(parent);
            parentObserver.Raise(parent, parentObserver, DefaultMetadata);
            canReturnParent = true;
            observer.Get(DefaultMetadata).ShouldEqual(parent);
            observer.Get(target, DefaultMetadata).ShouldEqual(parent);
            changedCount.ShouldEqual(2);

            canReturnParent = false;
            parentListener?.TryHandle(parent, this, DefaultMetadata);
            observer.Get(DefaultMetadata).ShouldEqual(target);
            observer.Get(target, DefaultMetadata).ShouldEqual(target);
            changedCount.ShouldEqual(3);

            parentObserver.Raise(parent, parentObserver, DefaultMetadata);
            changedCount.ShouldEqual(3);

            canReturnParent = true;
            parentListener?.TryHandle(parent, this, DefaultMetadata);
            observer.Get(DefaultMetadata).ShouldEqual(parent);
            observer.Get(target, DefaultMetadata).ShouldEqual(parent);
            changedCount.ShouldEqual(4);

            observer.Remove(listener);
            parentListener.ShouldBeNull();

            observer.Add(listener);
            parentListener.ShouldNotBeNull();

            RootSourceObserver.Clear(target);
            parentListener.ShouldBeNull();
            parentObserver.Raise(parent, this, DefaultMetadata);
            changedCount.ShouldEqual(4);
            observer.Get(DefaultMetadata).ShouldEqual(parent);
            observer.Get(target, DefaultMetadata).ShouldEqual(parent);
        }

        [Fact]
        public void ShouldReturnSelfNoParent()
        {
            var parentMember = new TestAccessorMemberInfo
            {
                GetValue = (o, context) => null,
                TryObserve = (o, listener, arg3) => default
            };
            MugenService.AddComponent(new TestMemberManagerComponent
            {
                TryGetMembers = (type, memberType, arg3, arg4, arg5) =>
                {
                    if (BindableMembers.For<object>().Parent().Name.Equals(arg4))
                        return parentMember;
                    return default;
                }
            });

            var target = new object();
            var observer = RootSourceObserver.GetOrAdd(target);
            observer.Get(DefaultMetadata).ShouldEqual(target);
            observer.Get(target, DefaultMetadata).ShouldEqual(target);
        }
    }
}