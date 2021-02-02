using System;
using System.Threading.Tasks;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Requests;
using MugenMvvm.UnitTests.ViewModels.Internal;
using MugenMvvm.UnitTests.Views.Internal;
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
        private readonly ViewManager _viewManager;

        public UndefinedMappingViewInitializerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _viewModel = new TestViewModel();
            _viewMapping = new ViewMapping("id", typeof(TestViewModel), typeof(object), DefaultMetadata);
            _view = new View(_viewMapping, this, _viewModel);
            _viewManager = new ViewManager(ComponentCollectionManager);
            _viewManager.AddComponent(new UndefinedMappingViewInitializer());
        }

        [Fact]
        public async Task TryCleanupAsyncShouldBeHandledByComponents()
        {
            var result = true;
            var invokeCount = 0;

            _viewManager.AddComponent(new TestViewManagerComponent
            {
                TryCleanupAsync = (v, r, meta, token) =>
                {
                    ++invokeCount;
                    v.ShouldEqual(_view);
                    r.ShouldEqual(_viewModel);
                    meta.ShouldEqual(DefaultMetadata);
                    token.ShouldEqual(DefaultCancellationToken);
                    return new ValueTask<bool>(result);
                }
            });

            var r = await _viewManager.TryCleanupAsync(_view, _viewModel, DefaultCancellationToken, DefaultMetadata);
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

            _viewManager.AddComponent(new TestViewMappingProviderComponent
            {
                TryGetMappings = (o, context) =>
                {
                    o.ShouldEqual(request);
                    context.ShouldEqual(DefaultMetadata);
                    return _viewMapping;
                }
            });
            _viewManager.AddComponent(new TestViewProviderComponent
            {
                TryGetViews = (o, context) =>
                {
                    o.ShouldEqual(request.ViewModel);
                    context.ShouldEqual(DefaultMetadata);
                    return new View(_viewMapping, oldView, request.ViewModel!);
                }
            });
            _viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (m, r, meta, token) =>
                {
                    ++invokeCount;
                    m.ShouldEqual(_viewMapping);
                    request.View.ShouldEqual(view);
                    r.ShouldEqual(request);
                    meta.ShouldEqual(DefaultMetadata);
                    token.ShouldEqual(DefaultCancellationToken);
                    return result;
                }
            });

            (await _viewManager.TryInitializeAsync(mapping, request, DefaultCancellationToken, DefaultMetadata)).ShouldEqual(result.Result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public async Task TryInitializeAsyncShouldBeHandledByComponentsMoreThatOneMapping()
        {
            var request = new ViewModelViewRequest(_viewModel, typeof(object));
            var mapping = ViewMapping.Undefined;
            var result = new ValueTask<IView?>();
            var invokeCount = 0;

            _viewManager.AddComponent(new TestViewMappingProviderComponent
            {
                TryGetMappings = (o, context) =>
                {
                    o.ShouldEqual(request);
                    context.ShouldEqual(DefaultMetadata);
                    return new[] {new ViewMapping("1", typeof(IViewModelBase), typeof(object)), new ViewMapping("1", typeof(IViewModelBase), typeof(object))};
                }
            });
            _viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (m, r, meta, token) =>
                {
                    ++invokeCount;
                    m.ShouldEqual(mapping);
                    r.ShouldEqual(request);
                    meta.ShouldEqual(DefaultMetadata);
                    token.ShouldEqual(DefaultCancellationToken);
                    return result;
                }
            });

            (await _viewManager.TryInitializeAsync(mapping, request, DefaultCancellationToken, DefaultMetadata)).ShouldEqual(result.Result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public async Task TryInitializeAsyncShouldBeHandledByComponentsNotUndefinedMapping()
        {
            var request = new ViewModelViewRequest(_viewModel, typeof(object));
            var result = new ValueTask<IView?>();
            var invokeCount = 0;

            _viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (m, r, meta, token) =>
                {
                    ++invokeCount;
                    m.ShouldEqual(_viewMapping);
                    r.ShouldEqual(request);
                    meta.ShouldEqual(DefaultMetadata);
                    token.ShouldEqual(DefaultCancellationToken);
                    return result;
                }
            });

            (await _viewManager.TryInitializeAsync(_viewMapping, request, DefaultCancellationToken, DefaultMetadata)).ShouldEqual(result.Result);
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

            _viewManager.AddComponent(new TestViewMappingProviderComponent
            {
                TryGetMappings = (o, context) =>
                {
                    o.ShouldEqual(request);
                    context.ShouldEqual(DefaultMetadata);
                    return newMapping;
                }
            });
            _viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (m, r, meta, token) =>
                {
                    ++invokeCount;
                    m.ShouldEqual(newMapping);
                    request.View.ShouldBeNull();
                    r.ShouldEqual(request);
                    meta.ShouldEqual(DefaultMetadata);
                    token.ShouldEqual(DefaultCancellationToken);
                    return result;
                }
            });

            (await _viewManager.TryInitializeAsync(mapping, request, DefaultCancellationToken, DefaultMetadata)).ShouldEqual(result.Result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public async Task TryInitializeAsyncShouldBeHandledByComponentsWithNewMapping2()
        {
            var request = new ViewModelViewRequest(_viewModel, typeof(int));
            var mapping = ViewMapping.Undefined;
            var result = new ValueTask<IView?>();
            var invokeCount = 0;

            _viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (m, r, meta, token) =>
                {
                    ++invokeCount;
                    m.Id.ShouldEqual($"a{request.ViewModel!.GetType().Name}{typeof(int).Name}");
                    m.ViewModelType.ShouldEqual(request.ViewModel.GetType());
                    m.ViewType.ShouldEqual(typeof(int));
                    m.Metadata.ShouldEqual(DefaultMetadata);
                    request.View.ShouldBeNull();
                    r.ShouldEqual(request);
                    meta.ShouldEqual(DefaultMetadata);
                    token.ShouldEqual(DefaultCancellationToken);
                    return result;
                }
            });

            (await _viewManager.TryInitializeAsync(mapping, request, DefaultCancellationToken, DefaultMetadata)).ShouldEqual(result.Result);
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

            _viewManager.AddComponent(new TestViewMappingProviderComponent
            {
                TryGetMappings = (o, context) =>
                {
                    o.ShouldEqual(request);
                    context.ShouldEqual(DefaultMetadata);
                    return newMapping;
                }
            });
            _viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (m, r, meta, token) =>
                {
                    ++invokeCount;
                    m.ShouldEqual(newMapping);
                    r.ShouldEqual(request);
                    meta.ShouldEqual(DefaultMetadata);
                    token.ShouldEqual(DefaultCancellationToken);
                    return result;
                }
            });

            (await _viewManager.TryInitializeAsync(mapping, request, DefaultCancellationToken, DefaultMetadata)).ShouldEqual(result.Result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public async Task TryInitializeAsyncShouldReturnCreatedView()
        {
            var request = new ViewModelViewRequest(_viewModel, typeof(object));
            _viewManager.AddComponent(new TestViewMappingProviderComponent
            {
                TryGetMappings = (o, context) =>
                {
                    o.ShouldEqual(request);
                    context.ShouldEqual(DefaultMetadata);
                    return _viewMapping;
                }
            });
            _viewManager.AddComponent(new TestViewProviderComponent
            {
                TryGetViews = (o, context) =>
                {
                    o.ShouldEqual(request.ViewModel);
                    context.ShouldEqual(DefaultMetadata);
                    return _view;
                }
            });
            _viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (m, r, meta, token) => throw new NotSupportedException()
            });

            (await _viewManager.TryInitializeAsync(ViewMapping.Undefined, request, DefaultCancellationToken, DefaultMetadata)).ShouldEqual(_view);
        }
    }
}