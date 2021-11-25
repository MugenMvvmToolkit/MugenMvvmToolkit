using System;
using System.Collections.Generic;
using System.ComponentModel;
using MugenMvvm.Commands.Components;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Models.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Commands.Components
{
    public class PropertyChangedCommandObserverTest : UnitTestBase
    {
        public PropertyChangedCommandObserverTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            Command.AddComponent(new CommandEventHandler());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        public void ShouldListenPropertyChangedEvent(int listenersCount)
        {
            var observer = new PropertyChangedCommandObserver();
            Command.AddComponent(observer);

            var models = new List<TestNotifyPropertyChangedModel>();
            for (var i = 0; i < listenersCount; i++)
            {
                var notifier = new TestNotifyPropertyChangedModel();
                models.Add(notifier);
                observer.Add(notifier).ShouldBeTrue();
                observer.Add(notifier).ShouldBeFalse();
                observer.Contains(notifier).ShouldBeTrue();
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
                observer.Remove(token).ShouldBeTrue();
                observer.Remove(token).ShouldBeFalse();
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
                ((PropertyChangedEventArgs) o!).PropertyName.ShouldEqual(propertyName);
                return canNotifyValue;
            };
            var observer = new PropertyChangedCommandObserver {CanNotify = canNotify};
            Command.AddComponent(observer);
            observer.Add(propertyChangedModel);

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
    }
}