using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Internal;
using MugenMvvm.Threading;
using MugenMvvm.UnitTests.Threading.Internal;
using MugenMvvm.UnitTests.ViewModels.Internal;
using MugenMvvm.UnitTests.Views.Internal;
using MugenMvvm.Views;
using MugenMvvm.Views.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Views.Components
{
    public class ExecutionModeViewManagerDecoratorTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public async Task TryInitializeAsyncShouldBeExecutedInline()
        {
            var dispatcher = new ThreadDispatcher();
            var component = new ExecutionModeViewManagerDecorator(dispatcher);
            var mapping = new ViewMapping("id", typeof(TestViewModel), typeof(object), DefaultMetadata);
            var viewModel = new TestViewModel();
            var result = new ValueTask<IView?>(new View(new ViewMapping("id", typeof(TestViewModel), typeof(object), DefaultMetadata), this, new TestViewModel()));
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
            manager.AddComponent(new TestViewManagerComponent(manager)
            {
                TryInitializeAsync = (viewMapping, r, meta, token) =>
                {
                    viewMapping.ShouldEqual(mapping);
                    r.ShouldEqual(viewModel);
                    meta.ShouldEqual(DefaultMetadata);
                    token.ShouldEqual(cancellationToken);
                    return result;
                }
            });

            (await manager.InitializeAsync(mapping, viewModel, cancellationToken, DefaultMetadata)).ShouldEqual(result.Result);
            action.ShouldBeNull();
        }

        [Theory]
        [InlineData(0)] //success
        [InlineData(1)] //canceled
        [InlineData(2)] //error
        public async Task TryInitializeAsyncShouldUseThreadDispatcher(int state)
        {
            var dispatcher = new ThreadDispatcher();
            var component = new ExecutionModeViewManagerDecorator(dispatcher);
            var mapping = new ViewMapping("id", typeof(TestViewModel), typeof(object), DefaultMetadata);
            var viewModel = new TestViewModel();
            var result = new ValueTask<IView?>(new View(new ViewMapping("id", typeof(TestViewModel), typeof(object), DefaultMetadata), this, new TestViewModel()));
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
            manager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (viewMapping, r, meta, token) =>
                {
                    viewMapping.ShouldEqual(mapping);
                    r.ShouldEqual(viewModel);
                    meta.ShouldEqual(DefaultMetadata);
                    token.ShouldEqual(cancellationToken);
                    if (state == 2)
                        throw ex;
                    return result;
                }
            });

            var task = manager.InitializeAsync(mapping, viewModel, cancellationToken, DefaultMetadata);
            if (state == 1)
                cancellationTokenSource.Cancel();
            task.IsCompleted.ShouldBeFalse();
            action!();
            await task.AsTask().WaitSafeAsync();
            switch (state)
            {
                case 1:
                    task.IsCanceled.ShouldBeTrue();
                    break;
                case 2:
                    task.IsFaulted.ShouldBeTrue();
                    task.AsTask().Exception!.GetBaseException().ShouldEqual(ex);
                    break;
                default:
                    task.IsCompleted.ShouldBeTrue();
                    task.Result.ShouldEqual(result.Result);
                    break;
            }
        }

        [Fact]
        public async Task TryCleanupAsyncShouldBeExecutedInline()
        {
            var dispatcher = new ThreadDispatcher();
            var component = new ExecutionModeViewManagerDecorator(dispatcher);
            var view = new View(new ViewMapping("id", typeof(TestViewModel), typeof(object), DefaultMetadata), this, new TestViewModel());
            var viewModel = new TestViewModel();
            var result = true;
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
            manager.AddComponent(new TestViewManagerComponent(manager)
            {
                TryCleanupAsync = (v, r, meta, token) =>
                {
                    v.ShouldEqual(view);
                    r.ShouldEqual(viewModel);
                    meta.ShouldEqual(DefaultMetadata);
                    token.ShouldEqual(cancellationToken);
                    return new ValueTask<bool>(result);
                }
            });

            var r = await manager.TryCleanupAsync(view, viewModel, cancellationToken, DefaultMetadata);
            r.ShouldEqual(result);
            action.ShouldBeNull();
        }

        [Theory]
        [InlineData(0)] //success
        [InlineData(1)] //canceled
        [InlineData(2)] //error
        public async Task TryCleanupAsyncShouldUseThreadDispatcher(int state)
        {
            var dispatcher = new ThreadDispatcher();
            var component = new ExecutionModeViewManagerDecorator(dispatcher);
            var view = new View(new ViewMapping("id", typeof(TestViewModel), typeof(object), DefaultMetadata), this, new TestViewModel());
            var viewModel = new TestViewModel();
            var result = true;
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
            manager.AddComponent(new TestViewManagerComponent(manager)
            {
                TryCleanupAsync = (v, r, meta, token) =>
                {
                    v.ShouldEqual(view);
                    r.ShouldEqual(viewModel);
                    meta.ShouldEqual(DefaultMetadata);
                    token.ShouldEqual(cancellationToken);
                    if (state == 2)
                        throw ex;
                    return new ValueTask<bool>(result);
                }
            });

            if (state == 1)
                cancellationTokenSource.Cancel();
            var task = manager.TryCleanupAsync(view, viewModel, cancellationToken, DefaultMetadata);
            task.IsCompleted.ShouldBeFalse();
            action!();
            await task.WaitSafeAsync();
            switch (state)
            {
                case 1:
                    task.IsCanceled.ShouldBeTrue();
                    break;
                case 2:
                    task.IsFaulted.ShouldBeTrue();
                    task.AsTask().Exception!.GetBaseException().ShouldEqual(ex);
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