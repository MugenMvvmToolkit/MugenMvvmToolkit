using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Internal;
using MugenMvvm.Requests;
using MugenMvvm.UnitTests.Internal.Internal;
using MugenMvvm.UnitTests.ViewModels.Internal;
using MugenMvvm.UnitTests.Views.Internal;
using MugenMvvm.ViewModels;
using MugenMvvm.Views;
using MugenMvvm.Views.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Views.Components
{
    public class ViewModelViewInitializerDecoratorTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public async Task TryInitializeAsyncShouldUpdateViewViewModel()
        {
            var viewType = typeof(object);
            var viewModelType = typeof(TestViewModel);
            var view = new object();
            var viewModel = new TestViewModel();
            var result = new ValueTask<IView?>(new View(new ViewMapping("id", typeof(object), typeof(TestViewModel), DefaultMetadata), this, new TestViewModel()));
            var initializeCount = 0;
            var mapping = new ViewMapping("id", viewType, viewModelType, DefaultMetadata);
            var cancellationToken = new CancellationTokenSource().Token;

            var viewManager = new ViewManager();
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (viewMapping, r, m, token) =>
                {
                    ++initializeCount;
                    viewManager.ShouldEqual(viewManager);
                    var request = (ViewModelViewRequest) r;
                    if (viewMapping == ViewMapping.Undefined)
                    {
                        request.ViewModel.ShouldBeNull();
                        request.View.ShouldBeNull();
                    }
                    else
                    {
                        request.View.ShouldEqual(view);
                        request.ViewModel.ShouldEqual(viewModel);
                    }

                    m.ShouldEqual(DefaultMetadata);
                    token.ShouldEqual(cancellationToken);
                    return result;
                }
            });
            var testServiceProvider = new TestServiceProvider
            {
                GetService = type =>
                {
                    type.ShouldEqual(viewType);
                    return view;
                }
            };
            var viewModelManager = new ViewModelManager();
            viewModelManager.AddComponent(new TestViewModelProviderComponent
            {
                TryGetViewModel = (o, arg3) =>
                {
                    o.ShouldEqual(viewModelType);
                    return viewModel;
                }
            });

            var component = new ViewModelViewInitializerDecorator(viewModelManager, testServiceProvider);
            viewManager.AddComponent(component);

            (await viewManager.InitializeAsync(mapping, new ViewModelViewRequest(null, null), cancellationToken, DefaultMetadata)).ShouldEqual(result.Result);
            initializeCount.ShouldEqual(1);

            initializeCount = 0;
            (await viewManager.InitializeAsync(mapping, viewModel, cancellationToken, DefaultMetadata)).ShouldEqual(result.Result);
            initializeCount.ShouldEqual(1);

            initializeCount = 0;
            (await viewManager.InitializeAsync(mapping, view, cancellationToken, DefaultMetadata)).ShouldEqual(result.Result);
            initializeCount.ShouldEqual(1);

            initializeCount = 0;
            (await viewManager.InitializeAsync(ViewMapping.Undefined, new ViewModelViewRequest(null, null), cancellationToken, DefaultMetadata)).ShouldEqual(result.Result);
            initializeCount.ShouldEqual(1);
        }

        [Fact]
        public async Task TryCleanupAsyncShouldBeHandledByComponents()
        {
            var viewType = typeof(object);
            var viewModelType = typeof(TestViewModel);
            var mapping = new ViewMapping("id", viewType, viewModelType, DefaultMetadata);
            var view = new View(mapping, new object(), new TestViewModel());
            var viewModel = new TestViewModel();
            var result = Default.TrueTask;
            var invokeCount = 0;
            var cancellationToken = new CancellationTokenSource().Token;

            var viewManager = new ViewManager();
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryCleanupAsync = (v, r, meta, token) =>
                {
                    ++invokeCount;
                    v.ShouldEqual(view);
                    r.ShouldEqual(viewModel);
                    meta.ShouldEqual(DefaultMetadata);
                    token.ShouldEqual(cancellationToken);
                    return result;
                }
            });

            var component = new ViewModelViewInitializerDecorator();
            viewManager.AddComponent(component);

            var r = await viewManager.TryCleanupAsync(view, viewModel, cancellationToken, DefaultMetadata);
            r.ShouldEqual(result.Result);
            invokeCount.ShouldEqual(1);
        }

        #endregion
    }
}