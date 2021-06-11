using System;
using System.Collections.Generic;
using System.ComponentModel;
using MugenMvvm.Commands.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Models.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Commands.Components
{
    [Collection(SharedContext)]
    public class PropertyChangedCommandNotifierTest : UnitTestBase
    {
        public PropertyChangedCommandNotifierTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
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
            var commandNotifier = new PropertyChangedCommandNotifier { CanNotify = canNotify };
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
        [InlineData(1)]
        [InlineData(5)]
        public void ShouldListenPropertyChangedEvent(int listenersCount)
        {
            var commandNotifier = new PropertyChangedCommandNotifier();
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
            var commandNotifier = new PropertyChangedCommandNotifier();
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