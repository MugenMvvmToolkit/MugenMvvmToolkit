using System;
using System.Linq;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.UnitTest.Binding.Observers.Internal;
using MugenMvvm.UnitTest.Internal.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Observers.PathObservers
{
    public abstract class ObserverBaseTest<TObserver> : UnitTestBase where TObserver : IMemberPathObserver
    {
        #region Fields

        protected const string MethodName = "MM";

        #endregion

        #region Methods

        [Fact]
        public void ConstructorShouldInitializeValues1()
        {
            var o = new object();
            var observer = GetObserver(o);
            observer.IsAlive.ShouldBeTrue();
            observer.Target.ShouldEqual(o);
        }

        [Fact]
        public void ConstructorShouldInitializeValues2()
        {
            var o = new TestWeakReference { IsAlive = true, Target = new object() };
            var observer = GetObserver(o);
            observer.IsAlive.ShouldBeTrue();
            observer.Target.ShouldEqual(o.Target);

            o.Target = null;
            o.IsAlive = false;
            observer.IsAlive.ShouldBeFalse();
            observer.Target.ShouldBeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void DisposeShouldClearObserver(bool canDispose)
        {
            var memberPathObserver = GetObserver(this);
            memberPathObserver.IsAlive.ShouldBeTrue();
            memberPathObserver.Target.ShouldEqual(this);
            ((IHasDisposeCondition)memberPathObserver).CanDispose = canDispose;

            memberPathObserver.Dispose();
            if (canDispose)
            {
                memberPathObserver.GetLastMember(DefaultMetadata).IsAvailable.ShouldBeFalse();
                memberPathObserver.GetMembers(DefaultMetadata).IsAvailable.ShouldBeFalse();
                memberPathObserver.IsAlive.ShouldBeFalse();
                memberPathObserver.Target.ShouldBeNull();
            }
            else
            {
                memberPathObserver.IsAlive.ShouldBeTrue();
                memberPathObserver.Target.ShouldEqual(this);
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(150)]
        [InlineData(200)]
        [InlineData(500)]
        public void ObserverShouldManageListeners(int count)
        {
            var listeners = new TestMemberPathObserverListener[count];
            for (var i = 0; i < listeners.Length; i++)
            {
                var index = i;
                listeners[index] = new TestMemberPathObserverListener();
            }

            var observer = GetObserver(this);
            for (var i = 0; i < count; i++)
            {
                observer.AddListener(listeners[i]);
                observer.GetListeners().AsList().SequenceEqual(listeners.Take(i + 1)).ShouldBeTrue();
            }

            var removeCount = Math.Min(count, 100);
            for (var index = 0; index < removeCount; index++)
            {
                observer.RemoveListener(listeners[index]);
                observer.GetListeners().AsList().SequenceEqual(listeners.Skip(index + 1)).ShouldBeTrue();
            }

            for (var i = 0; i < removeCount; i++)
            {
                observer.AddListener(listeners[i]);
                observer.GetListeners().AsList().SequenceEqual(listeners.Skip(removeCount).Concat(listeners.Take(i + 1))).ShouldBeTrue();
            }

            observer.Dispose();
            observer.GetListeners().IsNullOrEmpty().ShouldBeTrue();

            for (var i = 0; i < removeCount; i++)
                observer.AddListener(listeners[i]);
            observer.GetListeners().IsNullOrEmpty().ShouldBeTrue();
        }

        protected void ObserverShouldManageListenerEvents(TObserver observer, ListenerMode mode, int count, Action raiseEvent, Action<bool> onCleared, int validationCount = 1, bool ignoreFirstMember = true)
        {
            var listeners = new TestMemberPathObserverListener[count];
            for (var i = 0; i < listeners.Length; i++)
            {
                var index = i;
                listeners[index] = new TestMemberPathObserverListener
                {
                    OnPathMembersChanged = pathObserver => { pathObserver.ShouldEqual(observer); },
                    OnLastMemberChanged = pathObserver =>
                    {
                        pathObserver.ShouldEqual(observer);
                        if (mode == ListenerMode.Members)
                            throw new NotSupportedException();
                    },
                    OnError = (pathObserver, exception) =>
                    {
                        if (mode != ListenerMode.Error)
                            pathObserver.ShouldEqual(observer);
                        exception.ShouldNotBeNull();
                    }
                };
            }

            for (var i = 0; i < count; i++)
            {
                observer.AddListener(listeners[i]);
                if (!ignoreFirstMember || i != 0)
                    raiseEvent();
                ValidateInvokeCount(listeners, mode, validationCount, true, 0, i + 1);
                ValidateInvokeCount(listeners, mode, 0, true, i + 1);
            }

            for (var i = 0; i < count; i++)
            {
                observer.RemoveListener(listeners[i]);
                raiseEvent();
                ValidateInvokeCount(listeners, mode, validationCount, true, i + 1);
                ValidateInvokeCount(listeners, mode, 0, true, 0, i + 1);
            }

            onCleared(false);
            raiseEvent();
            ValidateInvokeCount(listeners, mode, 0, true, 0, count);

            for (var i = 0; i < count; i++)
            {
                observer.AddListener(listeners[i]);
                if (i != 0 || mode != ListenerMode.Error)
                    raiseEvent();
                ValidateInvokeCount(listeners, mode, validationCount, true, 0, i + 1);
                ValidateInvokeCount(listeners, mode, 0, true, i + 1);
            }

            observer.Dispose();
            onCleared(true);
            ValidateInvokeCount(listeners, mode, 0, true, 0, count);
        }

        private static void ValidateInvokeCount(TestMemberPathObserverListener[] listeners, ListenerMode mode, int count, bool clear = true, int? start = null, int? end = null)
        {
            for (var i = start.GetValueOrDefault(); i < end.GetValueOrDefault(listeners.Length); i++)
            {
                switch (mode)
                {
                    case ListenerMode.Error:
                        listeners[i].ErrorCount.ShouldEqual(count);
                        if (clear)
                            listeners[i].ErrorCount = 0;
                        break;
                    case ListenerMode.LastMember:
                        listeners[i].LastMemberChangedCount.ShouldEqual(count);
                        if (clear)
                            listeners[i].LastMemberChangedCount = 0;
                        break;
                    case ListenerMode.Members:
                        listeners[i].PathMembersChangedCount.ShouldEqual(count);
                        if (clear)
                            listeners[i].PathMembersChangedCount = 0;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
                }
            }
        }

        protected abstract TObserver GetObserver(object target);

        #endregion

        #region Nested types

        protected enum ListenerMode
        {
            Error,
            LastMember,
            Members
        }

        #endregion
    }
}