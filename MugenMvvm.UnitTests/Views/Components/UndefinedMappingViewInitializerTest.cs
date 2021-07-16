using System;
using System.Threading.Tasks;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Requests;
using MugenMvvm.Tests.ViewModels;
using MugenMvvm.Tests.Views;
using MugenMvvm.Views;
using MugenMvvm.Views.Components;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Views.Components
{
    public class UndefinedMappingViewInitializerTest : UnitTestBase
    {
        private readonly View _view;
        private readonly ViewMapping _viewMapping;
        private readonly TestViewModel _viewModel;

        public UndefinedMappingViewInitializerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _viewModel = new TestViewModel();
            _viewMapping = new ViewMapping("id", typeof(TestViewModel), typeof(object), Metadata);
            _view = new View(_viewMapping, this, _viewModel);
            ViewManager.AddComponent(new UndefinedMappingViewInitializer());
        }

        [Fact]
        public async Task TryCleanupAsyncShouldBeHandledByComponents()
        {
            var result = true;
            var invokeCount = 0;

            ViewManager.AddComponent(new TestViewManagerComponent
            {
                TryCleanupAsync = (m, v, r, meta, token) =>
                {
                    ++invokeCount;
                    m.ShouldEqual(ViewManager);
                    v.ShouldEqual(_view);
                    r.ShouldEqual(_viewModel);
                    meta.ShouldEqual(Metadata);
                    token.ShouldEqual(DefaultCancellationToken);
                    return new ValueTask<bool>(result);
                }
            });

            var r = await ViewManager.TryCleanupAsync(_view, _viewModel, DefaultCancellationToken, Metadata);
            r.ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public async Task TryInitializeAsyncShouldBeHandledByComponentsDifferentView()
        {
            var view = new object();
            var oldView = new object();
            var request = new ViewModelViewRequest(_viewModel, view);
            var mapping = ViewMapping.Undefined;
            var result = new ValueTask<IView?>();
            var invokeCount = 0;

            ViewManager.AddComponent(new TestViewMappingProviderComponent
            {
                TryGetMappings = (m, o, context) =>
                {
                    m.ShouldEqual(ViewManager);
                    o.ShouldEqual(request);
                    context.ShouldEqual(Metadata);
                    return _viewMapping;
                }
            });
            ViewManager.AddComponent(new TestViewProviderComponent
            {
                TryGetViews = (m, o, context) =>
                {
                    m.ShouldEqual(ViewManager);
                    o.ShouldEqual(request.ViewModel);
                    context.ShouldEqual(Metadata);
                    return new View(_viewMapping, oldView, request.ViewModel!);
                }
            });
            ViewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (vm, m, r, meta, token) =>
                {
                    ++invokeCount;
                    vm.ShouldEqual(ViewManager);
                    m.ShouldEqual(_viewMapping);
                    request.View.ShouldEqual(view);
                    r.ShouldEqual(request);
                    meta.ShouldEqual(Metadata);
                    token.ShouldEqual(DefaultCancellationToken);
                    return result;
                }
            });

            (await ViewManager.TryInitializeAsync(mapping, request, DefaultCancellationToken, Metadata)).ShouldEqual(result.Result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public async Task TryInitializeAsyncShouldBeHandledByComponentsMoreThatOneMapping()
        {
            var request = new ViewModelViewRequest(_viewModel, typeof(object));
            var mapping = ViewMapping.Undefined;
            var result = new ValueTask<IView?>();
            var invokeCount = 0;

            ViewManager.AddComponent(new TestViewMappingProviderComponent
            {
                TryGetMappings = (m, o, context) =>
                {
                    m.ShouldEqual(ViewManager);
                    o.ShouldEqual(request);
                    context.ShouldEqual(Metadata);
                    return new[] { new ViewMapping("1", typeof(IViewModelBase), typeof(object)), new ViewMapping("1", typeof(IViewModelBase), typeof(object)) };
                }
            });
            ViewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (vm, m, r, meta, token) =>
                {
                    ++invokeCount;
                    vm.ShouldEqual(ViewManager);
                    m.ShouldEqual(mapping);
                    r.ShouldEqual(request);
                    meta.ShouldEqual(Metadata);
                    token.ShouldEqual(DefaultCancellationToken);
                    return result;
                }
            });

            (await ViewManager.TryInitializeAsync(mapping, request, DefaultCancellationToken, Metadata)).ShouldEqual(result.Result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public async Task TryInitializeAsyncShouldBeHandledByComponentsNotUndefinedMapping()
        {
            var request = new ViewModelViewRequest(_viewModel, typeof(object));
            var result = new ValueTask<IView?>();
            var invokeCount = 0;

            ViewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (vm, m, r, meta, token) =>
                {
                    ++invokeCount;
                    vm.ShouldEqual(ViewManager);
                    m.ShouldEqual(_viewMapping);
                    r.ShouldEqual(request);
                    meta.ShouldEqual(Metadata);
                    token.ShouldEqual(DefaultCancellationToken);
                    return result;
                }
            });

            (await ViewManager.TryInitializeAsync(_viewMapping, request, DefaultCancellationToken, Metadata)).ShouldEqual(result.Result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public async Task TryInitializeAsyncShouldBeHandledByComponentsWithNewMapping1()
        {
            var request = new ViewModelViewRequest(_viewModel, typeof(object));
            var mapping = ViewMapping.Undefined;
            var newMapping = _viewMapping;
            var result = new ValueTask<IView?>();
            var invokeCount = 0;

            ViewManager.AddComponent(new TestViewMappingProviderComponent
            {
                TryGetMappings = (vm, o, context) =>
                {
                    vm.ShouldEqual(ViewManager);
                    o.ShouldEqual(request);
                    context.ShouldEqual(Metadata);
                    return newMapping;
                }
            });
            ViewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (vm, m, r, meta, token) =>
                {
                    ++invokeCount;
                    vm.ShouldEqual(ViewManager);
                    m.ShouldEqual(newMapping);
                    request.View.ShouldBeNull();
                    r.ShouldEqual(request);
                    meta.ShouldEqual(Metadata);
                    token.ShouldEqual(DefaultCancellationToken);
                    return result;
                }
            });

            (await ViewManager.TryInitializeAsync(mapping, request, DefaultCancellationToken, Metadata)).ShouldEqual(result.Result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public async Task TryInitializeAsyncShouldBeHandledByComponentsWithNewMapping2()
        {
            var request = new ViewModelViewRequest(_viewModel, typeof(int));
            var mapping = ViewMapping.Undefined;
            var result = new ValueTask<IView?>();
            var invokeCount = 0;

            ViewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (vm, m, r, meta, token) =>
                {
                    ++invokeCount;
                    vm.ShouldEqual(ViewManager);
                    m.Id.ShouldEqual($"a{request.ViewModel!.GetType().Name}{typeof(int).Name}");
                    m.ViewModelType.ShouldEqual(request.ViewModel.GetType());
                    m.ViewType.ShouldEqual(typeof(int));
                    m.Metadata.ShouldEqual(Metadata);
                    request.View.ShouldBeNull();
                    r.ShouldEqual(request);
                    meta.ShouldEqual(Metadata);
                    token.ShouldEqual(DefaultCancellationToken);
                    return result;
                }
            });

            (await ViewManager.TryInitializeAsync(mapping, request, DefaultCancellationToken, Metadata)).ShouldEqual(result.Result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public async Task TryInitializeAsyncShouldBeHandledByComponentsWithNewMapping3()
        {
            var request = _viewModel;
            var mapping = ViewMapping.Undefined;
            var newMapping = _viewMapping;
            var result = new ValueTask<IView?>();
            var invokeCount = 0;

            ViewManager.AddComponent(new TestViewMappingProviderComponent
            {
                TryGetMappings = (m, o, context) =>
                {
                    m.ShouldEqual(ViewManager);
                    o.ShouldEqual(request);
                    context.ShouldEqual(Metadata);
                    return newMapping;
                }
            });
            ViewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (vm, m, r, meta, token) =>
                {
                    ++invokeCount;
                    vm.ShouldEqual(ViewManager);
                    m.ShouldEqual(newMapping);
                    r.ShouldEqual(request);
                    meta.ShouldEqual(Metadata);
                    token.ShouldEqual(DefaultCancellationToken);
                    return result;
                }
            });

            (await ViewManager.TryInitializeAsync(mapping, request, DefaultCancellationToken, Metadata)).ShouldEqual(result.Result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public async Task TryInitializeAsyncShouldReturnCreatedView()
        {
            var request = new ViewModelViewRequest(_viewModel, typeof(object));
            ViewManager.AddComponent(new TestViewMappingProviderComponent
            {
                TryGetMappings = (m, o, context) =>
                {
                    m.ShouldEqual(ViewManager);
                    o.ShouldEqual(request);
                    context.ShouldEqual(Metadata);
                    return _viewMapping;
                }
            });
            ViewManager.AddComponent(new TestViewProviderComponent
            {
                TryGetViews = (m, o, context) =>
                {
                    m.ShouldEqual(ViewManager);
                    o.ShouldEqual(request.ViewModel);
                    context.ShouldEqual(Metadata);
                    return _view;
                }
            });
            ViewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (_, _, _, _, _) => throw new NotSupportedException()
            });

            (await ViewManager.TryInitializeAsync(ViewMapping.Undefined, request, DefaultCancellationToken, Metadata)).ShouldEqual(_view);
        }

        protected override IViewManager GetViewManager() => new ViewManager(ComponentCollectionManager);
    }
}