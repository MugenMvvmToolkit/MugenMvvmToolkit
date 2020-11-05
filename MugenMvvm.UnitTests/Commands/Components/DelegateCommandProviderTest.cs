using System;
using MugenMvvm.Commands;
using MugenMvvm.Commands.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Commands.Components
{
    public class DelegateCommandProviderTest : UnitTestBase
    {
        #region Fields

        private readonly DelegateCommandProvider _component;

        #endregion

        #region Constructors

        public DelegateCommandProviderTest()
        {
            _component = new DelegateCommandProvider();
        }

        #endregion

        #region Methods

        [Fact]
        public void TryGetCommandShouldReturnNullNotSupportedType() => _component.TryGetCommand<object>(new CommandManager(), _component, DefaultMetadata).ShouldBeNull();

        [Theory]
        [InlineData(false, null, null, false, false, false, false)]
        [InlineData(true, true, 1, true, true, true, true)]//CommandExecutionBehavior.CanExecuteBeforeExecute
        public void TryGetCommandShouldUseValidParameters1(bool hasCanExecute, bool? allowMultipleExecution,
            int? executionModeValue, bool hasThreadExecutionMode, bool addNotifiers, bool hasCanNotify, bool hasMetadata)
        {
            var executedCount = 0;
            var canExecuteValue = true;
            var executionMode = executionModeValue == null ? null : CommandExecutionBehavior.Get(executionModeValue.Value);
            Action execute = () => { ++executedCount; };
            var canExecute = hasCanExecute ? () => { return canExecuteValue; } : (Func<bool>?) null;
            var threadMode = hasThreadExecutionMode ? ThreadExecutionMode.Background : null;
            var notifiers = addNotifiers ? new[] {new object()} : null;
            var canNotify = GetHasCanNotify(hasCanNotify);
            var metadata = hasMetadata ? DefaultMetadata : null;

            var request = DelegateCommandRequest.Get(execute, canExecute, allowMultipleExecution, executionMode, threadMode, notifiers, canNotify);

            var command = _component.TryGetCommand<object>(new CommandManager(), request, metadata)!;
            command.ShouldNotBeNull();

            var component = command.GetComponent<DelegateExecutorCommandComponent<object>>();
            component.ExecuteAsync(command, null, null);
            executedCount.ShouldEqual(1);
            if (canExecute != null)
            {
                component.CanExecute(command, null, null).ShouldEqual(canExecuteValue);
                canExecuteValue = false;
                command.CanExecute(null).ShouldEqual(canExecuteValue);
                if (notifiers != null)
                    command.GetComponent<ConditionEventCommandComponent>().ShouldNotBeNull();
            }
        }

        private static Func<object, bool>? GetHasCanNotify(bool value)
        {
            if (value)
                return i => true;
            return null;
        }

        #endregion
    }
}