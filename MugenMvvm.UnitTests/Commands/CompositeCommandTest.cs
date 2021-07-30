using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Commands;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;
using MugenMvvm.Tests.Commands;
using MugenMvvm.Tests.Internal;
using MugenMvvm.UnitTests.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Commands
{
    public class CompositeCommandTest : SuspendableComponentOwnerTestBase<ICompositeCommand>
    {
        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void AddCanExecuteChangedShouldBeHandledByComponents(int componentCount)
        {
            var count = 0;
            EventHandler eventHandler = (sender, args) => { };
            for (var i = 0; i < componentCount; i++)
            {
                Command.AddComponent(new TestCommandEventHandlerComponent
                {
                    AddCanExecuteChanged = (c, handler) =>
                    {
                        ++count;
                        c.ShouldEqual(Command);
                        handler.ShouldEqual(eventHandler);
                    }
                });
            }

            Command.CanExecuteChanged += eventHandler;
            count.ShouldEqual(componentCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void CanExecuteShouldBeHandledByComponents(int componentCount)
        {
            var count = 0;
            var canExecute = false;
            Command.CanExecute(Command).ShouldBeTrue();
            for (var i = 0; i < componentCount; i++)
            {
                Command.AddComponent(new TestCommandConditionComponent
                {
                    CanExecute = (c, item) =>
                    {
                        c.ShouldEqual(Command);
                        item.ShouldEqual(Command);
                        ++count;
                        return canExecute;
                    }
                });
            }

            Command.CanExecute(Command).ShouldBeFalse();
            count.ShouldEqual(1);
            count = 0;
            canExecute = true;
            Command.CanExecute(Command).ShouldBeTrue();
            count.ShouldEqual(componentCount);
        }

        [Theory]
        [InlineData(false, null, false, false, false, false)]
        [InlineData(true, true, true, true, true, true)]
        public void CreateFromTaskShouldGenerateValidRequest1(bool hasCanExecute, bool? allowMultipleExecution, bool hasThreadExecutionMode, bool addNotifiers, bool hasCanNotify,
            bool hasMetadata)
        {
            var owner = new object();
            Func<CancellationToken, IReadOnlyMetadataContext?, Task> execute = (c, m) => Task.CompletedTask;
            var canExecute = GetCanExecuteNoObject(hasCanExecute);
            var threadMode = hasThreadExecutionMode ? ThreadExecutionMode.Background : null;
            var notifiers = addNotifiers ? new[] { new object() } : null;
            var canNotify = GetHasCanNotify(hasCanNotify);
            var metadata = hasMetadata ? Metadata : null;

            object? r = null;
            CommandManager.AddComponent(new TestCommandProviderComponent
            {
                TryGetCommand = (manager, s, o, arg3) =>
                {
                    manager.ShouldEqual(CommandManager);
                    s.ShouldEqual(owner);
                    r = o;
                    arg3.ShouldEqual(metadata);
                    return new CompositeCommand();
                }
            });

            CompositeCommand.CreateFromTask(owner, execute, canExecute, notifiers, allowMultipleExecution, threadMode, canNotify, metadata, CommandManager);
            if (r is DelegateCommandRequest request)
            {
                request.Execute.ShouldEqual(execute);
                request.CanExecute.ShouldEqual(canExecute);
                request.AllowMultipleExecution.ShouldEqual(allowMultipleExecution);
                request.EventThreadMode.ShouldEqual(threadMode);
                request.Notifiers.ShouldEqual(notifiers);
                request.CanNotify.ShouldEqual(canNotify);
            }
            else
                DelegateCommandRequest.Get(execute, canExecute, allowMultipleExecution, threadMode, notifiers, canNotify).ShouldEqual(r);
        }

        [Theory]
        [InlineData(false, null, false, false, false, false)]
        [InlineData(true, true, true, true, true, true)]
        public void CreateFromTaskShouldGenerateValidRequest2(bool hasCanExecute, bool? allowMultipleExecution, bool hasThreadExecutionMode, bool addNotifiers, bool hasCanNotify,
            bool hasMetadata)
        {
            var owner = new object();
            Func<object?, CancellationToken, IReadOnlyMetadataContext?, Task> execute = (item, c, m) => Task.CompletedTask;
            var canExecute = GetCanExecute(hasCanExecute);
            var threadMode = hasThreadExecutionMode ? ThreadExecutionMode.Background : null;
            var notifiers = addNotifiers ? new[] { new object() } : null;
            var canNotify = GetHasCanNotify(hasCanNotify);
            var metadata = hasMetadata ? Metadata : null;

            object? r = null;
            CommandManager.AddComponent(new TestCommandProviderComponent
            {
                TryGetCommand = (manager, s, o, arg3) =>
                {
                    manager.ShouldEqual(CommandManager);
                    s.ShouldEqual(owner);
                    r = o;
                    arg3.ShouldEqual(metadata);
                    return new CompositeCommand();
                }
            });

            CompositeCommand.CreateFromTask(owner, execute, canExecute, notifiers, allowMultipleExecution, threadMode, canNotify, metadata, CommandManager);
            if (r is DelegateCommandRequest request)
            {
                request.Execute.ShouldEqual(execute);
                request.CanExecute.ShouldEqual(canExecute);
                request.AllowMultipleExecution.ShouldEqual(allowMultipleExecution);
                request.EventThreadMode.ShouldEqual(threadMode);
                request.Notifiers.ShouldEqual(notifiers);
                request.CanNotify.ShouldEqual(canNotify);
            }
            else
                DelegateCommandRequest.Get(execute, canExecute, allowMultipleExecution, threadMode, notifiers, canNotify).ShouldEqual(r);
        }

        [Theory]
        [InlineData(false, null, false, false, false, false)]
        [InlineData(true, true, true, true, true, true)]
        public void CreateShouldGenerateValidRequest1(bool hasCanExecute, bool? allowMultipleExecution, bool hasThreadExecutionMode, bool addNotifiers, bool hasCanNotify,
            bool hasMetadata)
        {
            var owner = new object();
            Action<IReadOnlyMetadataContext?> execute = m => { };
            var canExecute = GetCanExecuteNoObject(hasCanExecute);
            var threadMode = hasThreadExecutionMode ? ThreadExecutionMode.Background : null;
            var notifiers = addNotifiers ? new[] { new object() } : null;
            var canNotify = GetHasCanNotify(hasCanNotify);
            var metadata = hasMetadata ? Metadata : null;

            object? r = null;
            CommandManager.AddComponent(new TestCommandProviderComponent
            {
                TryGetCommand = (manager, s, o, m) =>
                {
                    manager.ShouldEqual(CommandManager);
                    s.ShouldEqual(owner);
                    r = o;
                    m.ShouldEqual(metadata);
                    return new CompositeCommand();
                }
            });

            CompositeCommand.Create(owner, execute, canExecute, notifiers, allowMultipleExecution, threadMode, canNotify, metadata, CommandManager);
            if (r is DelegateCommandRequest request)
            {
                request.Execute.ShouldEqual(execute);
                request.CanExecute.ShouldEqual(canExecute);
                request.AllowMultipleExecution.ShouldEqual(allowMultipleExecution);
                request.EventThreadMode.ShouldEqual(threadMode);
                request.Notifiers.ShouldEqual(notifiers);
                request.CanNotify.ShouldEqual(canNotify);
            }
            else
                DelegateCommandRequest.Get(execute, canExecute, allowMultipleExecution, threadMode, notifiers, canNotify).ShouldEqual(r);
        }

        [Theory]
        [InlineData(false, null, false, false, false, false)]
        [InlineData(true, true, true, true, true, true)]
        public void CreateShouldGenerateValidRequest2(bool hasCanExecute, bool? allowMultipleExecution, bool hasThreadExecutionMode, bool addNotifiers, bool hasCanNotify,
            bool hasMetadata)
        {
            var owner = new object();
            Action<object, IReadOnlyMetadataContext?> execute = (t, m) => { };
            var canExecute = GetCanExecute(hasCanExecute);
            var threadMode = hasThreadExecutionMode ? ThreadExecutionMode.Background : null;
            var notifiers = addNotifiers ? new[] { new object() } : null;
            var canNotify = GetHasCanNotify(hasCanNotify);
            var metadata = hasMetadata ? Metadata : null;

            object? r = null;
            CommandManager.AddComponent(new TestCommandProviderComponent
            {
                TryGetCommand = (manager, s, o, m) =>
                {
                    manager.ShouldEqual(CommandManager);
                    s.ShouldEqual(owner);
                    r = o;
                    m.ShouldEqual(metadata);
                    return new CompositeCommand();
                }
            });

            CompositeCommand.Create(owner, execute, canExecute, notifiers, allowMultipleExecution, threadMode, canNotify, metadata, CommandManager);
            if (r is DelegateCommandRequest request)
            {
                request.Execute.ShouldEqual(execute);
                request.CanExecute.ShouldEqual(canExecute);
                request.AllowMultipleExecution.ShouldEqual(allowMultipleExecution);
                request.EventThreadMode.ShouldEqual(threadMode);
                request.Notifiers.ShouldEqual(notifiers);
                request.CanNotify.ShouldEqual(canNotify);
            }
            else
                DelegateCommandRequest.Get(execute, canExecute, allowMultipleExecution, threadMode, notifiers, canNotify).ShouldEqual(r);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(10, true)]
        [InlineData(1, false)]
        [InlineData(10, false)]
        public void DisposeShouldBeHandledByComponents(int componentCount, bool canDispose)
        {
            var count = 0;
            Command.IsDisposable = canDispose;
            for (var i = 0; i < componentCount; i++)
            {
                Command.AddComponent(new TestDisposableComponent<ICompositeCommand>
                {
                    Dispose = (o, _) =>
                    {
                        o.ShouldEqual(Command);
                        ++count;
                    }
                });
            }

            Command.IsDisposed.ShouldBeFalse();
            Command.Metadata.Set(MetadataContextKey.FromKey<object?>("t"), "");
            Command.Dispose();
            if (canDispose)
            {
                Command.IsDisposed.ShouldBeTrue();
                count.ShouldEqual(componentCount);
                Command.Components.Count.ShouldEqual(0);
                Command.Metadata.Count.ShouldEqual(0);
            }
            else
            {
                Command.IsDisposed.ShouldBeFalse();
                count.ShouldEqual(0);
                Command.Components.Count.ShouldEqual(componentCount);
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public async Task ExecuteShouldBeHandledByComponents(int componentCount)
        {
            var invokeCount = 0;
            var tcs = new TaskCompletionSource<bool>[componentCount];
            for (var i = 0; i < componentCount; i++)
            {
                var tc = new TaskCompletionSource<bool>();
                tcs[i] = tc;
                Command.AddComponent(new TestCommandExecutorComponent
                {
                    ExecuteAsync = (cmd, p, c, m) =>
                    {
                        cmd.ShouldEqual(Command);
                        p.ShouldEqual(this);
                        c.ShouldEqual(DefaultCancellationToken);
                        m.ShouldEqual(Metadata);
                        ++invokeCount;
                        return tc.Task;
                    },
                    Priority = -i
                });
            }

            var task = Command.ExecuteAsync(this, DefaultCancellationToken, Metadata);
            for (var i = 0; i < tcs.Length; i++)
                tcs[i].SetResult(i == componentCount - 1);

            (await task).ShouldBeTrue();
            invokeCount.ShouldEqual(componentCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void RaiseCanExecuteChangedShouldBeHandledByComponents(int componentCount)
        {
            var count = 0;
            for (var i = 0; i < componentCount; i++)
            {
                Command.AddComponent(new TestCommandEventHandlerComponent
                {
                    RaiseCanExecuteChanged = c =>
                    {
                        c.ShouldEqual(Command);
                        ++count;
                    }
                });
            }

            Command.RaiseCanExecuteChanged();
            count.ShouldEqual(componentCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void RemoveCanExecuteChangedShouldBeHandledByComponents(int componentCount)
        {
            var count = 0;
            EventHandler eventHandler = (sender, args) => { };
            for (var i = 0; i < componentCount; i++)
            {
                Command.AddComponent(new TestCommandEventHandlerComponent
                {
                    RemoveCanExecuteChanged = (c, handler) =>
                    {
                        ++count;
                        c.ShouldEqual(Command);
                        handler.ShouldEqual(eventHandler);
                    }
                });
            }

            Command.CanExecuteChanged -= eventHandler;
            count.ShouldEqual(componentCount);
        }

        private static Func<object?, object?, bool>? GetHasCanNotify(bool value)
        {
            if (value)
                return (_, _) => true;
            return null;
        }

        private static Func<IReadOnlyMetadataContext?, bool>? GetCanExecuteNoObject(bool value)
        {
            if (value)
                return m => true;
            return null;
        }

        private static Func<object?, IReadOnlyMetadataContext?, bool>? GetCanExecute(bool value)
        {
            if (value)
                return (t, m) => true;
            return null;
        }

        protected override ICompositeCommand GetComponentOwner(IComponentCollectionManager? componentCollectionManager = null) =>
            new CompositeCommand(null, componentCollectionManager);
    }
}