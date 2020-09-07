using System;
using System.Collections.Generic;
using System.ComponentModel;
using MugenMvvm.Commands;
using MugenMvvm.Commands.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Internal;
using MugenMvvm.Messaging;
using MugenMvvm.Threading;
using MugenMvvm.UnitTest.Internal.Internal;
using MugenMvvm.UnitTest.Messaging.Internal;
using MugenMvvm.UnitTest.Models;
using MugenMvvm.UnitTest.Models.Internal;
using MugenMvvm.UnitTest.Threading.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Commands.Components
{
    public class ConditionEventCommandComponentTest : UnitTestBase
    {
        #region Methods

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void ShouldUseLocalThreadDispatcher(int mode)
        {
            var executionMode = ThreadExecutionMode.Parse(mode);
            Action? invoke = null;
            var threadDispatcher = new ThreadDispatcher();
            var threadDispatcherComponent = new TestThreadDispatcherComponent();
            threadDispatcher.AddComponent(threadDispatcherComponent);
            threadDispatcherComponent.Execute = (action, mode, arg3, _) =>
            {
                mode.ShouldEqual(executionMode);
                invoke = () => action(arg3);
                return true;
            };

            var compositeCommand = new CompositeCommand();
            var conditionEventCommandComponent = new ConditionEventCommandComponent(threadDispatcher, executionMode, Default.Array<object>(), null);
            compositeCommand.AddComponent(conditionEventCommandComponent);
            var executed = 0;
            EventHandler handler = (sender, args) => ++executed;
            conditionEventCommandComponent.AddCanExecuteChanged(compositeCommand, handler);

            executed.ShouldEqual(0);
            conditionEventCommandComponent.RaiseCanExecuteChanged();
            executed.ShouldEqual(0);
            invoke.ShouldNotBeNull();

            invoke!();
            executed.ShouldEqual(1);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void ShouldUseGlobalThreadDispatcher(int mode)
        {
            var executionMode = ThreadExecutionMode.Parse(mode);
            Action? invoke = null;
            var threadDispatcherComponent = new TestThreadDispatcherComponent();
            using var subscriber = TestComponentSubscriber.Subscribe(threadDispatcherComponent);
            threadDispatcherComponent.Execute = (action, mode, arg3, _) =>
            {
                mode.ShouldEqual(executionMode);
                invoke = () => action(arg3);
                return true;
            };

            var compositeCommand = new CompositeCommand();
            var conditionEventCommandComponent = new ConditionEventCommandComponent(null, executionMode, Default.Array<object>(), null);
            compositeCommand.AddComponent(conditionEventCommandComponent);
            var executed = 0;
            EventHandler handler = (sender, args) => ++executed;
            conditionEventCommandComponent.AddCanExecuteChanged(compositeCommand, handler);

            executed.ShouldEqual(0);
            conditionEventCommandComponent.RaiseCanExecuteChanged();
            executed.ShouldEqual(0);
            invoke.ShouldNotBeNull();

            invoke!();
            executed.ShouldEqual(1);
        }

        [Fact]
        public void ShouldSubscribeUnsubscribeRaiseEventHandler()
        {
            var compositeCommand = new CompositeCommand();
            var conditionEventCommandComponent = new ConditionEventCommandComponent(null, ThreadExecutionMode.Current, Default.Array<object>(), null);
            compositeCommand.AddComponent(conditionEventCommandComponent);
            var executed = 0;
            EventHandler handler = (sender, args) =>
            {
                sender.ShouldEqual(compositeCommand);
                ++executed;
            };
            conditionEventCommandComponent.AddCanExecuteChanged(compositeCommand, handler);

            executed.ShouldEqual(0);
            conditionEventCommandComponent.RaiseCanExecuteChanged();
            executed.ShouldEqual(1);

            conditionEventCommandComponent.RemoveCanExecuteChanged(compositeCommand, handler);
            conditionEventCommandComponent.RaiseCanExecuteChanged();
            executed.ShouldEqual(1);
        }

        [Fact]
        public void ShouldSuspendNotifications()
        {
            var compositeCommand = new CompositeCommand();
            var conditionEventCommandComponent = new ConditionEventCommandComponent(null, ThreadExecutionMode.Current, Default.Array<object>(), null);
            compositeCommand.AddComponent(conditionEventCommandComponent);
            var executed = 0;
            EventHandler handler = (sender, args) => ++executed;
            conditionEventCommandComponent.AddCanExecuteChanged(compositeCommand, handler);

            executed.ShouldEqual(0);
            conditionEventCommandComponent.RaiseCanExecuteChanged();
            executed.ShouldEqual(1);

            var actionToken1 = conditionEventCommandComponent.Suspend(this, DefaultMetadata);
            var actionToken2 = conditionEventCommandComponent.Suspend(this, DefaultMetadata);
            conditionEventCommandComponent.RaiseCanExecuteChanged();
            executed.ShouldEqual(1);

            actionToken1.Dispose();
            conditionEventCommandComponent.RaiseCanExecuteChanged();
            executed.ShouldEqual(1);

            actionToken2.Dispose();
            executed.ShouldEqual(2);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        public void ShouldListenPropertyChangedEvent(int listenersCount)
        {
            var models = new List<TestNotifyPropertyChangedModel>();
            for (var i = 0; i < listenersCount; i++)
                models.Add(new TestNotifyPropertyChangedModel());

            var compositeCommand = new CompositeCommand();
            var conditionEventCommandComponent = new ConditionEventCommandComponent(null, ThreadExecutionMode.Current, models, null);
            compositeCommand.AddComponent(conditionEventCommandComponent);
            var executed = 0;
            EventHandler handler = (sender, args) => ++executed;
            conditionEventCommandComponent.AddCanExecuteChanged(compositeCommand, handler);

            executed.ShouldEqual(0);
            foreach (var model in models)
                model.OnPropertyChanged("Test");
            executed.ShouldEqual(listenersCount);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(1, false)]
        [InlineData(5, true)]
        [InlineData(5, false)]
        public void ShouldSubscribeMessenger(int listenersCount, bool hasService)
        {
            var subscribedCount = 0;
            IMessengerHandlerRaw? handlerRaw = null;
            var list = new List<object>();
            for (var i = 0; i < listenersCount; i++)
            {
                var messenger = new Messenger();
                var component = new TestMessengerSubscriberComponent
                {
                    TrySubscribe = (o, arg3, arg4) =>
                    {
                        ++subscribedCount;
                        handlerRaw = (IMessengerHandlerRaw?) o;
                        return true;
                    }
                };
                messenger.AddComponent(component);
                list.Add(hasService ? (object) new TestHasServiceModel<IMessenger> {Service = messenger} : messenger);
            }

            var compositeCommand = new CompositeCommand();
            var conditionEventCommandComponent = new ConditionEventCommandComponent(null, ThreadExecutionMode.Current, list, null);
            compositeCommand.AddComponent(conditionEventCommandComponent);
            var executed = 0;
            EventHandler handler = (sender, args) => ++executed;
            conditionEventCommandComponent.AddCanExecuteChanged(compositeCommand, handler);

            subscribedCount.ShouldEqual(listenersCount);
            executed.ShouldEqual(0);
            handlerRaw!.ShouldNotBeNull();
            handlerRaw!.CanHandle(typeof(object)).ShouldBeTrue();
            handlerRaw!.Handle(new MessageContext(this, this, DefaultMetadata));
            executed.ShouldEqual(1);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldSubscribeMessengerCanNotify(bool hasService)
        {
            IMessengerHandlerRaw? handlerRaw = null;
            var list = new List<object>();
            var messenger = new Messenger();
            var component = new TestMessengerSubscriberComponent
            {
                TrySubscribe = (o, arg3, arg4) =>
                {
                    handlerRaw = (IMessengerHandlerRaw?) o;
                    return true;
                }
            };
            messenger.AddComponent(component);
            list.Add(hasService ? (object) new TestHasServiceModel<IMessenger> {Service = messenger} : messenger);

            var canNotifyValue = false;
            Func<object?, bool> canNotify = o =>
            {
                o.ShouldEqual(this);
                return canNotifyValue;
            };

            var compositeCommand = new CompositeCommand();
            var conditionEventCommandComponent = new ConditionEventCommandComponent(null, ThreadExecutionMode.Current, list, canNotify);
            compositeCommand.AddComponent(conditionEventCommandComponent);
            var executed = 0;
            EventHandler handler = (sender, args) => ++executed;
            conditionEventCommandComponent.AddCanExecuteChanged(compositeCommand, handler);

            executed.ShouldEqual(0);
            handlerRaw!.Handle(new MessageContext(this, this, DefaultMetadata));
            executed.ShouldEqual(0);

            canNotifyValue = true;
            handlerRaw.Handle(new MessageContext(this, this, DefaultMetadata));
            executed.ShouldEqual(1);
        }

        [Fact]
        public void ShouldListenPropertyChangedEventCanNotify()
        {
            var propertyChangedModel = new TestNotifyPropertyChangedModel();
            var canNotifyValue = false;
            var propertyName = "test";
            Func<object?, bool> canNotify = o =>
            {
                ((PropertyChangedEventArgs) o!).PropertyName.ShouldEqual(propertyName);
                return canNotifyValue;
            };
            var compositeCommand = new CompositeCommand();
            var conditionEventCommandComponent = new ConditionEventCommandComponent(null, ThreadExecutionMode.Current, new[] {propertyChangedModel}, canNotify);
            compositeCommand.AddComponent(conditionEventCommandComponent);
            var executed = 0;
            EventHandler handler = (sender, args) => ++executed;
            conditionEventCommandComponent.AddCanExecuteChanged(compositeCommand, handler);

            executed.ShouldEqual(0);
            propertyChangedModel.OnPropertyChanged(propertyName);
            executed.ShouldEqual(0);

            canNotifyValue = true;
            propertyChangedModel.OnPropertyChanged(propertyName);
            executed.ShouldEqual(1);
        }

        [Fact]
        public void DisposeShouldClearEventHandler()
        {
            var compositeCommand = new CompositeCommand();
            var conditionEventCommandComponent = new ConditionEventCommandComponent(null, ThreadExecutionMode.Current, Default.Array<object>(), null);
            compositeCommand.AddComponent(conditionEventCommandComponent);
            var executed = 0;
            EventHandler handler = (sender, args) =>
            {
                sender.ShouldEqual(compositeCommand);
                ++executed;
            };
            conditionEventCommandComponent.AddCanExecuteChanged(compositeCommand, handler);

            executed.ShouldEqual(0);
            conditionEventCommandComponent.Dispose();
            conditionEventCommandComponent.RaiseCanExecuteChanged();
            executed.ShouldEqual(0);
        }

#if !DEBUG
        [Fact]
        public void ShouldListenPropertyChangedWeak()
        {
            var propertyChangedModel = new TestNotifyPropertyChangedModel();
            var compositeCommand = new CompositeCommand();
            var conditionEventCommandComponent = new ConditionEventCommandComponent(null, ThreadExecutionMode.Current, new[] {propertyChangedModel}, null);
            compositeCommand.AddComponent(conditionEventCommandComponent);
            var executed = 0;
            EventHandler handler = (sender, args) => ++executed;
            conditionEventCommandComponent.AddCanExecuteChanged(compositeCommand, handler);

            executed.ShouldEqual(0);
            propertyChangedModel.OnPropertyChanged("test");
            executed.ShouldEqual(1);

            var reference = new WeakReference(conditionEventCommandComponent);
            compositeCommand = null;
            conditionEventCommandComponent = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            propertyChangedModel.OnPropertyChanged("test");
            reference.IsAlive.ShouldBeFalse();
        }
#endif

        #endregion
    }
}