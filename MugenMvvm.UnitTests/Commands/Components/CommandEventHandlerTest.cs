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
using MugenMvvm.UnitTests.Internal.Internal;
using MugenMvvm.UnitTests.Messaging.Internal;
using MugenMvvm.UnitTests.Models.Internal;
using MugenMvvm.UnitTests.Threading.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Commands.Components
{
    public class CommandEventHandlerTest : UnitTestBase
    {
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void ShouldUseLocalThreadDispatcher(int mode)
        {
            var executionMode = ThreadExecutionMode.Get(mode);
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
            var conditionEventCommandComponent = new CommandEventHandler(threadDispatcher, executionMode);
            compositeCommand.AddComponent(conditionEventCommandComponent);
            var executed = 0;
            EventHandler handler = (sender, args) => ++executed;
            conditionEventCommandComponent.AddCanExecuteChanged(compositeCommand, handler, null);

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
            var executionMode = ThreadExecutionMode.Get(mode);
            Action? invoke = null;
            var threadDispatcherComponent = new TestThreadDispatcherComponent();
            using var t = MugenService.AddComponent(threadDispatcherComponent);
            threadDispatcherComponent.Execute = (action, mode, arg3, _) =>
            {
                mode.ShouldEqual(executionMode);
                invoke = () => action(arg3);
                return true;
            };

            var compositeCommand = new CompositeCommand();
            var conditionEventCommandComponent = new CommandEventHandler(null, executionMode);
            compositeCommand.AddComponent(conditionEventCommandComponent);
            var executed = 0;
            EventHandler handler = (sender, args) => ++executed;
            conditionEventCommandComponent.AddCanExecuteChanged(compositeCommand, handler, null);

            executed.ShouldEqual(0);
            conditionEventCommandComponent.RaiseCanExecuteChanged();
            executed.ShouldEqual(0);
            invoke.ShouldNotBeNull();

            invoke!();
            executed.ShouldEqual(1);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        public void ShouldListenPropertyChangedEvent(int listenersCount)
        {
            var compositeCommand = new CompositeCommand();
            var conditionEventCommandComponent = new CommandEventHandler(null, ThreadExecutionMode.Current);
            compositeCommand.AddComponent(conditionEventCommandComponent);

            var models = new List<TestNotifyPropertyChangedModel>();
            var tokens = new List<ActionToken>();
            for (var i = 0; i < listenersCount; i++)
            {
                var notifier = new TestNotifyPropertyChangedModel();
                models.Add(notifier);
                var token = compositeCommand.AddNotifier(notifier);
                token.IsEmpty.ShouldBeFalse();
                tokens.Add(token);
            }

            var executed = 0;
            EventHandler handler = (_, _) => ++executed;
            conditionEventCommandComponent.AddCanExecuteChanged(compositeCommand, handler, null);

            executed.ShouldEqual(0);
            foreach (var model in models)
                model.OnPropertyChanged("Test");
            executed.ShouldEqual(listenersCount);

            foreach (var token in tokens)
                token.Dispose();
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
            var compositeCommand = new CompositeCommand();
            var conditionEventCommandComponent = new CommandEventHandler(null, ThreadExecutionMode.Current);
            compositeCommand.AddComponent(conditionEventCommandComponent);
            IMessengerHandler messengerHandler = conditionEventCommandComponent;

            var tokens = new List<ActionToken>();
            for (var i = 0; i < listenersCount; i++)
            {
                var messenger = new Messenger();
                var component = new TestMessengerSubscriberComponent
                {
                    TrySubscribe = (o, _, _) =>
                    {
                        ++subscribedCount;
                        o.ShouldEqual(conditionEventCommandComponent);
                        return true;
                    },
                    TryUnsubscribe = (o, _) =>
                    {
                        --subscribedCount;
                        o.ShouldEqual(conditionEventCommandComponent);
                        return true;
                    }
                };
                messenger.AddComponent(component);

                var notifier = hasService ? (object) new TestHasServiceModel<IMessenger> {Service = messenger} : messenger;
                var token = compositeCommand.AddNotifier(notifier);
                token.IsEmpty.ShouldBeFalse();
                tokens.Add(token);
            }

            var executed = 0;
            EventHandler handler = (_, _) => ++executed;
            conditionEventCommandComponent.AddCanExecuteChanged(compositeCommand, handler, null);

            subscribedCount.ShouldEqual(listenersCount);
            executed.ShouldEqual(0);
            messengerHandler.ShouldNotBeNull();
            messengerHandler.CanHandle(typeof(object)).ShouldBeTrue();
            messengerHandler.Handle(new MessageContext(this, this, DefaultMetadata));
            executed.ShouldEqual(1);

            foreach (var token in tokens)
                token.Dispose();
            subscribedCount.ShouldEqual(0);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldSubscribeMessengerCanNotify(bool hasService)
        {
            IMessengerHandler? handlerRaw = null;
            var messenger = new Messenger();
            var component = new TestMessengerSubscriberComponent
            {
                TrySubscribe = (o, _, _) =>
                {
                    handlerRaw = (IMessengerHandler?) o;
                    return true;
                }
            };
            messenger.AddComponent(component);

            var canNotifyValue = false;
            Func<object?, object?, bool> canNotify = (s, o) =>
            {
                s.ShouldEqual(this);
                o.ShouldEqual(messenger);
                return canNotifyValue;
            };

            var compositeCommand = new CompositeCommand();
            var conditionEventCommandComponent = new CommandEventHandler(null, ThreadExecutionMode.Current) {CanNotify = canNotify};
            compositeCommand.AddComponent(conditionEventCommandComponent);
            compositeCommand.AddNotifier(hasService ? (object) new TestHasServiceModel<IMessenger> {Service = messenger} : messenger);

            var executed = 0;
            EventHandler handler = (_, _) => ++executed;
            conditionEventCommandComponent.AddCanExecuteChanged(compositeCommand, handler, null);

            executed.ShouldEqual(0);
            handlerRaw!.Handle(new MessageContext(this, messenger, DefaultMetadata));
            executed.ShouldEqual(0);

            canNotifyValue = true;
            handlerRaw.Handle(new MessageContext(this, messenger, DefaultMetadata));
            executed.ShouldEqual(1);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void DisposeShouldClearEventHandler(bool canDispose)
        {
            var compositeCommand = new CompositeCommand();
            var conditionEventCommandComponent = new CommandEventHandler(null, ThreadExecutionMode.Current);
            conditionEventCommandComponent.IsDisposable.ShouldBeTrue();
            conditionEventCommandComponent.IsDisposable = canDispose;
            compositeCommand.AddComponent(conditionEventCommandComponent);
            var executed = 0;
            EventHandler handler = (_, _) => ++executed;
            conditionEventCommandComponent.AddCanExecuteChanged(compositeCommand, handler, null);

            executed.ShouldEqual(0);
            conditionEventCommandComponent.Dispose();
            conditionEventCommandComponent.RaiseCanExecuteChanged();
            executed.ShouldEqual(canDispose ? 0 : 1);
        }

        [Fact]
        public void ShouldListenPropertyChangedEventCanNotify()
        {
            var propertyChangedModel = new TestNotifyPropertyChangedModel();
            var canNotifyValue = false;
            var propertyName = "test";
            Func<object?, object?, bool> canNotify = (s, o) =>
            {
                s.ShouldEqual(propertyChangedModel);
                ((PropertyChangedEventArgs) o!).PropertyName.ShouldEqual(propertyName);
                return canNotifyValue;
            };
            var compositeCommand = new CompositeCommand();
            var conditionEventCommandComponent = new CommandEventHandler(null, ThreadExecutionMode.Current) {CanNotify = canNotify};
            compositeCommand.AddComponent(conditionEventCommandComponent);
            compositeCommand.AddNotifier(propertyChangedModel);

            var executed = 0;
            EventHandler handler = (_, _) => ++executed;
            conditionEventCommandComponent.AddCanExecuteChanged(compositeCommand, handler, null);

            executed.ShouldEqual(0);
            propertyChangedModel.OnPropertyChanged(propertyName);
            executed.ShouldEqual(0);

            canNotifyValue = true;
            propertyChangedModel.OnPropertyChanged(propertyName);
            executed.ShouldEqual(1);
        }

        [Fact]
        public void ShouldSubscribeUnsubscribeRaiseEventHandler()
        {
            var compositeCommand = new CompositeCommand();
            var conditionEventCommandComponent = new CommandEventHandler(null, ThreadExecutionMode.Current);
            compositeCommand.AddComponent(conditionEventCommandComponent);
            var executed = 0;
            EventHandler handler = (sender, args) =>
            {
                sender.ShouldEqual(compositeCommand);
                ++executed;
            };
            conditionEventCommandComponent.AddCanExecuteChanged(compositeCommand, handler, null);

            executed.ShouldEqual(0);
            conditionEventCommandComponent.RaiseCanExecuteChanged();
            executed.ShouldEqual(1);

            conditionEventCommandComponent.RemoveCanExecuteChanged(compositeCommand, handler, null);
            conditionEventCommandComponent.RaiseCanExecuteChanged();
            executed.ShouldEqual(1);
        }

        [Fact]
        public void ShouldSuspendNotifications()
        {
            var compositeCommand = new CompositeCommand();
            var conditionEventCommandComponent = new CommandEventHandler(null, ThreadExecutionMode.Current);
            compositeCommand.AddComponent(conditionEventCommandComponent);
            var executed = 0;
            EventHandler handler = (sender, args) => ++executed;
            conditionEventCommandComponent.AddCanExecuteChanged(compositeCommand, handler, null);

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
    }
}