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
using MugenMvvm.Tests.Internal;
using MugenMvvm.Tests.Messaging;
using MugenMvvm.Tests.Threading;
using MugenMvvm.Threading;
using MugenMvvm.UnitTests.Models.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Commands.Components
{
    [Collection(SharedContext)]
    public class CommandEventHandlerTest : UnitTestBase
    {
        public CommandEventHandlerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            RegisterDisposeToken(WithGlobalService(WeakReferenceManager));
        }

        [Fact]
        public void ShouldListenPropertyChangedEventCanNotify()
        {
            var propertyChangedModel = new TestNotifyPropertyChangedModel { ThreadDispatcher = ThreadDispatcher };
            var canNotifyValue = false;
            var propertyName = "test";
            Func<object?, object?, bool> canNotify = (s, o) =>
            {
                s.ShouldEqual(propertyChangedModel);
                ((PropertyChangedEventArgs)o!).PropertyName.ShouldEqual(propertyName);
                return canNotifyValue;
            };
            var conditionEventCommandComponent = new CommandEventHandler(ThreadDispatcher, ThreadExecutionMode.Current) { CanNotify = canNotify };
            Command.AddComponent(conditionEventCommandComponent);
            Command.AddNotifier(propertyChangedModel);

            var executed = 0;
            EventHandler handler = (_, _) => ++executed;
            conditionEventCommandComponent.AddCanExecuteChanged(Command, handler, null);

            executed.ShouldEqual(0);
            propertyChangedModel.OnPropertyChanged(propertyName);
            executed.ShouldEqual(0);

            canNotifyValue = true;
            propertyChangedModel.OnPropertyChanged(propertyName);
            executed.ShouldEqual(1);
        }

        [Fact(Skip = ReleaseTest)]
        public void ShouldListenPropertyChangedWeak()
        {
            var propertyChangedModel = new TestNotifyPropertyChangedModel { ThreadDispatcher = ThreadDispatcher };
            var reference = ShouldListenPropertyChangedWeakImpl(propertyChangedModel);
            GcCollect();
            propertyChangedModel.OnPropertyChanged("test");
            reference.IsAlive.ShouldBeFalse();
        }

        [Fact]
        public void ShouldSubscribeUnsubscribeRaiseEventHandler()
        {
            var conditionEventCommandComponent = new CommandEventHandler(ThreadDispatcher, ThreadExecutionMode.Current);
            Command.AddComponent(conditionEventCommandComponent);
            var executed = 0;
            EventHandler handler = (sender, args) =>
            {
                sender.ShouldEqual(Command);
                ++executed;
            };
            Command.CanExecuteChanged += handler;

            executed.ShouldEqual(0);
            conditionEventCommandComponent.RaiseCanExecuteChanged();
            executed.ShouldEqual(1);

            Command.CanExecuteChanged -= handler;
            conditionEventCommandComponent.RaiseCanExecuteChanged();
            executed.ShouldEqual(1);
        }

        [Fact]
        public void ShouldSuspendNotifications()
        {
            var conditionEventCommandComponent = new CommandEventHandler(ThreadDispatcher, ThreadExecutionMode.Current);
            Command.AddComponent(conditionEventCommandComponent);
            var executed = 0;
            EventHandler handler = (sender, args) => ++executed;
            Command.CanExecuteChanged += handler;

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
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void ShouldUseLocalThreadDispatcher(int mode)
        {
            var executionMode = ThreadExecutionMode.Get(mode);
            Action? invoke = null;
            var threadDispatcher = new ThreadDispatcher(ComponentCollectionManager);
            threadDispatcher.AddComponent(new TestThreadDispatcherComponent
            {
                Execute = (_, action, mode, arg3, _) =>
                {
                    mode.ShouldEqual(executionMode);
                    invoke = () => action(arg3);
                    return true;
                }
            });

            var conditionEventCommandComponent = new CommandEventHandler(threadDispatcher, executionMode);
            Command.AddComponent(conditionEventCommandComponent);
            var executed = 0;
            EventHandler handler = (sender, args) => ++executed;
            Command.CanExecuteChanged += handler;

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
            var conditionEventCommandComponent = new CommandEventHandler(ThreadDispatcher, ThreadExecutionMode.Current);
            Command.AddComponent(conditionEventCommandComponent);

            var models = new List<TestNotifyPropertyChangedModel>();
            var tokens = new List<ActionToken>();
            for (var i = 0; i < listenersCount; i++)
            {
                var notifier = new TestNotifyPropertyChangedModel { ThreadDispatcher = ThreadDispatcher };
                models.Add(notifier);
                var token = Command.AddNotifier(notifier);
                token.IsEmpty.ShouldBeFalse();
                tokens.Add(token);
            }

            var executed = 0;
            EventHandler handler = (_, _) => ++executed;
            Command.CanExecuteChanged += handler;

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
            var conditionEventCommandComponent = new CommandEventHandler(ThreadDispatcher, ThreadExecutionMode.Current);
            Command.AddComponent(conditionEventCommandComponent);
            IMessengerHandler? messengerHandler = null;

            var tokens = new List<ActionToken>();
            for (var i = 0; i < listenersCount; i++)
            {
                var messenger = new Messenger(ComponentCollectionManager);
                messenger.AddComponent(new TestMessengerSubscriberComponent
                {
                    TrySubscribe = (_, o, _, _) =>
                    {
                        ++subscribedCount;
                        messengerHandler = (IMessengerHandler?)o;
                        o.ShouldBeType<CommandEventHandler.WeakHandler>();
                        return true;
                    },
                    TryUnsubscribe = (_, o, _) =>
                    {
                        --subscribedCount;
                        o.ShouldEqual(messengerHandler);
                        return true;
                    }
                });

                var notifier = hasService ? (object)new TestHasServiceModel<IMessenger> { Service = messenger } : messenger;
                var token = Command.AddNotifier(notifier);
                token.IsEmpty.ShouldBeFalse();
                tokens.Add(token);
            }

            var executed = 0;
            EventHandler handler = (_, _) => ++executed;
            Command.CanExecuteChanged += handler;

            subscribedCount.ShouldEqual(listenersCount);
            executed.ShouldEqual(0);
            messengerHandler.ShouldNotBeNull();
            messengerHandler!.CanHandle(typeof(object)).ShouldBeTrue();
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
            var messenger = new Messenger(ComponentCollectionManager);
            var component = new TestMessengerSubscriberComponent
            {
                TrySubscribe = (_, o, _, _) =>
                {
                    handlerRaw = (IMessengerHandler?)o;
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

            var conditionEventCommandComponent = new CommandEventHandler(ThreadDispatcher, ThreadExecutionMode.Current) { CanNotify = canNotify };
            Command.AddComponent(conditionEventCommandComponent);
            Command.AddNotifier(hasService ? new TestHasServiceModel<IMessenger> { Service = messenger } : messenger);

            var executed = 0;
            EventHandler handler = (_, _) => ++executed;
            Command.CanExecuteChanged += handler;

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
            var conditionEventCommandComponent = new CommandEventHandler(ThreadDispatcher, ThreadExecutionMode.Current);
            conditionEventCommandComponent.IsDisposable.ShouldBeTrue();
            conditionEventCommandComponent.IsDisposable = canDispose;
            Command.AddComponent(conditionEventCommandComponent);
            var executed = 0;
            EventHandler handler = (_, _) => ++executed;
            Command.CanExecuteChanged += handler;

            executed.ShouldEqual(0);
            conditionEventCommandComponent.Dispose();
            conditionEventCommandComponent.RaiseCanExecuteChanged();
            executed.ShouldEqual(canDispose ? 0 : 1);
        }

        private WeakReference ShouldListenPropertyChangedWeakImpl(TestNotifyPropertyChangedModel propertyChangedModel)
        {
            var compositeCommand = new CompositeCommand(null, ComponentCollectionManager);
            var conditionEventCommandComponent = new CommandEventHandler(ThreadDispatcher, ThreadExecutionMode.Current);
            compositeCommand.AddComponent(conditionEventCommandComponent);
            conditionEventCommandComponent.AddNotifier(propertyChangedModel);
            var executed = 0;
            EventHandler handler = (_, _) => ++executed;
            conditionEventCommandComponent.AddCanExecuteChanged(compositeCommand, handler, null);

            executed.ShouldEqual(0);
            propertyChangedModel.OnPropertyChanged("test");
            executed.ShouldEqual(1);

            return new WeakReference(conditionEventCommandComponent);
        }
    }
}