using System;
using System.Threading.Tasks;
using MugenMvvm.Commands;
using MugenMvvm.Commands.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTests.Metadata.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Commands.Components
{
    public class DelegateCommandProviderTest : UnitTestBase
    {
        public DelegateCommandProviderTest()
        {
            _component = new DelegateCommandProvider();
        }

        private readonly DelegateCommandProvider _component;

        [Theory]
        [InlineData(false, null, null, false, false, false, false)]
        [InlineData(true, true, CommandExecutionBehavior.CheckCanExecuteValue, true, true, true, true)]
        public async Task TryGetCommandShouldUseValidParameters1(bool hasCanExecute, bool? allowMultipleExecution,
            int? executionModeValue, bool hasThreadExecutionMode, bool addNotifiers, bool hasCanNotify, bool hasMetadata)
        {
            var executedCount = 0;
            var canExecuteValue = true;
            var executionMode = executionModeValue == null ? null : CommandExecutionBehavior.Get(executionModeValue.Value);
            Action execute = () => { ++executedCount; };
            var canExecute = hasCanExecute ? () => canExecuteValue : (Func<bool>?) null;
            var threadMode = hasThreadExecutionMode ? ThreadExecutionMode.Background : null;
            var notifiers = addNotifiers ? new[] {new object()} : null;
            var canNotify = GetHasCanNotify(hasCanNotify);
            var metadata = hasMetadata ? DefaultMetadata : null;

            var request = DelegateCommandRequest.Get(execute, canExecute, allowMultipleExecution, executionMode, threadMode, notifiers, canNotify);

            var command = _component.TryGetCommand<object>(null!, this, request, metadata)!;
            command.ShouldNotBeNull();

            var component = command.GetComponent<DelegateCommandExecutor<object>>();
            await component.ExecuteAsync(command, null, default, null);
            executedCount.ShouldEqual(1);
            if (canExecute != null)
            {
                component.CanExecute(command, null, null).ShouldEqual(canExecuteValue);
                canExecuteValue = false;
                command.CanExecute(null).ShouldEqual(canExecuteValue);
                if (notifiers != null)
                    command.GetComponent<CommandEventHandler>().ShouldNotBeNull();
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldCacheCommandEventHandler(bool cache)
        {
            _component.CacheCommandEventHandler = cache;
            var metadataOwner = new TestMetadataOwner<IMetadataContext> {Metadata = new MetadataContext()};
            Action execute = () => { };
            Func<bool> canExecute = () => true;
            var request = DelegateCommandRequest.Get(execute, canExecute, null, null, null, default, null);
            var command1 = _component.TryGetCommand<object>(null!, metadataOwner, request, null)!;
            var command2 = _component.TryGetCommand<object>(null!, metadataOwner, request, null)!;
            if (cache)
                command1.GetComponent<CommandEventHandler>().ShouldEqual(command2.GetComponent<CommandEventHandler>());
            else
                command1.GetComponent<CommandEventHandler>().ShouldNotEqual(command2.GetComponent<CommandEventHandler>());

            var command3 = _component.TryGetCommand<object>(null!, this, request, null)!;
            var command4 = _component.TryGetCommand<object>(null!, this, request, null)!;
            command3.GetComponent<CommandEventHandler>().ShouldNotEqual(command4.GetComponent<CommandEventHandler>());

            command1 = _component.TryGetCommand<object>(null!, metadataOwner, DelegateCommandRequest.Get(execute, canExecute, null, null, null, metadataOwner, null), null)!;
            command2 = _component.TryGetCommand<object>(null!, metadataOwner, DelegateCommandRequest.Get(execute, canExecute, null, null, null, metadataOwner, null), null)!;
            command1.GetComponent<CommandEventHandler>().ShouldNotEqual(command2.GetComponent<CommandEventHandler>());
        }

        private static Func<object?, object?, bool>? GetHasCanNotify(bool value)
        {
            if (value)
                return (_, _) => true;
            return null;
        }

        [Fact]
        public void TryGetCommandShouldReturnNullNotSupportedType() => _component.TryGetCommand<object>(null!, this, _component, DefaultMetadata).ShouldBeNull();
    }
}