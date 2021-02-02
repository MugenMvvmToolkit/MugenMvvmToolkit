using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Threading;
using MugenMvvm.UnitTests.Threading.Internal;
using MugenMvvm.UnitTests.ViewModels.Internal;
using MugenMvvm.UnitTests.Views.Internal;
using MugenMvvm.Views;
using MugenMvvm.Views.Components;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Views.Components
{
    public class ExecutionModeViewManagerDecoratorTest : UnitTestBase
    {
        private readonly View _view;
        private readonly TestViewModel _viewModel;
        private readonly ViewManager _viewManager;
        private readonly ThreadDispatcher _threadDispatcher;
        private readonly ExecutionModeViewManagerDecorator _decorator;

        public ExecutionModeViewManagerDecoratorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _viewModel = new TestViewModel();
            _view = new View(new ViewMapping("id", typeof(TestViewModel), typeof(object), DefaultMetadata), this, _viewModel);
            _viewManager = new ViewManager(ComponentCollectionManager);
            _threadDispatcher = new ThreadDispatcher(ComponentCollectionManager);
            _decorator = new ExecutionModeViewManagerDecorator(_threadDispatcher);
            _viewManager.AddComponent(_decorator);
        }

        [Fact]
        public async Task TryCleanupAsyncShouldBeExecutedInline()
        {
            var result = true;
            Action? action = null;
            _threadDispatcher.AddComponent(new TestThreadDispatcherComponent
            {
                CanExecuteInline = (mode, context) =>
                {
                    mode.ShouldEqual(_decorator.CleanupExecutionMode);
                    return true;
                },
                Execute = (a, mode, arg3, arg4) =>
                {
                    action += () => a(arg3);
                    return true;
                }
            });

            _viewManager.AddComponent(new TestViewManagerComponent(_viewManager)
            {
                TryCleanupAsync = (v, r, meta, token) =>
                {
                    v.ShouldEqual(_view);
                    r.ShouldEqual(_viewModel);
                    meta.ShouldEqual(DefaultMetadata);
                    token.ShouldEqual(DefaultCancellationToken);
                    return new ValueTask<bool>(result);
                }
            });

            var r = await _viewManager.TryCleanupAsync(_view, _viewModel, DefaultCancellationToken, DefaultMetadata);
            r.ShouldEqual(result);
            action.ShouldBeNull();
        }

        [Fact]
        public async Task TryInitializeAsyncShouldBeExecutedInline()
        {
            var result = new ValueTask<IView?>(_view);
            Action? action = null;
            _threadDispatcher.AddComponent(new TestThreadDispatcherComponent
            {
                CanExecuteInline = (mode, context) =>
                {
                    mode.ShouldEqual(_decorator.InitializeExecutionMode);
                    return true;
                },
                Execute = (a, mode, arg3, arg4) =>
                {
                    action += () => a(arg3);
                    return true;
                }
            });

            _viewManager.AddComponent(new TestViewManagerComponent(_viewManager)
            {
                TryInitializeAsync = (viewMapping, r, meta, token) =>
                {
                    viewMapping.ShouldEqual(_view.Mapping);
                    r.ShouldEqual(_viewModel);
                    meta.ShouldEqual(DefaultMetadata);
                    token.ShouldEqual(DefaultCancellationToken);
                    return result;
                }
            });

            (await _viewManager.InitializeAsync(_view.Mapping, _viewModel, DefaultCancellationToken, DefaultMetadata)).ShouldEqual(result.Result);
            action.ShouldBeNull();
        }

        [Theory]
        [InlineData(0)] //success
        [InlineData(1)] //canceled
        [InlineData(2)] //error
        public async Task TryInitializeAsyncShouldUseThreadDispatcher(int state)
        {
            var result = new ValueTask<IView?>(_view);
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            var ex = new Exception();
            Action? action = null;
            _threadDispatcher.AddComponent(new TestThreadDispatcherComponent
            {
                CanExecuteInline = (mode, context) =>
                {
                    mode.ShouldEqual(_decorator.InitializeExecutionMode);
                    return false;
                },
                Execute = (a, mode, arg3, arg4) =>
                {
                    mode.ShouldEqual(_decorator.InitializeExecutionMode);
                    action += () => a(arg3);
                    return true;
                }
            });

            _viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (viewMapping, r, meta, token) =>
                {
                    viewMapping.ShouldEqual(_view.Mapping);
                    r.ShouldEqual(_viewModel);
                    meta.ShouldEqual(DefaultMetadata);
                    token.ShouldEqual(cancellationToken);
                    if (state == 2)
                        throw ex;
                    return result;
                }
            });

            var task = _viewManager.InitializeAsync(_view.Mapping, _viewModel, cancellationToken, DefaultMetadata);
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

        [Theory]
        [InlineData(0)] //success
        [InlineData(1)] //canceled
        [InlineData(2)] //error
        public async Task TryCleanupAsyncShouldUseThreadDispatcher(int state)
        {
            var result = true;
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            var ex = new Exception();
            Action? action = null;
            _threadDispatcher.AddComponent(new TestThreadDispatcherComponent
            {
                CanExecuteInline = (mode, context) =>
                {
                    mode.ShouldEqual(_decorator.CleanupExecutionMode);
                    return false;
                },
                Execute = (a, mode, arg3, arg4) =>
                {
                    mode.ShouldEqual(_decorator.CleanupExecutionMode);
                    action += () => a(arg3);
                    return true;
                }
            });

            _viewManager.AddComponent(new TestViewManagerComponent(_viewManager)
            {
                TryCleanupAsync = (v, r, meta, token) =>
                {
                    v.ShouldEqual(_view);
                    r.ShouldEqual(_viewModel);
                    meta.ShouldEqual(DefaultMetadata);
                    token.ShouldEqual(cancellationToken);
                    if (state == 2)
                        throw ex;
                    return new ValueTask<bool>(result);
                }
            });

            if (state == 1)
                cancellationTokenSource.Cancel();
            var task = _viewManager.TryCleanupAsync(_view, _viewModel, cancellationToken, DefaultMetadata);
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
    }
}