using System;
using System.ComponentModel;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Bindings.Observation.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Tests.Bindings.Members;
using MugenMvvm.Tests.Bindings.Observation;
using MugenMvvm.Tests.Internal;
using MugenMvvm.Tests.Threading;
using MugenMvvm.UnitTests.Models.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Observation.Components
{
    [Collection(SharedContext)]
    public class PropertyChangedObserverProviderTest : UnitTestBase
    {
        public PropertyChangedObserverProviderTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            ObservationManager.AddComponent(new PropertyChangedObserverProvider(AttachedValueManager));
            RegisterDisposeToken(WithGlobalService(WeakReferenceManager));
            RegisterDisposeToken(WithGlobalService(ThreadDispatcher));
        }

        [Fact]
        public void TryGetMemberObserverShouldObservePropertyChanged1()
        {
            const string propertyName = nameof(TestNotifyPropertyChangedModel.Property);
            var target = new TestNotifyPropertyChangedModel();
            var listener = new TestWeakEventListener
            {
                IsAlive = true,
                TryHandle = (o, o1, m) =>
                {
                    o.ShouldEqual(target);
                    ((PropertyChangedEventArgs)o1!).PropertyName.ShouldEqual(propertyName);
                    return true;
                }
            };

            var member = target.GetType().GetProperty(propertyName);
            var observer = ObservationManager.TryGetMemberObserver(target.GetType(), member!, Metadata);
            observer.IsEmpty.ShouldBeFalse();

            var actionToken = observer.TryObserve(target, listener, Metadata);
            listener.InvokeCount.ShouldEqual(0);

            target.OnPropertyChanged(propertyName);
            listener.InvokeCount.ShouldEqual(1);

            actionToken.Dispose();
            target.OnPropertyChanged(propertyName);
            listener.InvokeCount.ShouldEqual(1);
        }

        [Fact]
        public void TryGetMemberObserverShouldObservePropertyChanged2()
        {
            const string propertyName = "Test";
            var target = new TestNotifyPropertyChangedModel();
            var listener = new TestWeakEventListener
            {
                IsAlive = true,
                TryHandle = (o, o1, m) =>
                {
                    o.ShouldEqual(target);
                    ((PropertyChangedEventArgs)o1!).PropertyName.ShouldEqual(propertyName);
                    return true;
                }
            };

            var member = new TestAccessorMemberInfo { Name = propertyName };
            var observer = ObservationManager.TryGetMemberObserver(target.GetType(), member, Metadata);
            observer.IsEmpty.ShouldBeFalse();

            var actionToken = observer.TryObserve(target, listener, Metadata);
            listener.InvokeCount.ShouldEqual(0);

            target.OnPropertyChanged(propertyName);
            listener.InvokeCount.ShouldEqual(1);

            actionToken.Dispose();
            target.OnPropertyChanged(propertyName);
            listener.InvokeCount.ShouldEqual(1);
        }

        [Fact]
        public void TryGetMemberObserverShouldObserveValueHolder()
        {
            const string propertyName = "Test";
            var target = new TestValueHolder<MemberListenerCollection>();
            var listener = new TestWeakEventListener
            {
                IsAlive = true,
                TryHandle = (o, o1, m) =>
                {
                    o.ShouldEqual(target);
                    o1.ShouldEqual(propertyName);
                    return true;
                }
            };

            var member = new TestAccessorMemberInfo { Name = propertyName };
            var observer = ObservationManager.TryGetMemberObserver(target.GetType(), member, Metadata);
            observer.IsEmpty.ShouldBeFalse();

            var actionToken = observer.TryObserve(target, listener, Metadata);
            listener.InvokeCount.ShouldEqual(0);

            target.Value!.Raise(target, propertyName, propertyName, null);
            listener.InvokeCount.ShouldEqual(1);

            actionToken.Dispose();
            target.Value!.Raise(target, propertyName, propertyName, null);
            listener.InvokeCount.ShouldEqual(1);
        }

        [Fact]
        public void TryGetMemberObserverShouldRaiseOnMainThread()
        {
            const string propertyName = nameof(TestNotifyPropertyChangedModel.Property);
            var target = new TestNotifyPropertyChangedModel();
            var listener = new TestWeakEventListener
            {
                IsAlive = true,
                TryHandle = (o, o1, m) =>
                {
                    o.ShouldEqual(target);
                    ((PropertyChangedEventArgs)o1!).PropertyName.ShouldEqual(propertyName);
                    return true;
                }
            };

            Action? invokeAction = null;
            ThreadDispatcher.ClearComponents();
            ThreadDispatcher.AddComponent(new TestThreadDispatcherComponent
            {
                CanExecuteInline = (_, mode, _) => mode != ThreadExecutionMode.Main,
                Execute = (_, action, m, s, _) =>
                {
                    m.ShouldEqual(ThreadExecutionMode.Main);
                    invokeAction = () => action(s);
                    return true;
                }
            });

            var member = target.GetType().GetProperty(propertyName);
            var observer = ObservationManager.TryGetMemberObserver(target.GetType(), member!, Metadata);
            observer.IsEmpty.ShouldBeFalse();

            var actionToken = observer.TryObserve(target, listener, Metadata);
            actionToken.IsEmpty.ShouldBeFalse();
            listener.InvokeCount.ShouldEqual(0);

            target.OnPropertyChanged(propertyName);
            listener.InvokeCount.ShouldEqual(0);

            invokeAction!();
            listener.InvokeCount.ShouldEqual(1);
        }

        [Fact]
        public void TryGetMemberObserverShouldReturnEmptyUnsupportedRequest() =>
            ObservationManager.TryGetMemberObserver(typeof(object), this, Metadata).IsEmpty.ShouldBeTrue();

        protected override IObservationManager GetObservationManager() => new ObservationManager(ComponentCollectionManager);
    }
}