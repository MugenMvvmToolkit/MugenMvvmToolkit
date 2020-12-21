using System;
using System.ComponentModel;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.UnitTests.Bindings.Observation.Internal;
using MugenMvvm.UnitTests.Models.Internal;
using MugenMvvm.UnitTests.Threading.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Models
{
    public class NotifyPropertyChangedBaseTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ClearPropertyChangedSubscribersShouldRemoveSubscribers()
        {
            var invokeCountEvent = 0;
            string propertyName = "Test";
            var model = new TestNotifyPropertyChangedModel();
            model.PropertyChanged += (sender, args) =>
            {
                sender.ShouldEqual(model);
                args.PropertyName.ShouldEqual(propertyName);
                ++invokeCountEvent;
            };

            model.OnPropertyChanged(propertyName);
            invokeCountEvent.ShouldEqual(1);

            model.ClearPropertyChangedSubscribers();
            invokeCountEvent = 0;
            model.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
            invokeCountEvent.ShouldEqual(0);
        }

        [Fact]
        public void ClearPropertyChangedSubscribersShouldRemoveHolderSubscribers()
        {
            var invokeCountEvent = 0;
            string propertyName = "Test";
            var model = new TestNotifyPropertyChangedModel();
            IValueHolder<MemberListenerCollection> holder = model;
            holder.Value = new MemberListenerCollection();
            holder.Value.Add(new TestWeakEventListener
            {
                TryHandle = (sender, args, _) =>
                {
                    sender.ShouldEqual(model);
                    ((PropertyChangedEventArgs) args!).PropertyName.ShouldEqual(propertyName);
                    ++invokeCountEvent;
                    return true;
                }
            }, propertyName);

            model.OnPropertyChanged(propertyName);
            invokeCountEvent.ShouldEqual(1);

            model.ClearPropertyChangedSubscribers();
            invokeCountEvent = 0;
            model.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
            invokeCountEvent.ShouldEqual(0);
            holder.Value.ShouldBeNull();
        }

        [Fact]
        public void SuspendShouldRaiseEmptyMemberEvent()
        {
            var invokeCount = 0;
            var invokeCountEvent = 0;
            var endSuspendCount = 0;
            var endSuspendDirtyCount = 0;
            string propertyName = "";
            var model = new TestNotifyPropertyChangedModel
            {
                OnPropertyChangedInternalHandler = args =>
                {
                    args.PropertyName.ShouldEqual(propertyName);
                    ++invokeCount;
                }
            };
            model.PropertyChanged += (sender, args) =>
            {
                sender.ShouldEqual(model);
                args.PropertyName.ShouldEqual(propertyName);
                ++invokeCountEvent;
            };
            model.OnEndSuspendHandler = b =>
            {
                if (b)
                    ++endSuspendDirtyCount;
                else
                    ++endSuspendCount;
            };

            model.Suspend().Dispose();
            endSuspendCount.ShouldEqual(1);
            endSuspendDirtyCount.ShouldEqual(0);
            invokeCount.ShouldEqual(0);
            invokeCountEvent.ShouldEqual(0);

            var token = model.Suspend();
            model.OnPropertyChanged(nameof(model.Property));
            invokeCount.ShouldEqual(0);
            invokeCountEvent.ShouldEqual(0);
            token.Dispose();
            invokeCount.ShouldEqual(1);
            invokeCountEvent.ShouldEqual(1);
            endSuspendCount.ShouldEqual(1);
            endSuspendDirtyCount.ShouldEqual(1);
        }

        [Fact]
        public void InvalidatePropertiesShouldRaiseEmptyMemberEvent()
        {
            var invokeCount = 0;
            var invokeCountEvent = 0;
            string propertyName = "";
            var model = new TestNotifyPropertyChangedModel
            {
                OnPropertyChangedInternalHandler = args =>
                {
                    args.PropertyName.ShouldEqual(propertyName);
                    ++invokeCount;
                }
            };
            model.PropertyChanged += (sender, args) =>
            {
                sender.ShouldEqual(model);
                args.PropertyName.ShouldEqual(propertyName);
                ++invokeCountEvent;
            };

            model.InvalidateProperties();
            invokeCount.ShouldEqual(1);
            invokeCountEvent.ShouldEqual(1);
        }

        [Fact]
        public void OnPropertyChangedShouldRaiseHolder()
        {
            var invokeCount = 0;
            var invokeCountEvent = 0;
            string propertyName = "Test";
            var model = new TestNotifyPropertyChangedModel
            {
                OnPropertyChangedInternalHandler = args =>
                {
                    args.PropertyName.ShouldEqual(propertyName);
                    ++invokeCount;
                }
            };
            model.PropertyChanged += (sender, args) =>
            {
                sender.ShouldEqual(model);
                args.PropertyName.ShouldEqual(propertyName);
                ++invokeCountEvent;
            };

            model.OnPropertyChanged(propertyName);
            invokeCount.ShouldEqual(1);
            invokeCountEvent.ShouldEqual(1);

            model.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
            invokeCount.ShouldEqual(2);
            invokeCountEvent.ShouldEqual(2);
        }

        [Fact]
        public void OnPropertyChangedShouldRaiseEventUsingThreadDispatcher()
        {
            Action<object?>? invokeAction = null;
            object? state = null;
            using var t = MugenService.AddComponent(new TestThreadDispatcherComponent
            {
                CanExecuteInline = (mode, context) => false,
                Execute = (action, mode, arg3, arg4) =>
                {
                    invokeAction = action;
                    state = arg3;
                    mode.ShouldEqual(ThreadExecutionMode.Main);
                    return true;
                }
            });

            string propertyName = "Test";
            var invokeCount = 0;
            var invokeCountEvent = 0;
            var model = new TestNotifyPropertyChangedModel
            {
                OnPropertyChangedInternalHandler = args =>
                {
                    args.PropertyName.ShouldEqual(propertyName);
                    ++invokeCount;
                }
            };
            model.PropertyChanged += (sender, args) =>
            {
                sender.ShouldEqual(model);
                args.PropertyName.ShouldEqual(propertyName);
                ++invokeCountEvent;
            };

            model.OnPropertyChanged(propertyName);
            invokeCount.ShouldEqual(0);
            invokeCountEvent.ShouldEqual(0);

            invokeAction!(state);
            invokeCount.ShouldEqual(1);
            invokeCountEvent.ShouldEqual(1);

            model.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
            invokeCount.ShouldEqual(1);
            invokeCountEvent.ShouldEqual(1);

            invokeAction!(state);
            invokeCount.ShouldEqual(2);
            invokeCountEvent.ShouldEqual(2);
        }

        #endregion
    }
}