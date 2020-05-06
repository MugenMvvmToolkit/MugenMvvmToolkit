using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MugenMvvm.Commands;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.UnitTest.Commands.Internal;
using MugenMvvm.UnitTest.Internal;
using MugenMvvm.UnitTest.Internal.Internal;
using MugenMvvm.UnitTest.Models;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Commands
{
    public class CompositeCommandTest : SuspendableComponentOwnerTestBase<CompositeCommand>
    {
        #region Methods

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void HasCanExecuteShouldBeHandledByComponents(int componentCount)
        {
            var count = 0;
            var canExecute = false;
            var compositeCommand = GetComponentOwner();
            compositeCommand.HasCanExecute.ShouldBeFalse();
            for (var i = 0; i < componentCount; i++)
            {
                var component = new TestConditionCommandComponent
                {
                    HasCanExecute = () =>
                    {
                        ++count;
                        return canExecute;
                    }
                };
                compositeCommand.AddComponent(component);
            }

            compositeCommand.HasCanExecute.ShouldBeFalse();
            count.ShouldEqual(componentCount);

            count = 0;
            canExecute = true;
            compositeCommand.HasCanExecute.ShouldBeTrue();
            count.ShouldEqual(1);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void CanExecuteShouldBeHandledByComponents(int componentCount)
        {
            var count = 0;
            var canExecute = false;
            var compositeCommand = GetComponentOwner();
            compositeCommand.CanExecute(compositeCommand).ShouldBeTrue();
            for (var i = 0; i < componentCount; i++)
            {
                var component = new TestConditionCommandComponent
                {
                    CanExecute = item =>
                    {
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
            var compositeCommand = GetComponentOwner();
            EventHandler eventHandler = (sender, args) => { };
            for (var i = 0; i < componentCount; i++)
            {
                var component = new TestConditionEventCommandComponent
                {
                    AddCanExecuteChanged = handler =>
                    {
                        ++count;
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
            var compositeCommand = GetComponentOwner();
            EventHandler eventHandler = (sender, args) => { };
            for (var i = 0; i < componentCount; i++)
            {
                var component = new TestConditionEventCommandComponent
                {
                    RemoveCanExecuteChanged = handler =>
                    {
                        ++count;
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
            var compositeCommand = GetComponentOwner();
            for (var i = 0; i < componentCount; i++)
            {
                var component = new TestConditionEventCommandComponent { RaiseCanExecuteChanged = () => ++count };
                compositeCommand.AddComponent(component);
            }

            compositeCommand.RaiseCanExecuteChanged();
            count.ShouldEqual(componentCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void DisposeShouldBeHandledByComponents(int componentCount)
        {
            var count = 0;
            var compositeCommand = GetComponentOwner();
            for (var i = 0; i < componentCount; i++)
            {
                var component = new TestDisposable { Dispose = () => ++count };
                compositeCommand.Components.Add(component);
            }

            compositeCommand.Dispose();
            count.ShouldEqual(componentCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ExecuteShouldBeHandledByComponents(int componentCount)
        {
            var compositeCommand = GetComponentOwner();
            var tcs = new List<TaskCompletionSource<object>>();
            for (var i = 0; i < componentCount; i++)
            {
                var component = new TestExecutorCommandComponent
                {
                    ExecuteAsync = o =>
                    {
                        o.ShouldEqual(compositeCommand);
                        var t = new TaskCompletionSource<object>();
                        tcs.Add(t);
                        return t.Task;
                    }
                };
                compositeCommand.AddComponent(component);
            }

            compositeCommand.Execute(compositeCommand);
            tcs.Count.ShouldEqual(componentCount);
        }

        [Theory]
        [InlineData(false, null, null, false, false, false, false)]
        [InlineData(true, true, CommandExecutionMode.CanExecuteBeforeExecute, true, true, true, true)]
        public void CreateShouldGenerateValidRequest1(bool hasCanExecute, bool? allowMultipleExecution,
            CommandExecutionMode? executionMode, bool hasThreadExecutionMode, bool addNotifiers, bool hasCanNotify, bool hasMetadata)
        {
            Action execute = () => { };
            var canExecute = GetCanExecuteNoObject(hasCanExecute);
            var threadMode = hasThreadExecutionMode ? ThreadExecutionMode.Background : null;
            var notifiers = addNotifiers ? new[] { new object() } : null;
            var canNotify = GetHasCanNotify(hasCanNotify);
            var metadata = hasMetadata ? DefaultMetadata : null;

            DelegateCommandRequest request = default;
            var component = new TestCommandProviderComponent
            {
                TryGetCommand = (o, type, arg3) =>
                {
                    request = (DelegateCommandRequest)o!;
                    type.ShouldEqual(typeof(DelegateCommandRequest));
                    arg3.ShouldEqual(metadata);
                    return new CompositeCommand();
                }
            };
            using var subscriber = TestComponentSubscriber.Subscribe(component);

            CompositeCommand.Create(execute, canExecute, allowMultipleExecution, executionMode, threadMode, notifiers, canNotify, metadata);
            request.IsEmpty.ShouldBeFalse();
            request.Execute.ShouldEqual(execute);
            request.CanExecute.ShouldEqual(canExecute);
            request.AllowMultipleExecution.ShouldEqual(allowMultipleExecution);
            request.ExecutionMode.ShouldEqual(executionMode);
            request.EventThreadMode.ShouldEqual(threadMode);
            request.Notifiers.ShouldEqual(notifiers);
            request.CanNotify.ShouldEqual(canNotify);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void CreateShouldGenerateValidRequest2(bool addNotifiers)
        {
            Action execute = () => { };
            var canExecute = GetCanExecuteNoObject(true);
            var notifiers = addNotifiers ? new[] { new object() } : null;

            DelegateCommandRequest request = default;
            var component = new TestCommandProviderComponent
            {
                TryGetCommand = (o, type, arg3) =>
                {
                    request = (DelegateCommandRequest)o!;
                    type.ShouldEqual(typeof(DelegateCommandRequest));
                    return new CompositeCommand();
                }
            };
            using var subscriber = TestComponentSubscriber.Subscribe(component);

            CompositeCommand.Create(execute, canExecute!, notifiers!);
            request.IsEmpty.ShouldBeFalse();
            request.Execute.ShouldEqual(execute);
            request.CanExecute.ShouldEqual(canExecute);
            request.Notifiers.ShouldEqual(notifiers);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, true)]
        public void CreateShouldGenerateValidRequest3(bool allowMultipleExecution, bool addNotifiers)
        {
            Action execute = () => { };
            var canExecute = GetCanExecuteNoObject(true);
            var notifiers = addNotifiers ? new[] { new object() } : null;

            DelegateCommandRequest request = default;
            var component = new TestCommandProviderComponent
            {
                TryGetCommand = (o, type, arg3) =>
                {
                    request = (DelegateCommandRequest)o!;
                    type.ShouldEqual(typeof(DelegateCommandRequest));
                    return new CompositeCommand();
                }
            };
            using var subscriber = TestComponentSubscriber.Subscribe(component);

            CompositeCommand.Create(execute, allowMultipleExecution, canExecute!, notifiers!);
            request.IsEmpty.ShouldBeFalse();
            request.Execute.ShouldEqual(execute);
            request.CanExecute.ShouldEqual(canExecute);
            request.AllowMultipleExecution.ShouldEqual(allowMultipleExecution);
            request.Notifiers.ShouldEqual(notifiers);
        }

        [Theory]
        [InlineData(false, null, null, false, false, false, false)]
        [InlineData(true, true, CommandExecutionMode.CanExecuteBeforeExecute, true, true, true, true)]
        public void CreateShouldGenerateValidRequest4(bool hasCanExecute, bool? allowMultipleExecution,
            CommandExecutionMode? executionMode, bool hasThreadExecutionMode, bool addNotifiers, bool hasCanNotify, bool hasMetadata)
        {
            Action<object> execute = t => { };
            var canExecute = GetCanExecute(hasCanExecute);
            var threadMode = hasThreadExecutionMode ? ThreadExecutionMode.Background : null;
            var notifiers = addNotifiers ? new[] { new object() } : null;
            var canNotify = GetHasCanNotify(hasCanNotify);
            var metadata = hasMetadata ? DefaultMetadata : null;

            DelegateCommandRequest request = default;
            var component = new TestCommandProviderComponent
            {
                TryGetCommand = (o, type, arg3) =>
                {
                    request = (DelegateCommandRequest)o!;
                    type.ShouldEqual(typeof(DelegateCommandRequest));
                    arg3.ShouldEqual(metadata);
                    return new CompositeCommand();
                }
            };
            using var subscriber = TestComponentSubscriber.Subscribe(component);

            CompositeCommand.Create(execute, canExecute, allowMultipleExecution, executionMode, threadMode, notifiers, canNotify, metadata);
            request.IsEmpty.ShouldBeFalse();
            request.Execute.ShouldEqual(execute);
            request.CanExecute.ShouldEqual(canExecute);
            request.AllowMultipleExecution.ShouldEqual(allowMultipleExecution);
            request.ExecutionMode.ShouldEqual(executionMode);
            request.EventThreadMode.ShouldEqual(threadMode);
            request.Notifiers.ShouldEqual(notifiers);
            request.CanNotify.ShouldEqual(canNotify);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void CreateShouldGenerateValidRequest5(bool addNotifiers)
        {
            Action<object> execute = t => { };
            var canExecute = GetCanExecute(true);
            var notifiers = addNotifiers ? new[] { new object() } : null;

            DelegateCommandRequest request = default;
            var component = new TestCommandProviderComponent
            {
                TryGetCommand = (o, type, arg3) =>
                {
                    request = (DelegateCommandRequest)o!;
                    type.ShouldEqual(typeof(DelegateCommandRequest));
                    return new CompositeCommand();
                }
            };
            using var subscriber = TestComponentSubscriber.Subscribe(component);

            CompositeCommand.Create(execute, canExecute!, notifiers!);
            request.IsEmpty.ShouldBeFalse();
            request.Execute.ShouldEqual(execute);
            request.CanExecute.ShouldEqual(canExecute);
            request.Notifiers.ShouldEqual(notifiers);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, true)]
        public void CreateShouldGenerateValidRequest6(bool allowMultipleExecution, bool addNotifiers)
        {
            Action<object> execute = t => { };
            var canExecute = GetCanExecute(true);
            var notifiers = addNotifiers ? new[] { new object() } : null;

            DelegateCommandRequest request = default;
            var component = new TestCommandProviderComponent
            {
                TryGetCommand = (o, type, arg3) =>
                {
                    request = (DelegateCommandRequest)o!;
                    type.ShouldEqual(typeof(DelegateCommandRequest));
                    return new CompositeCommand();
                }
            };
            using var subscriber = TestComponentSubscriber.Subscribe(component);

            CompositeCommand.Create(execute, allowMultipleExecution, canExecute!, notifiers!);
            request.IsEmpty.ShouldBeFalse();
            request.Execute.ShouldEqual(execute);
            request.CanExecute.ShouldEqual(canExecute);
            request.AllowMultipleExecution.ShouldEqual(allowMultipleExecution);
            request.Notifiers.ShouldEqual(notifiers);
        }

        [Theory]
        [InlineData(false, null, null, false, false, false, false)]
        [InlineData(true, true, CommandExecutionMode.CanExecuteBeforeExecute, true, true, true, true)]
        public void CreateFromTaskShouldGenerateValidRequest1(bool hasCanExecute, bool? allowMultipleExecution,
            CommandExecutionMode? executionMode, bool hasThreadExecutionMode, bool addNotifiers, bool hasCanNotify, bool hasMetadata)
        {
            Func<Task> execute = () => Task.CompletedTask;
            var canExecute = GetCanExecuteNoObject(hasCanExecute);
            var threadMode = hasThreadExecutionMode ? ThreadExecutionMode.Background : null;
            var notifiers = addNotifiers ? new[] { new object() } : null;
            var canNotify = GetHasCanNotify(hasCanNotify);
            var metadata = hasMetadata ? DefaultMetadata : null;

            DelegateCommandRequest request = default;
            var component = new TestCommandProviderComponent
            {
                TryGetCommand = (o, type, arg3) =>
                {
                    request = (DelegateCommandRequest)o!;
                    type.ShouldEqual(typeof(DelegateCommandRequest));
                    arg3.ShouldEqual(metadata);
                    return new CompositeCommand();
                }
            };
            using var subscriber = TestComponentSubscriber.Subscribe(component);

            CompositeCommand.CreateFromTask(execute, canExecute, allowMultipleExecution, executionMode, threadMode, notifiers, canNotify, metadata);
            request.IsEmpty.ShouldBeFalse();
            request.Execute.ShouldEqual(execute);
            request.CanExecute.ShouldEqual(canExecute);
            request.AllowMultipleExecution.ShouldEqual(allowMultipleExecution);
            request.ExecutionMode.ShouldEqual(executionMode);
            request.EventThreadMode.ShouldEqual(threadMode);
            request.Notifiers.ShouldEqual(notifiers);
            request.CanNotify.ShouldEqual(canNotify);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void CreateFromTaskShouldGenerateValidRequest2(bool addNotifiers)
        {
            Func<Task> execute = () => Task.CompletedTask;
            var canExecute = GetCanExecuteNoObject(true);
            var notifiers = addNotifiers ? new[] { new object() } : null;

            DelegateCommandRequest request = default;
            var component = new TestCommandProviderComponent
            {
                TryGetCommand = (o, type, arg3) =>
                {
                    request = (DelegateCommandRequest)o!;
                    type.ShouldEqual(typeof(DelegateCommandRequest));
                    return new CompositeCommand();
                }
            };
            using var subscriber = TestComponentSubscriber.Subscribe(component);

            CompositeCommand.CreateFromTask(execute, canExecute!, notifiers!);
            request.IsEmpty.ShouldBeFalse();
            request.Execute.ShouldEqual(execute);
            request.CanExecute.ShouldEqual(canExecute);
            request.Notifiers.ShouldEqual(notifiers);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, true)]
        public void CreateFromTaskShouldGenerateValidRequest3(bool allowMultipleExecution, bool addNotifiers)
        {
            Func<Task> execute = () => Task.CompletedTask;
            var canExecute = GetCanExecuteNoObject(true);
            var notifiers = addNotifiers ? new[] { new object() } : null;

            DelegateCommandRequest request = default;
            var component = new TestCommandProviderComponent
            {
                TryGetCommand = (o, type, arg3) =>
                {
                    request = (DelegateCommandRequest)o!;
                    type.ShouldEqual(typeof(DelegateCommandRequest));
                    return new CompositeCommand();
                }
            };
            using var subscriber = TestComponentSubscriber.Subscribe(component);

            CompositeCommand.CreateFromTask(execute, allowMultipleExecution, canExecute!, notifiers!);
            request.IsEmpty.ShouldBeFalse();
            request.Execute.ShouldEqual(execute);
            request.CanExecute.ShouldEqual(canExecute);
            request.AllowMultipleExecution.ShouldEqual(allowMultipleExecution);
            request.Notifiers.ShouldEqual(notifiers);
        }

        [Theory]
        [InlineData(false, null, null, false, false, false, false)]
        [InlineData(true, true, CommandExecutionMode.CanExecuteBeforeExecute, true, true, true, true)]
        public void CreateFromTaskShouldGenerateValidRequest4(bool hasCanExecute, bool? allowMultipleExecution,
            CommandExecutionMode? executionMode, bool hasThreadExecutionMode, bool addNotifiers, bool hasCanNotify, bool hasMetadata)
        {
            Func<object?, Task> execute = item => Task.CompletedTask;
            var canExecute = GetCanExecute(hasCanExecute);
            var threadMode = hasThreadExecutionMode ? ThreadExecutionMode.Background : null;
            var notifiers = addNotifiers ? new[] { new object() } : null;
            var canNotify = GetHasCanNotify(hasCanNotify);
            var metadata = hasMetadata ? DefaultMetadata : null;

            DelegateCommandRequest request = default;
            var component = new TestCommandProviderComponent
            {
                TryGetCommand = (o, type, arg3) =>
                {
                    request = (DelegateCommandRequest)o!;
                    type.ShouldEqual(typeof(DelegateCommandRequest));
                    arg3.ShouldEqual(metadata);
                    return new CompositeCommand();
                }
            };
            using var subscriber = TestComponentSubscriber.Subscribe(component);

            CompositeCommand.CreateFromTask(execute, canExecute, allowMultipleExecution, executionMode, threadMode, notifiers, canNotify, metadata);
            request.IsEmpty.ShouldBeFalse();
            request.Execute.ShouldEqual(execute);
            request.CanExecute.ShouldEqual(canExecute);
            request.AllowMultipleExecution.ShouldEqual(allowMultipleExecution);
            request.ExecutionMode.ShouldEqual(executionMode);
            request.EventThreadMode.ShouldEqual(threadMode);
            request.Notifiers.ShouldEqual(notifiers);
            request.CanNotify.ShouldEqual(canNotify);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void CreateFromTaskShouldGenerateValidRequest5(bool addNotifiers)
        {
            Func<object?, Task> execute = item => Task.CompletedTask;
            var canExecute = GetCanExecute(true);
            var notifiers = addNotifiers ? new[] { new object() } : null;

            DelegateCommandRequest request = default;
            var component = new TestCommandProviderComponent
            {
                TryGetCommand = (o, type, arg3) =>
                {
                    request = (DelegateCommandRequest)o!;
                    type.ShouldEqual(typeof(DelegateCommandRequest));
                    return new CompositeCommand();
                }
            };
            using var subscriber = TestComponentSubscriber.Subscribe(component);

            CompositeCommand.CreateFromTask(execute, canExecute!, notifiers!);
            request.IsEmpty.ShouldBeFalse();
            request.Execute.ShouldEqual(execute);
            request.CanExecute.ShouldEqual(canExecute);
            request.Notifiers.ShouldEqual(notifiers);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, true)]
        public void CreateFromTaskShouldGenerateValidRequest6(bool allowMultipleExecution, bool addNotifiers)
        {
            Func<object?, Task> execute = item => Task.CompletedTask;
            var canExecute = GetCanExecute(true);
            var notifiers = addNotifiers ? new[] { new object() } : null;

            DelegateCommandRequest request = default;
            var component = new TestCommandProviderComponent
            {
                TryGetCommand = (o, type, arg3) =>
                {
                    request = (DelegateCommandRequest)o!;
                    type.ShouldEqual(typeof(DelegateCommandRequest));
                    return new CompositeCommand();
                }
            };
            using var subscriber = TestComponentSubscriber.Subscribe(component);

            CompositeCommand.CreateFromTask(execute, allowMultipleExecution, canExecute!, notifiers!);
            request.IsEmpty.ShouldBeFalse();
            request.Execute.ShouldEqual(execute);
            request.CanExecute.ShouldEqual(canExecute);
            request.AllowMultipleExecution.ShouldEqual(allowMultipleExecution);
            request.Notifiers.ShouldEqual(notifiers);
        }

        private static Func<object, bool>? GetHasCanNotify(bool value)
        {
            if (value)
                return i => true;
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

        protected override CompositeCommand GetComponentOwner(IComponentCollectionProvider? collectionProvider = null)
        {
            return new CompositeCommand(collectionProvider);
        }

        #endregion
    }
}