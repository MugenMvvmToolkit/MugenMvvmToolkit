using System;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Bindings.Observation.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Internal;
using MugenMvvm.Internal.Components;
using MugenMvvm.Tests.Bindings.Observation;
using MugenMvvm.Tests.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Observation.Components
{
    [Collection(SharedContext)]
    public class EventInfoObserverProviderTest : UnitTestBase
    {
        public EventInfoObserverProviderTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            ReflectionManager.AddComponent(new ExpressionReflectionDelegateProvider());
            ObservationManager.AddComponent(new EventInfoObserverProvider(AttachedValueManager, ReflectionManager));
            RegisterDisposeToken(WithGlobalService(WeakReferenceManager));
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

            var observer = ObservationManager.TryGetMemberObserver(typeof(TestEventClass), eventInfo!, DefaultMetadata);
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
        public void TryGetMemberObserverShouldObserveEventHandlerStatic()
        {
            var msg = new EventArgs();
            var listener = new TestWeakEventListener
            {
                IsAlive = true,
                TryHandle = (o, o1, m) =>
                {
                    o.ShouldEqual(null);
                    o1.ShouldEqual(msg);
                    return true;
                }
            };

            var eventInfo = typeof(TestEventClass).GetEvent(nameof(TestEventClass.EventHandlerStatic));
            eventInfo.ShouldNotBeNull();

            var observer = ObservationManager.TryGetMemberObserver(typeof(TestEventClass), eventInfo!, DefaultMetadata);
            observer.IsEmpty.ShouldBeFalse();

            var actionToken = observer.TryObserve(null, listener, DefaultMetadata);
            actionToken.IsEmpty.ShouldBeFalse();

            listener.InvokeCount.ShouldEqual(0);
            TestEventClass.OnEventHandlerStatic(msg);
            listener.InvokeCount.ShouldEqual(1);
            TestEventClass.OnEventHandlerStatic(msg);
            listener.InvokeCount.ShouldEqual(2);

            actionToken.Dispose();
            TestEventClass.OnEventHandlerStatic(msg);
            listener.InvokeCount.ShouldEqual(2);
        }

        [Fact]
        public void TryGetMemberObserverShouldObserveEventUsingReflectionDelegateProvider()
        {
            var testDelegateProvider = new TestReflectionDelegateProviderComponent();
            ReflectionManager.AddComponent(testDelegateProvider);

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

            var observer = ObservationManager.TryGetMemberObserver(typeof(TestEventClass), eventInfo!, DefaultMetadata);
            observer.IsEmpty.ShouldBeTrue();

            testDelegateProvider.CanCreateDelegate = (_, type, _) =>
            {
                type.ShouldEqual(typeof(Action));
                return true;
            };
            observer = ObservationManager.TryGetMemberObserver(typeof(TestEventClass), eventInfo!, DefaultMetadata);

            testDelegateProvider.TryCreateDelegate = (_, _, t, _) =>
            {
                var collection = (EventListenerCollection)t!;
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

        [Fact]
        public void TryGetMemberObserverShouldReturnEmptyUnsupportedRequest() =>
            ObservationManager.TryGetMemberObserver(typeof(object), this, DefaultMetadata).IsEmpty.ShouldBeTrue();

        protected override IObservationManager GetObservationManager() => new ObservationManager(ComponentCollectionManager);

        protected override IReflectionManager GetReflectionManager() => new ReflectionManager(ComponentCollectionManager);

        public sealed class TestEventClass
        {
            public static event EventHandler? EventHandlerStatic;

            public event EventHandler? EventHandler;

            public event Action? Action;

            public static void OnEventHandlerStatic(EventArgs args) => EventHandlerStatic?.Invoke(null, args);

            public void OnEventHandler(EventArgs args) => EventHandler?.Invoke(this, args);

            public void OnAction() => Action?.Invoke();
        }
    }
}