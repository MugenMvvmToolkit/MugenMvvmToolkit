using System.Threading.Tasks;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Requests;
using MugenMvvm.Tests.Internal;
using MugenMvvm.Tests.ViewModels;
using MugenMvvm.Tests.Views;
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
        private readonly TestServiceProvider _serviceProvider;

        public ViewModelViewInitializerDecoratorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _viewModel = new TestViewModel();
            var mapping = new ViewMapping("id", typeof(TestViewModel), typeof(object), Metadata);
            _view = new View(mapping, new object(), _viewModel);
            _serviceProvider = new TestServiceProvider();
            ViewManager.AddComponent(new ViewModelViewInitializerDecorator(ViewModelManager, _serviceProvider));
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
        public async Task TryInitializeAsyncShouldUpdateViewViewModel()
        {
            var result = new ValueTask<IView?>(_view);
            var initializeCount = 0;

            ViewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (vm, viewMapping, r, m, token) =>
                {
                    ++initializeCount;
                    vm.ShouldEqual(ViewManager);
                    var request = (ViewModelViewRequest)r;
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

                    m.ShouldEqual(Metadata);
                    token.ShouldEqual(DefaultCancellationToken);
                    return result;
                }
            });
            _serviceProvider.GetService = type =>
            {
                type.ShouldEqual(_view.Mapping.ViewType);
                return _view;
            };

            ViewModelManager.AddComponent(new TestViewModelProviderComponent
            {
                TryGetViewModel = (_, o, arg3) =>
                {
                    o.ShouldEqual(_view.Mapping.ViewModelType);
                    return _viewModel;
                }
            });

            (await ViewManager.InitializeAsync(_view.Mapping, new ViewModelViewRequest(null, null), DefaultCancellationToken, Metadata)).ShouldEqual(result.Result);
            initializeCount.ShouldEqual(1);

            initializeCount = 0;
            (await ViewManager.InitializeAsync(_view.Mapping, _viewModel, DefaultCancellationToken, Metadata)).ShouldEqual(result.Result);
            initializeCount.ShouldEqual(1);

            initializeCount = 0;
            (await ViewManager.InitializeAsync(_view.Mapping, _view, DefaultCancellationToken, Metadata)).ShouldEqual(result.Result);
            initializeCount.ShouldEqual(1);

            initializeCount = 0;
            (await ViewManager.InitializeAsync(ViewMapping.Undefined, new ViewModelViewRequest(null, null), DefaultCancellationToken, Metadata)).ShouldEqual(result.Result);
            initializeCount.ShouldEqual(1);
        }

        protected override IViewManager GetViewManager() => new ViewManager(ComponentCollectionManager);

        protected override IViewModelManager GetViewModelManager() => new ViewModelManager(ComponentCollectionManager);
    }
}