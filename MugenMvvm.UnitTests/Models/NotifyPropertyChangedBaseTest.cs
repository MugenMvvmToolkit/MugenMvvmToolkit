using System;
using System.ComponentModel;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Tests.Bindings.Observation;
using MugenMvvm.Tests.Threading;
using MugenMvvm.Threading;
using MugenMvvm.UnitTests.Models.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Models
{
    [Collection(SharedContext)]
    public class NotifyPropertyChangedBaseTest : UnitTestBase
    {
        private readonly ThreadDispatcher _threadDispatcher;
        private readonly TestNotifyPropertyChangedModel _model;

        public NotifyPropertyChangedBaseTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _threadDispatcher = new ThreadDispatcher(ComponentCollectionManager);
            _threadDispatcher.AddComponent(new TestThreadDispatcherComponent());
            _model = new TestNotifyPropertyChangedModel();
        }

        [Fact]
        public void ClearPropertyChangedSubscribersShouldRemoveHolderSubscribers()
        {
            var invokeCountEvent = 0;
            string propertyName = "Test";

            IValueHolder<MemberListenerCollection> holder = _model;
            holder.Value = new MemberListenerCollection();
            holder.Value.Add(new TestWeakEventListener
            {
                IsWeak = true,
                TryHandle = (sender, args, _) =>
                {
                    sender.ShouldEqual(_model);
                    ((PropertyChangedEventArgs)args!).PropertyName.ShouldEqual(propertyName);
                    ++invokeCountEvent;
                    return true;
                }
            }, propertyName);

            _model.OnPropertyChanged(propertyName);
            invokeCountEvent.ShouldEqual(1);

            _model.ClearPropertyChangedSubscribers();
            invokeCountEvent = 0;
            _model.OnPropertyChangedRaw(new PropertyChangedEventArgs(propertyName));
            invokeCountEvent.ShouldEqual(0);
            holder.Value.ShouldBeNull();
        }

        [Fact]
        public void ClearPropertyChangedSubscribersShouldRemoveSubscribers()
        {
            var invokeCountEvent = 0;
            string propertyName = "Test";

            _model.PropertyChanged += (sender, args) =>
            {
                sender.ShouldEqual(_model);
                args.PropertyName.ShouldEqual(propertyName);
                ++invokeCountEvent;
            };

            _model.OnPropertyChanged(propertyName);
            invokeCountEvent.ShouldEqual(1);

            _model.ClearPropertyChangedSubscribers();
            invokeCountEvent = 0;
            _model.OnPropertyChangedRaw(new PropertyChangedEventArgs(propertyName));
            invokeCountEvent.ShouldEqual(0);
        }

        [Fact]
        public void InvalidatePropertiesShouldRaiseEmptyMemberEvent()
        {
            var invokeCount = 0;
            var invokeCountEvent = 0;
            string propertyName = "";

            _model.OnPropertyChangedInternalHandler = args =>
            {
                args.PropertyName.ShouldEqual(propertyName);
                ++invokeCount;
            };
            _model.PropertyChanged += (sender, args) =>
            {
                sender.ShouldEqual(_model);
                args.PropertyName.ShouldEqual(propertyName);
                ++invokeCountEvent;
            };

            _model.InvalidateProperties();
            invokeCount.ShouldEqual(1);
            invokeCountEvent.ShouldEqual(1);
        }

        [Fact]
        public void OnPropertyChangedShouldRaiseVirtualMethod()
        {
            var invokeCount = 0;
            var invokeCountEvent = 0;
            string propertyName = "Test";

            _model.OnPropertyChangedInternalHandler = args =>
            {
                args.PropertyName.ShouldEqual(propertyName);
                ++invokeCount;
            };
            _model.PropertyChanged += (sender, args) =>
            {
                sender.ShouldEqual(_model);
                args.PropertyName.ShouldEqual(propertyName);
                ++invokeCountEvent;
            };

            _model.OnPropertyChanged(propertyName);
            invokeCount.ShouldEqual(1);
            invokeCountEvent.ShouldEqual(1);

            _model.OnPropertyChangedRaw(new PropertyChangedEventArgs(propertyName));
            invokeCount.ShouldEqual(2);
            invokeCountEvent.ShouldEqual(2);
        }

        [Fact]
        public void SuspendShouldRaiseEmptyMemberEvent()
        {
            var invokeCount = 0;
            var invokeCountEvent = 0;
            var endSuspendCount = 0;
            var endSuspendDirtyCount = 0;
            string propertyName = "";

            _model.OnPropertyChangedInternalHandler = args =>
            {
                args.PropertyName.ShouldEqual(propertyName);
                ++invokeCount;
            };
            _model.PropertyChanged += (sender, args) =>
            {
                sender.ShouldEqual(_model);
                args.PropertyName.ShouldEqual(propertyName);
                ++invokeCountEvent;
            };
            _model.OnEndSuspendHandler = b =>
            {
                if (b)
                    ++endSuspendDirtyCount;
                else
                    ++endSuspendCount;
            };

            _model.Suspend().Dispose();
            endSuspendCount.ShouldEqual(1);
            endSuspendDirtyCount.ShouldEqual(0);
            invokeCount.ShouldEqual(0);
            invokeCountEvent.ShouldEqual(0);

            var token = _model.Suspend();
            _model.OnPropertyChanged(nameof(_model.Property));
            invokeCount.ShouldEqual(0);
            invokeCountEvent.ShouldEqual(0);
            token.Dispose();
            invokeCount.ShouldEqual(1);
            invokeCountEvent.ShouldEqual(1);
            endSuspendCount.ShouldEqual(1);
            endSuspendDirtyCount.ShouldEqual(1);
        }
    }
}