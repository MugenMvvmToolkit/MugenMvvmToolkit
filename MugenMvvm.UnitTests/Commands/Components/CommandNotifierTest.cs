using System;
using System.Collections.Generic;
using System.ComponentModel;
using MugenMvvm.Commands.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Internal;
using MugenMvvm.Messaging;
using MugenMvvm.Tests.Internal;
using MugenMvvm.Tests.Messaging;
using MugenMvvm.UnitTests.Models.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Commands.Components
{
    [Collection(SharedContext)]
    public class CommandNotifierTest : UnitTestBase
    {
        public CommandNotifierTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            Command.AddComponent(new CommandEventHandler(ThreadDispatcher, ThreadExecutionMode.Current));
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
            var commandNotifier = new CommandNotifier { CanNotify = canNotify };
            Command.AddComponent(commandNotifier);
            commandNotifier.AddNotifier(propertyChangedModel);

            var executed = 0;
            EventHandler handler = (_, _) => ++executed;
            Command.CanExecuteChanged += handler;

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

            var commandNotifier = new CommandNotifier { CanNotify = canNotify };
            Command.AddComponent(commandNotifier);
            commandNotifier.AddNotifier(hasService ? new TestHasServiceModel<IMessenger> { Service = messenger } : messenger);

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
        [InlineData(1, true)]
        [InlineData(1, false)]
        [InlineData(5, true)]
        [InlineData(5, false)]
        public void ShouldSubscribeMessenger(int listenersCount, bool hasService)
        {
            var subscribedCount = 0;
            var commandNotifier = new CommandNotifier();
            Command.AddComponent(commandNotifier);
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
                        o.ShouldBeType<CommandNotifier.WeakHandler>();
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
                var token = commandNotifier.AddNotifier(notifier);
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
        [InlineData(1)]
        [InlineData(5)]
        public void ShouldListenPropertyChangedEvent(int listenersCount)
        {
            var commandNotifier = new CommandNotifier();
            Command.AddComponent(commandNotifier);

            var models = new List<TestNotifyPropertyChangedModel>();
            var tokens = new List<ActionToken>();
            for (var i = 0; i < listenersCount; i++)
            {
                var notifier = new TestNotifyPropertyChangedModel { ThreadDispatcher = ThreadDispatcher };
                models.Add(notifier);
                var token = commandNotifier.AddNotifier(notifier);
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

        private WeakReference ShouldListenPropertyChangedWeakImpl(TestNotifyPropertyChangedModel propertyChangedModel)
        {
            var commandNotifier = new CommandNotifier();
            Command.AddComponent(commandNotifier);
            commandNotifier.AddNotifier(propertyChangedModel);
            var executed = 0;
            EventHandler handler = (_, _) => ++executed;
            Command.CanExecuteChanged += handler;

            executed.ShouldEqual(0);
            propertyChangedModel.OnPropertyChanged("test");
            executed.ShouldEqual(1);

            return new WeakReference(commandNotifier);
        }
    }
}