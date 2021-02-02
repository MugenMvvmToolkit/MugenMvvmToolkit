using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
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
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Presenters.Components
{
    public class ViewPresenterDecoratorTest : UnitTestBase
    {
        private readonly ViewManager _viewManager;
        private readonly Presenter _presenter;
        private readonly ViewModelManager _viewModelManager;
        private readonly NavigationDispatcher _navigationDispatcher;
        private readonly ViewPresenterDecorator _viewPresenterDecorator;

        public ViewPresenterDecoratorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _viewManager = new ViewManager(ComponentCollectionManager);
            _presenter = new Presenter(ComponentCollectionManager);
            _viewModelManager = new ViewModelManager(ComponentCollectionManager);
            _navigationDispatcher = new NavigationDispatcher(ComponentCollectionManager);
            _viewPresenterDecorator = new ViewPresenterDecorator(_viewManager, _viewModelManager, _navigationDispatcher);
            _presenter.AddComponent(_viewPresenterDecorator);
        }

        [Fact]
        public void ShouldCloseMultiViews()
        {
            var view = new object();
            var v1 = new View(ViewMapping.Undefined, view, new TestViewModel());
            var v2 = new View(ViewMapping.Undefined, view, new TestViewModel());
            var presenterResult = new PresenterResult(this, "t", NavigationProvider.System, NavigationType.Alert);

            var invokeCount = 0;
            _viewManager.AddComponent(new TestViewProviderComponent
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
            _presenter.AddComponent(new TestPresenterComponent(_presenter)
            {
                TryClose = (o, arg4, arg5) =>
                {
                    closedVms.Add(o);
                    arg4.ShouldEqual(DefaultMetadata);
                    return presenterResult;
                }
            });
            _presenter.TryClose(view, default, DefaultMetadata).AsList().ShouldEqual(new[] {presenterResult, presenterResult});
            invokeCount.ShouldEqual(1);
            closedVms.Count.ShouldEqual(2);
            closedVms.Contains(v1.ViewModel).ShouldBeTrue();
            closedVms.Contains(v2.ViewModel).ShouldBeTrue();
        }

        [Fact]
        public void ShouldIgnoreMultiMappings()
        {
            var view = new object();
            var presenterResult = new PresenterResult(this, "t", NavigationProvider.System, NavigationType.Alert);
            var invokeCount = 0;
            _viewManager.AddComponent(new TestViewMappingProviderComponent
            {
                TryGetMappings = (o, arg4) =>
                {
                    ++invokeCount;
                    o.ShouldEqual(view);
                    arg4.ShouldEqual(DefaultMetadata);
                    return new[] {ViewMapping.Undefined, ViewMapping.Undefined};
                }
            });

            _presenter.AddComponent(new TestPresenterComponent(_presenter)
            {
                TryShow = (o, arg4, arg5) =>
                {
                    o.ShouldEqual(view);
                    arg4.ShouldEqual(DefaultMetadata);
                    return presenterResult;
                }
            });
            _presenter.TryShow(view, default, DefaultMetadata).ShouldEqual(presenterResult);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void ShouldIgnoreMultiViews()
        {
            var view = new object();
            var presenterResult = new PresenterResult(this, "t", NavigationProvider.System, NavigationType.Alert);

            var invokeCount = 0;
            _viewManager.AddComponent(new TestViewProviderComponent
            {
                TryGetViews = (o, context) =>
                {
                    ++invokeCount;
                    o.ShouldEqual(view);
                    context.ShouldEqual(DefaultMetadata);
                    return new[] {new View(ViewMapping.Undefined, view, new TestViewModel()), new View(ViewMapping.Undefined, view, new TestViewModel())};
                }
            });

            _presenter.AddComponent(new TestPresenterComponent(_presenter)
            {
                TryShow = (o, arg4, arg5) =>
                {
                    o.ShouldEqual(view);
                    arg4.ShouldEqual(DefaultMetadata);
                    return presenterResult;
                }
            });
            _presenter.TryShow(view, default, DefaultMetadata).ShouldEqual(presenterResult);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void ShouldIgnoreNonViewRequest()
        {
            var request = new TestViewModel();
            var presenterResult = new PresenterResult(this, "t", NavigationProvider.System, NavigationType.Alert);
            _presenter.AddComponent(new TestPresenterComponent(_presenter)
            {
                TryShow = (o, arg4, arg5) =>
                {
                    o.ShouldEqual(request);
                    arg4.ShouldEqual(DefaultMetadata);
                    return presenterResult;
                }
            });
            _presenter.TryShow(request, default, DefaultMetadata).ShouldEqual(presenterResult);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldCreateViewModelForViewBasedOnMapping(bool disposeViewModel)
        {
            var disposeCount = 0;
            var view = new object();
            var viewModel = new TestViewModel {Dispose = () => ++disposeCount};
            var mapping = new ViewMapping("d", typeof(TestViewModel), typeof(object));
            var presenterResult = new PresenterResult(viewModel, "t", NavigationProvider.System, NavigationType.Alert);
            var callback = new NavigationCallback(NavigationCallbackType.Close, "id", NavigationType.Alert);
            var request = view;

            _viewManager.AddComponent(new TestViewMappingProviderComponent
            {
                TryGetMappings = (o, arg4) =>
                {
                    o.ShouldEqual(request);
                    arg4.ShouldEqual(DefaultMetadata);
                    return mapping;
                }
            });
            _viewModelManager.AddComponent(new TestViewModelProviderComponent
            {
                TryGetViewModel = (o, arg4) =>
                {
                    o.ShouldEqual(mapping.ViewModelType);
                    arg4.ShouldEqual(DefaultMetadata);
                    return viewModel;
                }
            });
            _navigationDispatcher.AddComponent(new TestNavigationCallbackManagerComponent
            {
                TryGetNavigationCallbacks = (o, arg4) =>
                {
                    o.ShouldEqual(presenterResult);
                    arg4.ShouldEqual(DefaultMetadata);
                    return callback;
                }
            });

            _viewPresenterDecorator.DisposeViewModelOnClose = disposeViewModel;
            _presenter.AddComponent(new TestPresenterComponent(_presenter)
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
            _presenter.TryShow(request, default, DefaultMetadata).ShouldEqual(presenterResult);
            disposeCount.ShouldEqual(0);
            callback.SetResult(new NavigationContext(viewModel, NavigationProvider.System, "d", NavigationType.Alert, NavigationMode.Close));
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
            var mapping = new ViewMapping("d", typeof(TestViewModel), typeof(object));
            var presenterResult = new PresenterResult(viewModel, "t", NavigationProvider.System, NavigationType.Alert);
            var callback = new NavigationCallback(NavigationCallbackType.Close, "id", NavigationType.Alert);
            var viewObj = new View(mapping, view, viewModel);
            var request = view;

            _viewManager.AddComponent(new TestViewProviderComponent
            {
                TryGetViews = (o, context) =>
                {
                    o.ShouldEqual(view);
                    context.ShouldEqual(DefaultMetadata);
                    return viewObj;
                }
            });
            _navigationDispatcher.AddComponent(new TestNavigationCallbackManagerComponent
            {
                TryGetNavigationCallbacks = (o, arg4) =>
                {
                    o.ShouldEqual(presenterResult);
                    arg4.ShouldEqual(DefaultMetadata);
                    return callback;
                }
            });

            _viewPresenterDecorator.DisposeViewModelOnClose = disposeViewModel;
            _presenter.AddComponent(new TestPresenterComponent(_presenter)
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
            _presenter.TryShow(request, default, DefaultMetadata).ShouldEqual(presenterResult);
            disposeCount.ShouldEqual(0);
            callback.SetResult(new NavigationContext(viewModel, NavigationProvider.System, "d", NavigationType.Alert, NavigationMode.Close));
            disposeCount.ShouldEqual(0);
        }
    }
}