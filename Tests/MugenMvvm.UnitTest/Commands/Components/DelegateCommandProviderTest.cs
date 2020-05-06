using System;
using MugenMvvm.Commands;
using MugenMvvm.Commands.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Commands.Components
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
        public void TryGetCommandShouldReturnNullNotSupportedType()
        {
            _component.TryGetCommand(_component, DefaultMetadata).ShouldBeNull();
        }

        [Theory]
        [InlineData(false, null, null, false, false, false, false)]
        [InlineData(true, true, CommandExecutionMode.CanExecuteBeforeExecute, true, true, true, true)]
        public void TryGetCommandShouldUseValidParameters1(bool hasCanExecute, bool? allowMultipleExecution,
            CommandExecutionMode? executionMode, bool hasThreadExecutionMode, bool addNotifiers, bool hasCanNotify, bool hasMetadata)
        {
            var executedCount = 0;
            var canExecuteValue = true;
            Action execute = () => { ++executedCount; };
            var canExecute = hasCanExecute ? () => { return canExecuteValue; } : (Func<bool>?) null;
            var threadMode = hasThreadExecutionMode ? ThreadExecutionMode.Background : null;
            var notifiers = addNotifiers ? new[] {new object()} : null;
            var canNotify = GetHasCanNotify(hasCanNotify);
            var metadata = hasMetadata ? DefaultMetadata : null;

            var request = new DelegateCommandRequest(
                (in DelegateCommandRequest commandRequest, DelegateCommandRequest.IProvider provider, IReadOnlyMetadataContext? arg3) => { return provider.TryGetCommand<object>(commandRequest, arg3); }, execute,
                canExecute, allowMultipleExecution, executionMode, threadMode, notifiers, canNotify);

            var command = _component.TryGetCommand(request, metadata)!;
            command.ShouldNotBeNull();

            var component = command.GetComponent<DelegateExecutorCommandComponent<object>>();
            component.ExecuteAsync(null);
            executedCount.ShouldEqual(1);
            if (canExecute != null)
            {
                component.CanExecute(null).ShouldEqual(canExecuteValue);
                canExecuteValue = false;
                command.CanExecute(null).ShouldEqual(canExecuteValue);
                if (request.Notifiers != null)
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