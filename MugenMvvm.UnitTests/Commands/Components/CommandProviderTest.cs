using System;
using System.ComponentModel;
using System.Threading.Tasks;
using MugenMvvm.Commands;
using MugenMvvm.Commands.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;
using MugenMvvm.Tests.Metadata;
using MugenMvvm.UnitTests.Models.Internal;
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
        public void ShouldAddCommandEventHandlerOnlyForINotifyPropertyChanged()
        {
            Action execute = () => { };
            Func<bool> canExecute = () => true;
            var request = DelegateCommandRequest.Get(execute, canExecute, null, null, default, null);
            var propertyChangedModel = new TestNotifyPropertyChangedModel();
            var cmd1 = CommandManager.TryGetCommand<object>(propertyChangedModel, request)!;
            var cmd2 = CommandManager.TryGetCommand<object>(this, request)!;
            cmd1.GetComponentOptional<PropertyChangedCommandNotifier>().ShouldNotBeNull();
            cmd2.GetComponentOptional<PropertyChangedCommandNotifier>().ShouldBeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldCacheCommandEventHandler(bool cache)
        {
            _component.CacheCommandNotifier = cache;
            var metadataOwner = new TestMetadataOwnerPropertyChanged { Metadata = new MetadataContext() };
            Action execute = () => { };
            Func<bool> canExecute = () => true;
            var request = DelegateCommandRequest.Get(execute, canExecute, null, null, default, null);
            var command1 = CommandManager.TryGetCommand<object>(metadataOwner, request)!;
            var command2 = CommandManager.TryGetCommand<object>(metadataOwner, request)!;
            if (cache)
                command1.GetComponent<PropertyChangedCommandNotifier>().ShouldEqual(command2.GetComponent<PropertyChangedCommandNotifier>());
            else
                command1.GetComponent<PropertyChangedCommandNotifier>().ShouldNotEqual(command2.GetComponent<PropertyChangedCommandNotifier>());

            var propertyChangedModel = new TestNotifyPropertyChangedModel();
            var command3 = CommandManager.TryGetCommand<object>(propertyChangedModel, request)!;
            var command4 = CommandManager.TryGetCommand<object>(propertyChangedModel, request)!;
            command3.GetComponent<PropertyChangedCommandNotifier>().ShouldNotEqual(command4.GetComponent<PropertyChangedCommandNotifier>());

            command1 = CommandManager.TryGetCommand<object>(metadataOwner, DelegateCommandRequest.Get(execute, canExecute, null, null, metadataOwner, null))!;
            command2 = CommandManager.TryGetCommand<object>(metadataOwner, DelegateCommandRequest.Get(execute, canExecute, null, null, metadataOwner, null))!;
            command1.GetComponent<PropertyChangedCommandNotifier>().ShouldNotEqual(command2.GetComponent<PropertyChangedCommandNotifier>());
        }

        [Fact]
        public void ShouldCreateRawCommand()
        {
            var command = CommandManager.TryGetCommand<object>(this, CommandMetadata.RawCommandRequest)!;
            command.GetComponents<object>().Count.ShouldEqual(1);
            command.GetComponentOptional<CommandEventHandler>().ShouldNotBeNull();
        }

        [Fact]
        public void TryGetCommandShouldReturnDefaultCommandAnyRequest()
        {
            var command = CommandManager.TryGetCommand<object>(this, this, DefaultMetadata)!;
            command.GetComponents<object>().Count.ShouldEqual(1);
            command.GetComponentOptional<CommandEventHandler>().ShouldNotBeNull();
        }

        [Theory]
        [InlineData(false, null, false, false, false, false)]
        [InlineData(true, true, true, true, true, true)]
        public async Task TryGetCommandShouldUseValidParameters1(bool hasCanExecute, bool? allowMultipleExecution, bool hasThreadExecutionMode, bool addNotifiers,
            bool hasCanNotify, bool hasMetadata)
        {
            var executedCount = 0;
            var canExecuteValue = true;
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

            var request = DelegateCommandRequest.Get(execute, canExecute, allowMultipleExecution, threadMode, notifiers, canNotify);

            var command = CommandManager.TryGetCommand<object>(this, request, metadata)!;
            command.ShouldNotBeNull();

            var component = command.GetComponent<DelegateCommandExecutor.IDelegateCommandExecutor>();
            await component.TryExecuteAsync(command, null, DefaultCancellationToken, DefaultMetadata);
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

        protected override ICommandManager GetCommandManager() => new CommandManager(ComponentCollectionManager);

        private static Func<object?, object?, bool>? GetHasCanNotify(bool value)
        {
            if (value)
                return (_, _) => true;
            return null;
        }

        private sealed class TestMetadataOwnerPropertyChanged : TestMetadataOwner<IMetadataContext>, INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;
        }
    }
}