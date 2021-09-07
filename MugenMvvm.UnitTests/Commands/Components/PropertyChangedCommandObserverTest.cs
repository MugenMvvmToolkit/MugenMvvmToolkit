using System;
using System.Collections.Generic;
using System.ComponentModel;
using MugenMvvm.Commands.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Models.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Commands.Components
{
    [Collection(SharedContext)]
    public class PropertyChangedCommandObserverTest : UnitTestBase
    {
        public PropertyChangedCommandObserverTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            Command.AddComponent(new CommandEventHandler());
            RegisterDisposeToken(WithGlobalService(WeakReferenceManager));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        public void ShouldListenPropertyChangedEvent(int listenersCount)
        {
            var commandNotifier = new PropertyChangedCommandObserver();
            Command.AddComponent(commandNotifier);

            var models = new List<TestNotifyPropertyChangedModel>();
            for (var i = 0; i < listenersCount; i++)
            {
                var notifier = new TestNotifyPropertyChangedModel();
                models.Add(notifier);
                commandNotifier.Add(notifier).ShouldBeTrue();
                commandNotifier.Add(notifier).ShouldBeFalse();
                commandNotifier.Contains(notifier).ShouldBeTrue();
            }

            var executed = 0;
            EventHandler handler = (_, _) => ++executed;
            Command.CanExecuteChanged += handler;

            executed.ShouldEqual(0);
            foreach (var model in models)
                model.OnPropertyChanged("Test");
            executed.ShouldEqual(listenersCount);

            foreach (var token in models)
            {
                commandNotifier.Remove(token).ShouldBeTrue();
                commandNotifier.Remove(token).ShouldBeFalse();
            }

            foreach (var model in models)
                model.OnPropertyChanged("Test");
            executed.ShouldEqual(listenersCount);
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
                ((PropertyChangedEventArgs)o!).PropertyName.ShouldEqual(propertyName);
                return canNotifyValue;
            };
            var commandNotifier = new PropertyChangedCommandObserver { CanNotify = canNotify };
            Command.AddComponent(commandNotifier);
            commandNotifier.Add(propertyChangedModel);

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
            var propertyChangedModel = new TestNotifyPropertyChangedModel();
            var reference = ShouldListenPropertyChangedWeakImpl(propertyChangedModel);
            GcCollect();
            propertyChangedModel.OnPropertyChanged("test");
            reference.IsAlive.ShouldBeFalse();
        }

        private WeakReference ShouldListenPropertyChangedWeakImpl(TestNotifyPropertyChangedModel propertyChangedModel)
        {
            var commandNotifier = new PropertyChangedCommandObserver();
            Command.AddComponent(commandNotifier);
            commandNotifier.Add(propertyChangedModel);
            var executed = 0;
            EventHandler handler = (_, _) => ++executed;
            Command.CanExecuteChanged += handler;

            executed.ShouldEqual(0);
            propertyChangedModel.OnPropertyChanged("test");
            executed.ShouldEqual(1);

            Command.ClearComponents();
            return new WeakReference(commandNotifier);
        }
    }
}