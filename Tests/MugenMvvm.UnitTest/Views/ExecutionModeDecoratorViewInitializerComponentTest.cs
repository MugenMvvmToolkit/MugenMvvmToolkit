using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Extensions;
using MugenMvvm.Threading;
using MugenMvvm.UnitTest.Threading;
using MugenMvvm.UnitTest.ViewModels;
using MugenMvvm.Views;
using MugenMvvm.Views.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Views
{
    public class ExecutionModeDecoratorViewInitializerComponentTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryInitializeAsyncShouldBeExecutedInline()
        {
            var dispatcher = new ThreadDispatcher();
            var component = new ExecutionModeDecoratorViewInitializerComponent(dispatcher);
            var mapping = new ViewModelViewMapping("id", typeof(object), typeof(TestViewModel), DefaultMetadata);
            var view = new object();
            var viewModel = new TestViewModel();
            var result = Task.FromResult(new ViewInitializationResult());
            var cancellationToken = new CancellationTokenSource().Token;
            Action? action = null;
            dispatcher.AddComponent(new TestThreadDispatcherComponent
            {
                CanExecuteInline = (mode, context) =>
                {
                    mode.ShouldEqual(component.InitializeExecutionMode);
                    return true;
                },
                Execute = (a, mode, arg3, arg4) =>
                {
                    action += () => a(arg3);
                    return true;
                }
            });
            var manager = new ViewManager();
            manager.AddComponent(component);
            manager.AddComponent(new TestViewInitializerComponent
            {
                TryInitializeAsync = (viewMapping, v, vm, meta, token) =>
                {
                    viewMapping.ShouldEqual(mapping);
                    v.ShouldEqual(view);
                    vm.ShouldEqual(viewModel);
                    meta.ShouldEqual(DefaultMetadata);
                    token.ShouldEqual(cancellationToken);
                    return result;
                }
            });

            manager.InitializeAsync(mapping, view, viewModel, DefaultMetadata, cancellationToken).ShouldEqual(result);
            action.ShouldBeNull();
        }

        [Theory]
        [InlineData(0)]//success
        [InlineData(1)]//canceled
        [InlineData(2)]//error
        public void TryInitializeAsyncShouldUseThreadDispatcher(int state)
        {
            var dispatcher = new ThreadDispatcher();
            var component = new ExecutionModeDecoratorViewInitializerComponent(dispatcher);
            var mapping = new ViewModelViewMapping("id", typeof(object), typeof(TestViewModel), DefaultMetadata);
            var view = new object();
            var viewModel = new TestViewModel();
            var result = Task.FromResult(new ViewInitializationResult(new View(mapping, view), viewModel, DefaultMetadata));
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            var ex = new Exception();
            Action? action = null;
            dispatcher.AddComponent(new TestThreadDispatcherComponent
            {
                CanExecuteInline = (mode, context) =>
                {
                    mode.ShouldEqual(component.InitializeExecutionMode);
                    return false;
                },
                Execute = (a, mode, arg3, arg4) =>
                {
                    mode.ShouldEqual(component.InitializeExecutionMode);
                    action += () => a(arg3);
                    return true;
                }
            });
            var manager = new ViewManager();
            manager.AddComponent(component);
            manager.AddComponent(new TestViewInitializerComponent
            {
                TryInitializeAsync = (viewMapping, v, vm, meta, token) =>
                {
                    viewMapping.ShouldEqual(mapping);
                    v.ShouldEqual(view);
                    vm.ShouldEqual(viewModel);
                    meta.ShouldEqual(DefaultMetadata);
                    token.ShouldEqual(cancellationToken);
                    if (state == 2)
                        throw ex;
                    return result;
                }
            });

            if (state == 1)
                cancellationTokenSource.Cancel();
            var task = manager.InitializeAsync(mapping, view, viewModel, DefaultMetadata, cancellationToken);
            task.IsCompleted.ShouldBeFalse();
            action!();
            switch (state)
            {
                case 1:
                    task.IsCanceled.ShouldBeTrue();
                    break;
                case 2:
                    task.IsFaulted.ShouldBeTrue();
                    task.Exception.GetBaseException().ShouldEqual(ex);
                    break;
                default:
                    task.IsCompleted.ShouldBeTrue();
                    task.Result.ShouldEqual(result.Result);
                    break;
            }
        }

        [Fact]
        public void TryCleanupAsyncShouldBeExecutedInline()
        {
            var dispatcher = new ThreadDispatcher();
            var component = new ExecutionModeDecoratorViewInitializerComponent(dispatcher);
            var mapping = new ViewModelViewMapping("id", typeof(object), typeof(TestViewModel), DefaultMetadata);
            var view = new View(mapping, new object());
            var viewModel = new TestViewModel();
            var result = Task.FromResult(new ViewInitializationResult());
            var cancellationToken = new CancellationTokenSource().Token;
            Action? action = null;
            dispatcher.AddComponent(new TestThreadDispatcherComponent
            {
                CanExecuteInline = (mode, context) =>
                {
                    mode.ShouldEqual(component.CleanupExecutionMode);
                    return true;
                },
                Execute = (a, mode, arg3, arg4) =>
                {
                    action += () => a(arg3);
                    return true;
                }
            });
            var manager = new ViewManager();
            manager.AddComponent(component);
            manager.AddComponent(new TestViewInitializerComponent
            {
                TryCleanupAsync = (v, vm, meta, token) =>
                {
                    v.ShouldEqual(view);
                    vm.ShouldEqual(viewModel);
                    meta.ShouldEqual(DefaultMetadata);
                    token.ShouldEqual(cancellationToken);
                    return result;
                }
            });

            manager.CleanupAsync(view, viewModel, DefaultMetadata, cancellationToken).ShouldEqual(result);
            action.ShouldBeNull();
        }

        [Theory]
        [InlineData(0)]//success
        [InlineData(1)]//canceled
        [InlineData(2)]//error
        public void TryCleanupAsyncShouldUseThreadDispatcher(int state)
        {
            var dispatcher = new ThreadDispatcher();
            var component = new ExecutionModeDecoratorViewInitializerComponent(dispatcher);
            var mapping = new ViewModelViewMapping("id", typeof(object), typeof(TestViewModel), DefaultMetadata);
            var view = new View(mapping, new object());
            var viewModel = new TestViewModel();
            var result = Task.FromResult(new ViewInitializationResult(new View(mapping, view), viewModel, DefaultMetadata));
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            var ex = new Exception();
            Action? action = null;
            dispatcher.AddComponent(new TestThreadDispatcherComponent
            {
                CanExecuteInline = (mode, context) =>
                {
                    mode.ShouldEqual(component.CleanupExecutionMode);
                    return false;
                },
                Execute = (a, mode, arg3, arg4) =>
                {
                    mode.ShouldEqual(component.CleanupExecutionMode);
                    action += () => a(arg3);
                    return true;
                }
            });
            var manager = new ViewManager();
            manager.AddComponent(component);
            manager.AddComponent(new TestViewInitializerComponent
            {
                TryCleanupAsync = (v, vm, meta, token) =>
                {
                    v.ShouldEqual(view);
                    vm.ShouldEqual(viewModel);
                    meta.ShouldEqual(DefaultMetadata);
                    token.ShouldEqual(cancellationToken);
                    if (state == 2)
                        throw ex;
                    return result;
                }
            });

            if (state == 1)
                cancellationTokenSource.Cancel();
            var task = manager.CleanupAsync(view, viewModel, DefaultMetadata, cancellationToken);
            task.IsCompleted.ShouldBeFalse();
            action!();
            switch (state)
            {
                case 1:
                    task.IsCanceled.ShouldBeTrue();
                    break;
                case 2:
                    task.IsFaulted.ShouldBeTrue();
                    task.Exception.GetBaseException().ShouldEqual(ex);
                    break;
                default:
                    task.IsCompleted.ShouldBeTrue();
                    task.IsCanceled.ShouldBeFalse();
                    break;
            }
        }

        #endregion
    }
}