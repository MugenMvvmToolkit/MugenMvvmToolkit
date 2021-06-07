using System.ComponentModel;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Bindings.Observation.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Tests.Bindings.Members;
using MugenMvvm.Tests.Bindings.Observation;
using MugenMvvm.Tests.Internal;
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
        }

        [Fact]
        public void TryGetMemberObserverShouldObservePropertyChanged1()
        {
            const string propertyName = nameof(TestNotifyPropertyChangedModel.Property);
            var target = new TestNotifyPropertyChangedModel { ThreadDispatcher = ThreadDispatcher };
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
            var observer = ObservationManager.TryGetMemberObserver(target.GetType(), member!, DefaultMetadata);
            observer.IsEmpty.ShouldBeFalse();

            var actionToken = observer.TryObserve(target, listener, DefaultMetadata);
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
            var target = new TestNotifyPropertyChangedModel { ThreadDispatcher = ThreadDispatcher };
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
            var observer = ObservationManager.TryGetMemberObserver(target.GetType(), member, DefaultMetadata);
            observer.IsEmpty.ShouldBeFalse();

            var actionToken = observer.TryObserve(target, listener, DefaultMetadata);
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
            var observer = ObservationManager.TryGetMemberObserver(target.GetType(), member, DefaultMetadata);
            observer.IsEmpty.ShouldBeFalse();

            var actionToken = observer.TryObserve(target, listener, DefaultMetadata);
            listener.InvokeCount.ShouldEqual(0);

            target.Value!.Raise(target, propertyName, propertyName, null);
            listener.InvokeCount.ShouldEqual(1);

            actionToken.Dispose();
            target.Value!.Raise(target, propertyName, propertyName, null);
            listener.InvokeCount.ShouldEqual(1);
        }

        [Fact]
        public void TryGetMemberObserverShouldReturnEmptyUnsupportedRequest() =>
            ObservationManager.TryGetMemberObserver(typeof(object), this, DefaultMetadata).IsEmpty.ShouldBeTrue();

        protected override IObservationManager GetObservationManager() => new ObservationManager(ComponentCollectionManager);
    }
}