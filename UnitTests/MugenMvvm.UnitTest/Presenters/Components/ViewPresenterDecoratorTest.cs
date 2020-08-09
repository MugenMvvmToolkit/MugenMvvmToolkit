using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Internal;
using MugenMvvm.Navigation;
using MugenMvvm.Presenters;
using MugenMvvm.Presenters.Components;
using MugenMvvm.Requests;
using MugenMvvm.UnitTest.Navigation.Internal;
using MugenMvvm.UnitTest.Presenters.Internal;
using MugenMvvm.UnitTest.ViewModels.Internal;
using MugenMvvm.UnitTest.Views.Internal;
using MugenMvvm.ViewModels;
using MugenMvvm.Views;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Presenters.Components
{
    public class ViewPresenterDecoratorTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ShouldIgnoreNonViewRequest()
        {
            var request = new TestViewModel();
            var presenterResult = new PresenterResult(this, "t", Default.NavigationProvider, NavigationType.Alert);

            var presenter = new Presenter();
            presenter.AddComponent(new ViewPresenterDecorator());
            presenter.AddComponent(new TestPresenterComponent(presenter)
            {
                TryShow = (o, arg4, arg5) =>
                {
                    o.ShouldEqual(request);
                    arg4.ShouldEqual(DefaultMetadata);
                    return presenterResult;
                }
            });
            presenter.TryShow(request, default, DefaultMetadata).ShouldEqual(presenterResult);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldCreateViewModelForViewBasedOnMapping(bool disposeViewModel)
        {
            var disposeCount = 0;
            var view = new object();
            var viewModel = new TestViewModel {Dispose = () => ++disposeCount};
            var mapping = new ViewMapping("d", typeof(object), typeof(TestViewModel));
            var presenterResult = new PresenterResult(viewModel, "t", Default.NavigationProvider, NavigationType.Alert);
            var callback = new NavigationCallback(NavigationCallbackType.Close, "id", NavigationType.Alert);
            var viewObj = new View(mapping, view, viewModel);
            var request = view;

            var viewManager = new ViewManager();
            viewManager.AddComponent(new TestViewMappingProviderComponent
            {
                TryGetMappings = (o, arg4) =>
                {
                    o.ShouldEqual(request);
                    arg4.ShouldEqual(DefaultMetadata);
                    return mapping;
                }
            });
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (viewMapping, arg3, arg5, arg6) =>
                {
                    viewMapping.ShouldEqual(mapping);
                    arg3.ShouldEqual(new ViewModelViewRequest(viewModel, view));
                    arg5.ShouldEqual(DefaultMetadata);
                    return Task.FromResult<IView>(viewObj);
                }
            });
            var viewModelManager = new ViewModelManager();
            viewModelManager.AddComponent(new TestViewModelProviderComponent
            {
                TryGetViewModel = (o, arg4) =>
                {
                    o.ShouldEqual(mapping.ViewModelType);
                    arg4.ShouldEqual(DefaultMetadata);
                    return viewModel;
                }
            });

            var dispatcher = new NavigationDispatcher();
            dispatcher.AddComponent(new TestNavigationCallbackManagerComponent
            {
                TryGetNavigationCallbacks = (o, arg4) =>
                {
                    o.ShouldEqual(presenterResult);
                    arg4.ShouldEqual(DefaultMetadata);
                    return callback;
                }
            });
            var presenter = new Presenter();
            presenter.AddComponent(new ViewPresenterDecorator(viewManager, viewModelManager, dispatcher) {DisposeViewModelOnClose = disposeViewModel});
            presenter.AddComponent(new TestPresenterComponent(presenter)
            {
                TryShow = (o, arg4, arg5) =>
                {
                    var viewRequest = (ViewModelViewRequest) o;
                    viewRequest.ViewModel.ShouldEqual(viewModel);
                    viewRequest.View.ShouldEqual(view);
                    arg4.ShouldEqual(DefaultMetadata);
                    return presenterResult;
                }
            });
            presenter.TryShow(request, default, DefaultMetadata).ShouldEqual(presenterResult);
            disposeCount.ShouldEqual(0);
            callback.SetResult(new NavigationContext(viewModel, Default.NavigationProvider, "d", NavigationType.Alert, NavigationMode.Close));
            disposeCount.ShouldEqual(disposeViewModel ? 1 : 0);
        }

        #endregion
    }
}