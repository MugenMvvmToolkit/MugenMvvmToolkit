using System.ComponentModel;
using MugenMvvm.Binding.Observers.Components;
using MugenMvvm.UnitTest.Binding.Members.Internal;
using MugenMvvm.UnitTest.Binding.Observers.Internal;
using MugenMvvm.UnitTest.Models;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Observers.Components
{
    public class PropertyChangedMemberObserverProviderTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryGetMemberObserverShouldReturnEmptyUnsupportedRequest()
        {
            var component = new PropertyChangedMemberObserverProvider();
            component.TryGetMemberObserver(typeof(object), this, DefaultMetadata).IsEmpty.ShouldBeTrue();
        }

        [Fact]
        public void TryGetMemberObserverShouldObservePropertyChanged1()
        {
            const string propertyName = nameof(TestNotifyPropertyChangedModel.Property);
            var target = new TestNotifyPropertyChangedModel();
            var component = new PropertyChangedMemberObserverProvider();
            var listener = new TestWeakEventListener
            {
                IsAlive = true,
                TryHandle = (o, o1, m) =>
                {
                    o.ShouldEqual(target);
                    ((PropertyChangedEventArgs) o1!).PropertyName.ShouldEqual(propertyName);
                    return true;
                }
            };

            var member = target.GetType().GetProperty(propertyName);
            var observer = component.TryGetMemberObserver(target.GetType(), member, DefaultMetadata);
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
            var target = new TestNotifyPropertyChangedModel();
            var component = new PropertyChangedMemberObserverProvider();
            var listener = new TestWeakEventListener
            {
                IsAlive = true,
                TryHandle = (o, o1, m) =>
                {
                    o.ShouldEqual(target);
                    ((PropertyChangedEventArgs) o1!).PropertyName.ShouldEqual(propertyName);
                    return true;
                }
            };

            var member = new TestAccessorMemberInfo {Name = propertyName};
            var observer = component.TryGetMemberObserver(target.GetType(), member, DefaultMetadata);
            observer.IsEmpty.ShouldBeFalse();

            var actionToken = observer.TryObserve(target, listener, DefaultMetadata);
            listener.InvokeCount.ShouldEqual(0);

            target.OnPropertyChanged(propertyName);
            listener.InvokeCount.ShouldEqual(1);

            actionToken.Dispose();
            target.OnPropertyChanged(propertyName);
            listener.InvokeCount.ShouldEqual(1);
        }

        #endregion
    }
}