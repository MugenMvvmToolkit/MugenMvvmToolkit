using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using MugenMvvm.Navigation;
using MugenMvvm.Presenters;
using MugenMvvm.Presenters.Components;
using MugenMvvm.Requests;
using MugenMvvm.UnitTests.Navigation.Internal;
using MugenMvvm.UnitTests.Presenters.Internal;
using MugenMvvm.UnitTests.ViewModels.Internal;
using MugenMvvm.UnitTests.Views.Internal;
using MugenMvvm.ViewModels;
using MugenMvvm.Views;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Presenters.Components
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

        [Fact]
        public void ShouldIgnoreMultiMappings()
        {
            var view = new object();
            var presenterResult = new PresenterResult(this, "t", Default.NavigationProvider, NavigationType.Alert);

            var invokeCount = 0;
            var viewManager = new ViewManager();
            viewManager.AddComponent(new TestViewMappingProviderComponent
            {
                TryGetMappings = (o, arg4) =>
                {
                    ++invokeCount;
                    o.ShouldEqual(view);
                    arg4.ShouldEqual(DefaultMetadata);
                    return new[] {ViewMapping.Undefined, ViewMapping.Undefined};
                }
            });

            var presenter = new Presenter();
            presenter.AddComponent(new ViewPresenterDecorator(viewManager));
            presenter.AddComponent(new TestPresenterComponent(presenter)
            {
                TryShow = (o, arg4, arg5) =>
                {
                    o.ShouldEqual(view);
                    arg4.ShouldEqual(DefaultMetadata);
                    return presenterResult;
                }
            });
            presenter.TryShow(view, default, DefaultMetadata).ShouldEqual(presenterResult);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void ShouldIgnoreMultiViews()
        {
            var view = new object();
            var presenterResult = new PresenterResult(this, "t", Default.NavigationProvider, NavigationType.Alert);

            var invokeCount = 0;
            var viewManager = new ViewManager();
            viewManager.AddComponent(new TestViewProviderComponent
            {
                TryGetViews = (o, context) =>
                {
                    ++invokeCount;
                    o.ShouldEqual(view);
                    context.ShouldEqual(DefaultMetadata);
                    return new[] {new View(ViewMapping.Undefined, view, new TestViewModel()), new View(ViewMapping.Undefined, view, new TestViewModel())};
                }
            });

            var presenter = new Presenter();
            presenter.AddComponent(new ViewPresenterDecorator(viewManager));
            presenter.AddComponent(new TestPresenterComponent(presenter)
            {
                TryShow = (o, arg4, arg5) =>
                {
                    o.ShouldEqual(view);
                    arg4.ShouldEqual(DefaultMetadata);
                    return presenterResult;
                }
            });
            presenter.TryShow(view, default, DefaultMetadata).ShouldEqual(presenterResult);
            invokeCount.ShouldEqual(1);
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


        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldReuseViewModelForView(bool disposeViewModel)
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
            viewManager.AddComponent(new TestViewProviderComponent
            {
                TryGetViews = (o, context) =>
                {
                    o.ShouldEqual(view);
                    context.ShouldEqual(DefaultMetadata);
                    return viewObj;
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
            presenter.AddComponent(new ViewPresenterDecorator(viewManager, null, dispatcher) {DisposeViewModelOnClose = disposeViewModel});
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
            disposeCount.ShouldEqual(0);
        }


        [Fact]
        public void ShouldCloseMultiViews()
        {
            var view = new object();
            var v1 = new View(ViewMapping.Undefined, view, new TestViewModel());
            var v2 = new View(ViewMapping.Undefined, view, new TestViewModel());
            var presenterResult = new PresenterResult(this, "t", Default.NavigationProvider, NavigationType.Alert);

            var invokeCount = 0;
            var viewManager = new ViewManager();
            viewManager.AddComponent(new TestViewProviderComponent
            {
                TryGetViews = (o, context) =>
                {
                    ++invokeCount;
                    o.ShouldEqual(view);
                    context.ShouldEqual(DefaultMetadata);
                    return new[] {v1, v2};
                }
            });

            var closedVms = new HashSet<object>();
            var presenter = new Presenter();
            presenter.AddComponent(new ViewPresenterDecorator(viewManager));
            presenter.AddComponent(new TestPresenterComponent(presenter)
            {
                TryClose = (o, arg4, arg5) =>
                {
                    closedVms.Add(o);
                    arg4.ShouldEqual(DefaultMetadata);
                    return presenterResult;
                }
            });
            presenter.TryClose(view, default, DefaultMetadata).AsList().ShouldEqual(new[] {presenterResult, presenterResult});
            invokeCount.ShouldEqual(1);
            closedVms.Count.ShouldEqual(2);
            closedVms.Contains(v1.ViewModel).ShouldBeTrue();
            closedVms.Contains(v2.ViewModel).ShouldBeTrue();
        }

        #endregion
    }
}