﻿using System;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Binding.Observers.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using MugenMvvm.Internal.Components;
using MugenMvvm.UnitTest.Binding.Observers.Internal;
using MugenMvvm.UnitTest.Internal.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Observers.Components
{
    public class EventInfoMemberObserverProviderTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryGetMemberObserverShouldReturnEmptyUnsupportedRequest()
        {
            var component = new EventInfoMemberObserverProvider();
            component.TryGetMemberObserver(typeof(object), this, DefaultMetadata).IsEmpty.ShouldBeTrue();
        }

        [Fact]
        public void TryGetMemberObserverShouldObserveEventHandler()
        {
            var msg = new EventArgs();
            var target = new TestEventClass();
            var listener = new TestWeakEventListener
            {
                IsAlive = true,
                TryHandle = (o, o1, m) =>
                {
                    o.ShouldEqual(target);
                    o1.ShouldEqual(msg);
                    return true;
                }
            };

            var eventInfo = typeof(TestEventClass).GetEvent(nameof(TestEventClass.EventHandler));
            eventInfo.ShouldNotBeNull();
            var component = new EventInfoMemberObserverProvider();

            var observer = component.TryGetMemberObserver(typeof(TestEventClass), eventInfo, DefaultMetadata);
            observer.IsEmpty.ShouldBeFalse();

            var actionToken = observer.TryObserve(target, listener, DefaultMetadata);
            actionToken.IsEmpty.ShouldBeFalse();

            listener.InvokeCount.ShouldEqual(0);
            target.OnEventHandler(msg);
            listener.InvokeCount.ShouldEqual(1);
            target.OnEventHandler(msg);
            listener.InvokeCount.ShouldEqual(2);

            actionToken.Dispose();
            target.OnEventHandler(msg);
            listener.InvokeCount.ShouldEqual(2);
        }

        [Fact]
        public void TryGetMemberObserverShouldObserveEventUsingReflectionDelegateProvider()
        {
            var delegateProvider = new ReflectionDelegateProvider();
            var testDelegateProvider = new TestReflectionDelegateProviderComponent();
            delegateProvider.AddComponent(testDelegateProvider);
            delegateProvider.AddComponent(new ExpressionReflectionDelegateProvider());

            var msg = new EventArgs();
            var target = new TestEventClass();
            var listener = new TestWeakEventListener
            {
                IsAlive = true,
                TryHandle = (o, o1, m) =>
                {
                    o.ShouldEqual(target);
                    o1.ShouldEqual(msg);
                    m.ShouldEqual(DefaultMetadata);
                    return true;
                }
            };

            var eventInfo = typeof(TestEventClass).GetEvent(nameof(TestEventClass.Action));
            eventInfo.ShouldNotBeNull();
            var component = new EventInfoMemberObserverProvider(reflectionDelegateProvider: delegateProvider);

            var observer = component.TryGetMemberObserver(typeof(TestEventClass), eventInfo, DefaultMetadata);
            observer.IsEmpty.ShouldBeTrue();

            testDelegateProvider.CanCreateDelegate = (type, info) =>
            {
                type.ShouldEqual(typeof(Action));
                return true;
            };
            observer = component.TryGetMemberObserver(typeof(TestEventClass), eventInfo, DefaultMetadata);

            testDelegateProvider.TryCreateDelegate = (type, o, arg3) =>
            {
                var collection = (EventListenerCollection) o!;
                return new Action(() => collection.Raise(target, msg, DefaultMetadata));
            };

            var actionToken = observer.TryObserve(target, listener, DefaultMetadata);
            actionToken.IsEmpty.ShouldBeFalse();

            listener.InvokeCount.ShouldEqual(0);
            target.OnAction();
            listener.InvokeCount.ShouldEqual(1);
            target.OnAction();
            listener.InvokeCount.ShouldEqual(2);

            actionToken.Dispose();
            target.OnAction();
            listener.InvokeCount.ShouldEqual(2);
        }

        #endregion

        #region Nested types

        public sealed class TestEventClass
        {
            #region Events

            public event EventHandler? EventHandler;

            public event Action? Action;

            #endregion

            #region Methods

            public void OnEventHandler(EventArgs args)
            {
                EventHandler?.Invoke(this, args);
            }

            public void OnAction()
            {
                Action?.Invoke();
            }

            #endregion
        }

        #endregion
    }
}