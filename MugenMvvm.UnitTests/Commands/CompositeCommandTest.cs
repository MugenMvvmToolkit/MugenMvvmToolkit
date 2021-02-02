using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Commands;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTests.Commands.Internal;
using MugenMvvm.UnitTests.Internal;
using MugenMvvm.UnitTests.Models.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Commands
{
    [Collection(SharedContext)]
    public class CompositeCommandTest : SuspendableComponentOwnerTestBase<CompositeCommand>
    {
        public CompositeCommandTest()
        {
            MugenService.Configuration.InitializeInstance<ICommandManager>(new CommandManager());
        }

        private static Func<object?, object?, bool>? GetHasCanNotify(bool value)
        {
            if (value)
                return (_, _) => true;
            return null;
        }

        private static Func<bool>? GetCanExecuteNoObject(bool value)
        {
            if (value)
                return () => true;
            return null;
        }

        private static Func<object?, bool>? GetCanExecute(bool value)
        {
            if (value)
                return t => true;
            return null;
        }

        public override void Dispose()
        {
            MugenService.Configuration.Clear<ICommandManager>();
            base.Dispose();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void HasCanExecuteShouldBeHandledByComponents(int componentCount)
        {
            var count = 0;
            var canExecute = false;
            var compositeCommand = GetComponentOwner(ComponentCollectionManager);
            compositeCommand.HasCanExecute().ShouldBeFalse();
            for (var i = 0; i < componentCount; i++)
            {
                var component = new TestCommandConditionComponent
                {
                    HasCanExecute = c =>
                    {
                        ++count;
                        c.ShouldEqual(compositeCommand);
                        return canExecute;
                    }
                };
                compositeCommand.AddComponent(component);
            }

            compositeCommand.HasCanExecute().ShouldBeFalse();
            count.ShouldEqual(componentCount);

            count = 0;
            canExecute = true;
            compositeCommand.HasCanExecute().ShouldBeTrue();
            count.ShouldEqual(1);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void CanExecuteShouldBeHandledByComponents(int componentCount)
        {
            var count = 0;
            var canExecute = false;
            var compositeCommand = GetComponentOwner(ComponentCollectionManager);
            compositeCommand.CanExecute(compositeCommand).ShouldBeTrue();
            for (var i = 0; i < componentCount; i++)
            {
                var component = new TestCommandConditionComponent
                {
                    CanExecute = (c, item) =>
                    {
                        c.ShouldEqual(compositeCommand);
                        item.ShouldEqual(compositeCommand);
                        ++count;
                        return canExecute;
                    }
                };
                compositeCommand.AddComponent(component);
            }

            compositeCommand.CanExecute(compositeCommand).ShouldBeFalse();
            count.ShouldEqual(1);
            count = 0;
            canExecute = true;
            compositeCommand.CanExecute(compositeCommand).ShouldBeTrue();
            count.ShouldEqual(componentCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void AddCanExecuteChangedShouldBeHandledByComponents(int componentCount)
        {
            var count = 0;
            var compositeCommand = GetComponentOwner(ComponentCollectionManager);
            EventHandler eventHandler = (sender, args) => { };
            for (var i = 0; i < componentCount; i++)
            {
                var component = new TestCommandEventHandlerComponent
                {
                    AddCanExecuteChanged = (c, handler) =>
                    {
                        ++count;
                        c.ShouldEqual(compositeCommand);
                        handler.ShouldEqual(eventHandler);
                    }
                };
                compositeCommand.AddComponent(component);
            }

            compositeCommand.CanExecuteChanged += eventHandler;
            count.ShouldEqual(componentCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void RemoveCanExecuteChangedShouldBeHandledByComponents(int componentCount)
        {
            var count = 0;
            var compositeCommand = GetComponentOwner(ComponentCollectionManager);
            EventHandler eventHandler = (sender, args) => { };
            for (var i = 0; i < componentCount; i++)
            {
                var component = new TestCommandEventHandlerComponent
                {
                    RemoveCanExecuteChanged = (c, handler) =>
                    {
                        ++count;
                        c.ShouldEqual(compositeCommand);
                        handler.ShouldEqual(eventHandler);
                    }
                };
                compositeCommand.AddComponent(component);
            }

            compositeCommand.CanExecuteChanged -= eventHandler;
            count.ShouldEqual(componentCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void RaiseCanExecuteChangedShouldBeHandledByComponents(int componentCount)
        {
            var count = 0;
            var compositeCommand = GetComponentOwner(ComponentCollectionManager);
            for (var i = 0; i < componentCount; i++)
            {
                var component = new TestCommandEventHandlerComponent
                {
                    RaiseCanExecuteChanged = c =>
                    {
                        c.ShouldEqual(compositeCommand);
                        ++count;
                    }
                };
                compositeCommand.AddComponent(component);
            }

            compositeCommand.RaiseCanExecuteChanged();
            count.ShouldEqual(componentCount);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(10, true)]
        [InlineData(1, false)]
        [InlineData(10, false)]
        public void DisposeShouldBeHandledByComponents(int componentCount, bool canDispose)
        {
            var count = 0;
            var compositeCommand = GetComponentOwner(ComponentCollectionManager);
            compositeCommand.IsDisposable = canDispose;
            for (var i = 0; i < componentCount; i++)
            {
                var component = new TestDisposable {Dispose = () => ++count};
                compositeCommand.Components.TryAdd(component);
            }

            compositeCommand.IsDisposed.ShouldBeFalse();
            compositeCommand.Metadata.Set(MetadataContextKey.FromKey<object?>("t"), "");
            compositeCommand.Dispose();
            if (canDispose)
            {
                compositeCommand.IsDisposed.ShouldBeTrue();
                count.ShouldEqual(componentCount);
                compositeCommand.Components.Count.ShouldEqual(0);
                compositeCommand.Metadata.Count.ShouldEqual(0);
            }
            else
            {
                compositeCommand.IsDisposed.ShouldBeFalse();
                count.ShouldEqual(0);
                compositeCommand.Components.Count.ShouldEqual(componentCount);
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ExecuteShouldBeHandledByComponents(int componentCount)
        {
            var cts = new CancellationTokenSource().Token;
            var compositeCommand = GetComponentOwner(ComponentCollectionManager);
            var tcs = new List<TaskCompletionSource<object>>();
            for (var i = 0; i < componentCount; i++)
            {
                var component = new TestCommandExecutorComponent(compositeCommand)
                {
                    ExecuteAsync = (p, c, m) =>
                    {
                        p.ShouldEqual(compositeCommand);
                        c.ShouldEqual(cts);
                        m.ShouldEqual(DefaultMetadata);
                        var t = new TaskCompletionSource<object>();
                        tcs.Add(t);
                        return t.Task;
                    }
                };
                compositeCommand.AddComponent(component);
            }

            compositeCommand.ExecuteAsync(compositeCommand, cts, DefaultMetadata);
            tcs.Count.ShouldEqual(componentCount);
        }

        [Theory]
        [InlineData(false, null, null, false, false, false, false)]
        [InlineData(true, true, CommandExecutionBehavior.CheckCanExecuteValue, true, true, true, true)]
        public void CreateShouldGenerateValidRequest1(bool hasCanExecute, bool? allowMultipleExecution,
            int? executionModeValue, bool hasThreadExecutionMode, bool addNotifiers, bool hasCanNotify, bool hasMetadata)
        {
            var owner = new object();
            Action execute = () => { };
            var executionMode = executionModeValue == null ? null : CommandExecutionBehavior.Get(executionModeValue.Value);
            var canExecute = GetCanExecuteNoObject(hasCanExecute);
            var threadMode = hasThreadExecutionMode ? ThreadExecutionMode.Background : null;
            var notifiers = addNotifiers ? new[] {new object()} : null;
            var canNotify = GetHasCanNotify(hasCanNotify);
            var metadata = hasMetadata ? DefaultMetadata : null;

            object? r = null;
            MugenService.AddComponent(new TestCommandProviderComponent
            {
                TryGetCommand = (s, o, m) =>
                {
                    s.ShouldEqual(owner);
                    r = o;
                    m.ShouldEqual(metadata);
                    return new CompositeCommand();
                }
            });

            CompositeCommand.Create(owner, execute, canExecute, notifiers, allowMultipleExecution, executionMode, threadMode, canNotify, metadata);
            if (r is DelegateCommandRequest request)
            {
                request.Execute.ShouldEqual(execute);
                request.CanExecute.ShouldEqual(canExecute);
                request.AllowMultipleExecution.ShouldEqual(allowMultipleExecution);
                request.ExecutionMode.ShouldEqual(executionMode);
                request.EventThreadMode.ShouldEqual(threadMode);
                request.Notifiers.ShouldEqual(notifiers);
                request.CanNotify.ShouldEqual(canNotify);
            }
            else
                DelegateCommandRequest.Get(execute, canExecute, allowMultipleExecution, executionMode, threadMode, notifiers, canNotify).ShouldEqual(r);
        }

        [Theory]
        [InlineData(false, null, null, false, false, false, false)]
        [InlineData(true, true, CommandExecutionBehavior.CheckCanExecuteValue, true, true, true, true)]
        public void CreateShouldGenerateValidRequest2(bool hasCanExecute, bool? allowMultipleExecution,
            int? executionModeValue, bool hasThreadExecutionMode, bool addNotifiers, bool hasCanNotify, bool hasMetadata)
        {
            var owner = new object();
            Action<object> execute = t => { };
            var executionMode = executionModeValue == null ? null : CommandExecutionBehavior.Get(executionModeValue.Value);
            var canExecute = GetCanExecute(hasCanExecute);
            var threadMode = hasThreadExecutionMode ? ThreadExecutionMode.Background : null;
            var notifiers = addNotifiers ? new[] {new object()} : null;
            var canNotify = GetHasCanNotify(hasCanNotify);
            var metadata = hasMetadata ? DefaultMetadata : null;

            object? r = null;
            MugenService.AddComponent(new TestCommandProviderComponent
            {
                TryGetCommand = (s, o, m) =>
                {
                    s.ShouldEqual(owner);
                    r = o;
                    m.ShouldEqual(metadata);
                    return new CompositeCommand();
                }
            });

            CompositeCommand.Create(owner, execute, canExecute, notifiers, allowMultipleExecution, executionMode, threadMode, canNotify, metadata);
            if (r is DelegateCommandRequest request)
            {
                request.Execute.ShouldEqual(execute);
                request.CanExecute.ShouldEqual(canExecute);
                request.AllowMultipleExecution.ShouldEqual(allowMultipleExecution);
                request.ExecutionMode.ShouldEqual(executionMode);
                request.EventThreadMode.ShouldEqual(threadMode);
                request.Notifiers.ShouldEqual(notifiers);
                request.CanNotify.ShouldEqual(canNotify);
            }
            else
                DelegateCommandRequest.Get(execute, canExecute, allowMultipleExecution, executionMode, threadMode, notifiers, canNotify).ShouldEqual(r);
        }

        [Theory]
        [InlineData(false, null, null, false, false, false, false)]
        [InlineData(true, true, CommandExecutionBehavior.CheckCanExecuteValue, true, true, true, true)]
        public void CreateFromTaskShouldGenerateValidRequest1(bool hasCanExecute, bool? allowMultipleExecution,
            int? executionModeValue, bool hasThreadExecutionMode, bool addNotifiers, bool hasCanNotify, bool hasMetadata)
        {
            var owner = new object();
            Func<Task> execute = () => Default.CompletedTask;
            var executionMode = executionModeValue == null ? null : CommandExecutionBehavior.Get(executionModeValue.Value);
            var canExecute = GetCanExecuteNoObject(hasCanExecute);
            var threadMode = hasThreadExecutionMode ? ThreadExecutionMode.Background : null;
            var notifiers = addNotifiers ? new[] {new object()} : null;
            var canNotify = GetHasCanNotify(hasCanNotify);
            var metadata = hasMetadata ? DefaultMetadata : null;

            object? r = null;
            MugenService.AddComponent(new TestCommandProviderComponent
            {
                TryGetCommand = (s, o, arg3) =>
                {
                    s.ShouldEqual(owner);
                    r = o;
                    arg3.ShouldEqual(metadata);
                    return new CompositeCommand();
                }
            });

            CompositeCommand.CreateFromTask(owner, execute, canExecute, notifiers, allowMultipleExecution, executionMode, threadMode, canNotify, metadata);
            if (r is DelegateCommandRequest request)
            {
                request.Execute.ShouldEqual(execute);
                request.CanExecute.ShouldEqual(canExecute);
                request.AllowMultipleExecution.ShouldEqual(allowMultipleExecution);
                request.ExecutionMode.ShouldEqual(executionMode);
                request.EventThreadMode.ShouldEqual(threadMode);
                request.Notifiers.ShouldEqual(notifiers);
                request.CanNotify.ShouldEqual(canNotify);
            }
            else
                DelegateCommandRequest.Get(execute, canExecute, allowMultipleExecution, executionMode, threadMode, notifiers, canNotify).ShouldEqual(r);
        }

        [Theory]
        [InlineData(false, null, null, false, false, false, false)]
        [InlineData(true, true, CommandExecutionBehavior.CheckCanExecuteValue, true, true, true, true)]
        public void CreateFromTaskShouldGenerateValidRequest2(bool hasCanExecute, bool? allowMultipleExecution,
            int? executionModeValue, bool hasThreadExecutionMode, bool addNotifiers, bool hasCanNotify, bool hasMetadata)
        {
            var owner = new object();
            Func<object?, Task> execute = item => Default.CompletedTask;
            var executionMode = executionModeValue == null ? null : CommandExecutionBehavior.Get(executionModeValue.Value);
            var canExecute = GetCanExecute(hasCanExecute);
            var threadMode = hasThreadExecutionMode ? ThreadExecutionMode.Background : null;
            var notifiers = addNotifiers ? new[] {new object()} : null;
            var canNotify = GetHasCanNotify(hasCanNotify);
            var metadata = hasMetadata ? DefaultMetadata : null;

            object? r = null;
            MugenService.AddComponent(new TestCommandProviderComponent
            {
                TryGetCommand = (s, o, arg3) =>
                {
                    s.ShouldEqual(owner);
                    r = o;
                    arg3.ShouldEqual(metadata);
                    return new CompositeCommand();
                }
            });

            CompositeCommand.CreateFromTask(owner, execute, canExecute, notifiers, allowMultipleExecution, executionMode, threadMode, canNotify, metadata);
            if (r is DelegateCommandRequest request)
            {
                request.Execute.ShouldEqual(execute);
                request.CanExecute.ShouldEqual(canExecute);
                request.AllowMultipleExecution.ShouldEqual(allowMultipleExecution);
                request.ExecutionMode.ShouldEqual(executionMode);
                request.EventThreadMode.ShouldEqual(threadMode);
                request.Notifiers.ShouldEqual(notifiers);
                request.CanNotify.ShouldEqual(canNotify);
            }
            else
                DelegateCommandRequest.Get(execute, canExecute, allowMultipleExecution, executionMode, threadMode, notifiers, canNotify).ShouldEqual(r);
        }

        protected override CompositeCommand GetComponentOwner(IComponentCollectionManager? componentCollectionManager = null) => new(null, componentCollectionManager);
    }
}