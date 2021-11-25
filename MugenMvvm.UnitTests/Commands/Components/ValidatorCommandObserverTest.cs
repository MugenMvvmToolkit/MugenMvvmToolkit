using System;
using System.Collections.Generic;
using MugenMvvm.Commands.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Validation;
using MugenMvvm.Validation.Components;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Commands.Components
{
    public class ValidatorCommandObserverTest : UnitTestBase
    {
        public ValidatorCommandObserverTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            Command.AddComponent(new CommandEventHandler());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        public void ShouldListenValidatorEvent(int listenersCount)
        {
            var observer = new ValidatorCommandObserver();
            Command.AddComponent(observer);

            var models = new List<Validator>();
            for (var i = 0; i < listenersCount; i++)
            {
                var notifier = new Validator(null, ComponentCollectionManager);
                notifier.AddComponent(new ValidatorErrorManager());
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
                model.SetErrors(this, new ValidationErrorInfo(this, "Test", this));
            executed.ShouldEqual(listenersCount);

            foreach (var token in models)
            {
                observer.Remove(token).ShouldBeTrue();
                observer.Remove(token).ShouldBeFalse();
            }

            foreach (var model in models)
                model.ClearErrors();
            executed.ShouldEqual(listenersCount);
        }
    }
}