using System;
using System.Threading.Tasks;
using MugenMvvm.Commands;
using MugenMvvm.Commands.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;
using MugenMvvm.Tests.Metadata;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Commands.Components
{
    [Collection(SharedContext)]
    public class CommandProviderTest : UnitTestBase
    {
        private readonly CommandProvider _component;

        public CommandProviderTest()
        {
            _component = new CommandProvider(ThreadDispatcher, ComponentCollectionManager);
            CommandManager.AddComponent(_component);
            RegisterDisposeToken(WithGlobalService(WeakReferenceManager));
        }

        [Fact]
        public void ShouldCreateRawCommand()
        {
            var command = CommandManager.TryGetCommand<object>(this, RawCommandRequest.Instance)!;
            command.GetComponents<object>().Count.ShouldEqual(1);
            command.GetComponentOptional<CommandEventHandler>().ShouldNotBeNull();
        }

        [Fact]
        public void TryGetCommandShouldReturnNullNotSupportedType() => CommandManager.TryGetCommand<object>(this, this, DefaultMetadata).ShouldBeNull();

        protected override ICommandManager GetCommandManager() => new CommandManager(ComponentCollectionManager);

        [Theory]
        [InlineData(false, null, null, false, false, false, false)]
        [InlineData(true, true, CommandExecutionBehavior.CheckCanExecuteValue, true, true, true, true)]
        public async Task TryGetCommandShouldUseValidParameters1(bool hasCanExecute, bool? allowMultipleExecution,
            int? executionModeValue, bool hasThreadExecutionMode, bool addNotifiers, bool hasCanNotify, bool hasMetadata)
        {
            var executedCount = 0;
            var canExecuteValue = true;
            var executionMode = executionModeValue == null ? null : CommandExecutionBehavior.Get(executionModeValue.Value);
            Action<IReadOnlyMetadataContext?> execute = m =>
            {
                m.ShouldEqual(DefaultMetadata);
                ++executedCount;
            };
            var canExecute = hasCanExecute
                ? m =>
                {
                    m.ShouldEqual(DefaultMetadata);
                    return canExecuteValue;
                }
                : (Func<IReadOnlyMetadataContext?, bool>?)null;
            var threadMode = hasThreadExecutionMode ? ThreadExecutionMode.Background : null;
            var notifiers = addNotifiers ? new[] { new object() } : null;
            var canNotify = GetHasCanNotify(hasCanNotify);
            var metadata = hasMetadata ? DefaultMetadata : null;

            var request = DelegateCommandRequest.Get(execute, canExecute, allowMultipleExecution, executionMode, threadMode, notifiers, canNotify);

            var command = CommandManager.TryGetCommand<object>(this, request, metadata)!;
            command.ShouldNotBeNull();

            var component = command.GetComponent<DelegateCommandExecutor<object>>();
            await component.ExecuteAsync(command, null, DefaultCancellationToken, DefaultMetadata);
            executedCount.ShouldEqual(1);
            if (canExecute != null)
            {
                component.CanExecute(command, null, DefaultMetadata).ShouldEqual(canExecuteValue);
                canExecuteValue = false;
                command.CanExecute(null, DefaultMetadata).ShouldEqual(canExecuteValue);
                if (notifiers != null)
                    command.GetComponent<CommandEventHandler>().ShouldNotBeNull();
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldCacheCommandEventHandler1(bool cache)
        {
            _component.CacheCommandEventHandler = cache;
            var metadataOwner = new TestMetadataOwner<IMetadataContext> { Metadata = new MetadataContext() };
            Action execute = () => { };
            Func<bool> canExecute = () => true;
            var request = DelegateCommandRequest.Get(execute, canExecute, null, null, null, default, null);
            var command1 = CommandManager.TryGetCommand<object>(metadataOwner, request)!;
            var command2 = CommandManager.TryGetCommand<object>(metadataOwner, request)!;
            if (cache)
                command1.GetComponent<CommandEventHandler>().ShouldEqual(command2.GetComponent<CommandEventHandler>());
            else
                command1.GetComponent<CommandEventHandler>().ShouldNotEqual(command2.GetComponent<CommandEventHandler>());

            var command3 = CommandManager.TryGetCommand<object>(this, request)!;
            var command4 = CommandManager.TryGetCommand<object>(this, request)!;
            command3.GetComponent<CommandEventHandler>().ShouldNotEqual(command4.GetComponent<CommandEventHandler>());

            command1 = CommandManager.TryGetCommand<object>(metadataOwner, DelegateCommandRequest.Get(execute, canExecute, null, null, null, metadataOwner, null))!;
            command2 = CommandManager.TryGetCommand<object>(metadataOwner, DelegateCommandRequest.Get(execute, canExecute, null, null, null, metadataOwner, null))!;
            command1.GetComponent<CommandEventHandler>().ShouldNotEqual(command2.GetComponent<CommandEventHandler>());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldCacheCommandEventHandler2(bool cache)
        {
            _component.CacheCommandEventHandler = cache;
            var metadataOwner = new TestMetadataOwner<IMetadataContext> { Metadata = new MetadataContext() };
            var request = RawCommandRequest.Instance;
            var command1 = CommandManager.TryGetCommand<object>(metadataOwner, request)!;
            var command2 = CommandManager.TryGetCommand<object>(metadataOwner, request)!;
            if (cache)
                command1.GetComponent<CommandEventHandler>().ShouldEqual(command2.GetComponent<CommandEventHandler>());
            else
                command1.GetComponent<CommandEventHandler>().ShouldNotEqual(command2.GetComponent<CommandEventHandler>());

            var command3 = CommandManager.TryGetCommand<object>(this, request)!;
            var command4 = CommandManager.TryGetCommand<object>(this, request)!;
            command3.GetComponent<CommandEventHandler>().ShouldNotEqual(command4.GetComponent<CommandEventHandler>());
        }

        private static Func<object?, object?, bool>? GetHasCanNotify(bool value)
        {
            if (value)
                return (_, _) => true;
            return null;
        }
    }
}