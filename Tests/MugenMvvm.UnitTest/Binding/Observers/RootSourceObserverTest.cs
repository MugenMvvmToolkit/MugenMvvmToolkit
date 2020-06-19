using System;
using MugenMvvm.Binding;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Internal;
using MugenMvvm.UnitTest.Binding.Members.Internal;
using MugenMvvm.UnitTest.Binding.Observers.Internal;
using MugenMvvm.UnitTest.Internal.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Observers
{
    public class RootSourceObserverTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ShouldReturnSelfNoParent()
        {
            var target = new object();
            var observer = RootSourceObserver.GetOrAdd(target);
            observer.Get(DefaultMetadata).ShouldEqual(target);
            observer.Get(target, DefaultMetadata).ShouldEqual(target);
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
                        return new ActionToken((o1, o2) => parentListener = null);
                    }

                    return default;
                }
            };
            using var m = TestComponentSubscriber.Subscribe(new TestMemberManagerComponent
            {
                TryGetMembers = (type, memberType, arg3, arg4, arg5, arg6) =>
                {
                    if (BindableMembers.Object.Parent.Name.Equals(arg4))
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

        #endregion
    }
}