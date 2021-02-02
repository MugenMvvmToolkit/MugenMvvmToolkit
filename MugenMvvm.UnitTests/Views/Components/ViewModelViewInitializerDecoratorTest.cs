using System.Threading.Tasks;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Requests;
using MugenMvvm.UnitTests.Internal.Internal;
using MugenMvvm.UnitTests.ViewModels.Internal;
using MugenMvvm.UnitTests.Views.Internal;
using MugenMvvm.ViewModels;
using MugenMvvm.Views;
using MugenMvvm.Views.Components;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Views.Components
{
    public class ViewModelViewInitializerDecoratorTest : UnitTestBase
    {
        private readonly View _view;
        private readonly TestViewModel _viewModel;
        private readonly ViewManager _viewManager;
        private readonly ViewModelManager _viewModelManager;
        private readonly TestServiceProvider _serviceProvider;

        public ViewModelViewInitializerDecoratorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _viewModel = new TestViewModel();
            var mapping = new ViewMapping("id", typeof(TestViewModel), typeof(object), DefaultMetadata);
            _view = new View(mapping, new object(), _viewModel);
            _serviceProvider = new TestServiceProvider();
            _viewModelManager = new ViewModelManager(ComponentCollectionManager);
            _viewManager = new ViewManager(ComponentCollectionManager);
            _viewManager.AddComponent(new ViewModelViewInitializerDecorator(_viewModelManager, _serviceProvider));
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
        public async Task TryInitializeAsyncShouldUpdateViewViewModel()
        {
            var result = new ValueTask<IView?>(_view);
            var initializeCount = 0;

            _viewManager.AddComponent(new TestViewManagerComponent(_viewManager)
            {
                TryInitializeAsync = (viewMapping, r, m, token) =>
                {
                    ++initializeCount;
                    var request = (ViewModelViewRequest) r;
                    if (viewMapping == ViewMapping.Undefined)
                    {
                        request.ViewModel.ShouldBeNull();
                        request.View.ShouldBeNull();
                    }
                    else
                    {
                        request.View.ShouldEqual(_view);
                        request.ViewModel.ShouldEqual(_viewModel);
                    }

                    m.ShouldEqual(DefaultMetadata);
                    token.ShouldEqual(DefaultCancellationToken);
                    return result;
                }
            });
            _serviceProvider.GetService = type =>
            {
                type.ShouldEqual(_view.Mapping.ViewType);
                return _view;
            };

            _viewModelManager.AddComponent(new TestViewModelProviderComponent
            {
                TryGetViewModel = (o, arg3) =>
                {
                    o.ShouldEqual(_view.Mapping.ViewModelType);
                    return _viewModel;
                }
            });

            (await _viewManager.InitializeAsync(_view.Mapping, new ViewModelViewRequest(null, null), DefaultCancellationToken, DefaultMetadata)).ShouldEqual(result.Result);
            initializeCount.ShouldEqual(1);

            initializeCount = 0;
            (await _viewManager.InitializeAsync(_view.Mapping, _viewModel, DefaultCancellationToken, DefaultMetadata)).ShouldEqual(result.Result);
            initializeCount.ShouldEqual(1);

            initializeCount = 0;
            (await _viewManager.InitializeAsync(_view.Mapping, _view, DefaultCancellationToken, DefaultMetadata)).ShouldEqual(result.Result);
            initializeCount.ShouldEqual(1);

            initializeCount = 0;
            (await _viewManager.InitializeAsync(ViewMapping.Undefined, new ViewModelViewRequest(null, null), DefaultCancellationToken, DefaultMetadata)).ShouldEqual(result.Result);
            initializeCount.ShouldEqual(1);
        }
    }
}