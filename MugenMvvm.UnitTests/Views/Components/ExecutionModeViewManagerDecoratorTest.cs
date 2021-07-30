using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Tests.Threading;
using MugenMvvm.Tests.ViewModels;
using MugenMvvm.Tests.Views;
using MugenMvvm.Threading;
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
        private readonly ExecutionModeViewManagerDecorator _decorator;

        public ExecutionModeViewManagerDecoratorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _viewModel = new TestViewModel();
            _view = new View(new ViewMapping("id", typeof(TestViewModel), typeof(object), Metadata), this, _viewModel);
            _decorator = new ExecutionModeViewManagerDecorator(ThreadDispatcher);
            ViewManager.AddComponent(_decorator);
        }

        [Fact]
        public async Task TryCleanupAsyncShouldBeExecutedInline()
        {
            var result = true;
            Action? action = null;
            ThreadDispatcher.AddComponent(new TestThreadDispatcherComponent
            {
                CanExecuteInline = (_, mode, _) =>
                {
                    mode.ShouldEqual(_decorator.CleanupExecutionMode);
                    return true;
                },
                Execute = (_, a, _, arg3, _) =>
                {
                    action += () => a(arg3);
                    return true;
                }
            });

            ViewManager.AddComponent(new TestViewManagerComponent
            {
                TryCleanupAsync = (m, v, r, meta, token) =>
                {
                    m.ShouldEqual(ViewManager);
                    v.ShouldEqual(_view);
                    r.ShouldEqual(_viewModel);
                    meta.ShouldEqual(Metadata);
                    token.ShouldEqual(DefaultCancellationToken);
                    return Task.FromResult(result);
                }
            });

            var r = await ViewManager.TryCleanupAsync(_view, _viewModel, DefaultCancellationToken, Metadata);
            r.ShouldEqual(result);
            action.ShouldBeNull();
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
            ThreadDispatcher.AddComponent(new TestThreadDispatcherComponent
            {
                CanExecuteInline = (_, mode, _) =>
                {
                    mode.ShouldEqual(_decorator.CleanupExecutionMode);
                    return false;
                },
                Execute = (_, a, mode, arg3, _) =>
                {
                    mode.ShouldEqual(_decorator.CleanupExecutionMode);
                    action += () => a(arg3);
                    return true;
                }
            });

            ViewManager.AddComponent(new TestViewManagerComponent
            {
                TryCleanupAsync = (m, v, r, meta, token) =>
                {
                    m.ShouldEqual(ViewManager);
                    v.ShouldEqual(_view);
                    r.ShouldEqual(_viewModel);
                    meta.ShouldEqual(Metadata);
                    token.ShouldEqual(cancellationToken);
                    if (state == 2)
                        throw ex;
                    return Task.FromResult(result);
                }
            });

            if (state == 1)
                cancellationTokenSource.Cancel();
            var task = ViewManager.TryCleanupAsync(_view, _viewModel, cancellationToken, Metadata);
            task.IsCompleted.ShouldEqual(state == 1);
            action?.Invoke();
            await task.WaitSafeAsync();
            switch (state)
            {
                case 1:
                    task.IsCanceled.ShouldBeTrue();
                    break;
                case 2:
                    task.IsFaulted.ShouldBeTrue();
                    task.Exception!.GetBaseException().ShouldEqual(ex);
                    break;
                default:
                    task.IsCompleted.ShouldBeTrue();
                    task.IsCanceled.ShouldBeFalse();
                    break;
            }
        }

        [Fact]
        public async Task TryInitializeAsyncShouldBeExecutedInline()
        {
            var result = new ValueTask<IView?>(_view);
            Action? action = null;
            ThreadDispatcher.AddComponent(new TestThreadDispatcherComponent
            {
                CanExecuteInline = (_, mode, _) =>
                {
                    mode.ShouldEqual(_decorator.InitializeExecutionMode);
                    return true;
                },
                Execute = (_, a, _, arg3, _) =>
                {
                    action += () => a(arg3);
                    return true;
                }
            });

            ViewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (m, viewMapping, r, meta, token) =>
                {
                    m.ShouldEqual(ViewManager);
                    viewMapping.ShouldEqual(_view.Mapping);
                    r.ShouldEqual(_viewModel);
                    meta.ShouldEqual(Metadata);
                    token.ShouldEqual(DefaultCancellationToken);
                    return result;
                }
            });

            (await ViewManager.InitializeAsync(_view.Mapping, _viewModel, DefaultCancellationToken, Metadata)).ShouldEqual(result.Result);
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
            ThreadDispatcher.AddComponent(new TestThreadDispatcherComponent
            {
                CanExecuteInline = (_, mode, _) =>
                {
                    mode.ShouldEqual(_decorator.InitializeExecutionMode);
                    return false;
                },
                Execute = (_, a, mode, arg3, _) =>
                {
                    mode.ShouldEqual(_decorator.InitializeExecutionMode);
                    action += () => a(arg3);
                    return true;
                }
            });

            ViewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (m, viewMapping, r, meta, token) =>
                {
                    m.ShouldEqual(ViewManager);
                    viewMapping.ShouldEqual(_view.Mapping);
                    r.ShouldEqual(_viewModel);
                    meta.ShouldEqual(Metadata);
                    token.ShouldEqual(cancellationToken);
                    if (state == 2)
                        throw ex;
                    return result;
                }
            });

            var task = ViewManager.InitializeAsync(_view.Mapping, _viewModel, cancellationToken, Metadata);
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

        protected override IThreadDispatcher GetThreadDispatcher() => new ThreadDispatcher(ComponentCollectionManager);

        protected override IViewManager GetViewManager() => new ViewManager(ComponentCollectionManager);
    }
}