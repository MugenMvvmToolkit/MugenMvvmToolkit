using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presentation;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Navigation;
using MugenMvvm.Presentation;
using MugenMvvm.Presentation.Components;
using MugenMvvm.Requests;
using MugenMvvm.Tests.Navigation;
using MugenMvvm.Tests.Presentation;
using MugenMvvm.Tests.ViewModels;
using MugenMvvm.Tests.Views;
using MugenMvvm.ViewModels;
using MugenMvvm.Views;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Presentation.Components
{
    public class ViewPresenterDecoratorTest : UnitTestBase
    {
        private readonly ViewPresenterDecorator _viewPresenterDecorator;

        public ViewPresenterDecoratorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _viewPresenterDecorator = new ViewPresenterDecorator(ViewManager, ViewModelManager, NavigationDispatcher);
            Presenter.AddComponent(_viewPresenterDecorator);
        }

        [Fact]
        public void ShouldCloseMultiViews()
        {
            var view = new object();
            var v1 = new View(ViewMapping.Undefined, view, new TestViewModel());
            var v2 = new View(ViewMapping.Undefined, view, new TestViewModel());
            var presenterResult = new PresenterResult(this, "t", NavigationProvider.System, NavigationType.Alert);

            var invokeCount = 0;
            ViewManager.AddComponent(new TestViewProviderComponent
            {
                TryGetViews = (_, o, context) =>
                {
                    ++invokeCount;
                    o.ShouldEqual(view);
                    context.ShouldEqual(DefaultMetadata);
                    return new[] { v1, v2 };
                }
            });

            var closedVms = new HashSet<object>();
            Presenter.AddComponent(new TestPresenterComponent
            {
                TryClose = (_, o, arg4, arg5) =>
                {
                    closedVms.Add(o);
                    arg4.ShouldEqual(DefaultMetadata);
                    return presenterResult;
                }
            });
            Presenter.TryClose(view, default, DefaultMetadata).ShouldEqual(new[] { presenterResult, presenterResult });
            invokeCount.ShouldEqual(1);
            closedVms.Count.ShouldEqual(2);
            closedVms.Contains(v1.ViewModel).ShouldBeTrue();
            closedVms.Contains(v2.ViewModel).ShouldBeTrue();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldCreateViewModelForViewBasedOnMapping(bool disposeViewModel)
        {
            var disposeCount = 0;
            var view = new object();
            var viewModel = new TestViewModel { Dispose = () => ++disposeCount };
            var mapping = new ViewMapping("d", typeof(TestViewModel), typeof(object));
            var presenterResult = new PresenterResult(viewModel, "t", NavigationProvider.System, NavigationType.Alert);
            var callback = new NavigationCallback(NavigationCallbackType.Close, "id", NavigationType.Alert);
            var request = view;

            ViewManager.AddComponent(new TestViewMappingProviderComponent
            {
                TryGetMappings = (_, o, arg4) =>
                {
                    o.ShouldEqual(request);
                    arg4.ShouldEqual(DefaultMetadata);
                    return mapping;
                }
            });
            ViewModelManager.AddComponent(new TestViewModelProviderComponent
            {
                TryGetViewModel = (_, o, arg4) =>
                {
                    o.ShouldEqual(mapping.ViewModelType);
                    arg4.ShouldEqual(DefaultMetadata);
                    return viewModel;
                }
            });
            NavigationDispatcher.AddComponent(new TestNavigationCallbackManagerComponent
            {
                TryGetNavigationCallbacks = (_, o, arg4) =>
                {
                    o.ShouldEqual(presenterResult);
                    arg4.ShouldEqual(DefaultMetadata);
                    return callback;
                }
            });

            _viewPresenterDecorator.DisposeViewModelOnClose = disposeViewModel;
            Presenter.AddComponent(new TestPresenterComponent
            {
                TryShow = (_, o, arg4, arg5) =>
                {
                    var viewRequest = (ViewModelViewRequest)o;
                    viewRequest.ViewModel.ShouldEqual(viewModel);
                    viewRequest.View.ShouldEqual(view);
                    arg4.ShouldEqual(DefaultMetadata);
                    return presenterResult;
                }
            });
            Presenter.TryShow(request, default, DefaultMetadata).ShouldEqual(presenterResult);
            disposeCount.ShouldEqual(0);
            callback.SetResult(new NavigationContext(viewModel, NavigationProvider.System, "d", NavigationType.Alert, NavigationMode.Close));
            disposeCount.ShouldEqual(disposeViewModel ? 1 : 0);
        }

        [Fact]
        public void ShouldIgnoreMultiMappings()
        {
            var view = new object();
            var presenterResult = new PresenterResult(this, "t", NavigationProvider.System, NavigationType.Alert);
            var invokeCount = 0;
            ViewManager.AddComponent(new TestViewMappingProviderComponent
            {
                TryGetMappings = (_, o, arg4) =>
                {
                    ++invokeCount;
                    o.ShouldEqual(view);
                    arg4.ShouldEqual(DefaultMetadata);
                    return new[] { ViewMapping.Undefined, ViewMapping.Undefined };
                }
            });

            Presenter.AddComponent(new TestPresenterComponent
            {
                TryShow = (_, o, arg4, arg5) =>
                {
                    o.ShouldEqual(view);
                    arg4.ShouldEqual(DefaultMetadata);
                    return presenterResult;
                }
            });
            Presenter.TryShow(view, default, DefaultMetadata).ShouldEqual(presenterResult);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void ShouldIgnoreMultiViews()
        {
            var view = new object();
            var presenterResult = new PresenterResult(this, "t", NavigationProvider.System, NavigationType.Alert);

            var invokeCount = 0;
            ViewManager.AddComponent(new TestViewProviderComponent
            {
                TryGetViews = (_, o, context) =>
                {
                    ++invokeCount;
                    o.ShouldEqual(view);
                    context.ShouldEqual(DefaultMetadata);
                    return new[] { new View(ViewMapping.Undefined, view, new TestViewModel()), new View(ViewMapping.Undefined, view, new TestViewModel()) };
                }
            });

            Presenter.AddComponent(new TestPresenterComponent
            {
                TryShow = (_, o, arg4, arg5) =>
                {
                    o.ShouldEqual(view);
                    arg4.ShouldEqual(DefaultMetadata);
                    return presenterResult;
                }
            });
            Presenter.TryShow(view, default, DefaultMetadata).ShouldEqual(presenterResult);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void ShouldIgnoreNonViewRequest()
        {
            var request = new TestViewModel();
            var presenterResult = new PresenterResult(this, "t", NavigationProvider.System, NavigationType.Alert);
            Presenter.AddComponent(new TestPresenterComponent
            {
                TryShow = (_, o, arg4, arg5) =>
                {
                    o.ShouldEqual(request);
                    arg4.ShouldEqual(DefaultMetadata);
                    return presenterResult;
                }
            });
            Presenter.TryShow(request, default, DefaultMetadata).ShouldEqual(presenterResult);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldReuseViewModelForView(bool disposeViewModel)
        {
            var disposeCount = 0;
            var view = new object();
            var viewModel = new TestViewModel { Dispose = () => ++disposeCount };
            var mapping = new ViewMapping("d", typeof(TestViewModel), typeof(object));
            var presenterResult = new PresenterResult(viewModel, "t", NavigationProvider.System, NavigationType.Alert);
            var callback = new NavigationCallback(NavigationCallbackType.Close, "id", NavigationType.Alert);
            var viewObj = new View(mapping, view, viewModel);
            var request = view;

            ViewManager.AddComponent(new TestViewProviderComponent
            {
                TryGetViews = (_, o, context) =>
                {
                    o.ShouldEqual(view);
                    context.ShouldEqual(DefaultMetadata);
                    return viewObj;
                }
            });
            NavigationDispatcher.AddComponent(new TestNavigationCallbackManagerComponent
            {
                TryGetNavigationCallbacks = (_, o, arg4) =>
                {
                    o.ShouldEqual(presenterResult);
                    arg4.ShouldEqual(DefaultMetadata);
                    return callback;
                }
            });

            _viewPresenterDecorator.DisposeViewModelOnClose = disposeViewModel;
            Presenter.AddComponent(new TestPresenterComponent
            {
                TryShow = (_, o, arg4, arg5) =>
                {
                    var viewRequest = (ViewModelViewRequest)o;
                    viewRequest.ViewModel.ShouldEqual(viewModel);
                    viewRequest.View.ShouldEqual(view);
                    arg4.ShouldEqual(DefaultMetadata);
                    return presenterResult;
                }
            });
            Presenter.TryShow(request, default, DefaultMetadata).ShouldEqual(presenterResult);
            disposeCount.ShouldEqual(0);
            callback.SetResult(new NavigationContext(viewModel, NavigationProvider.System, "d", NavigationType.Alert, NavigationMode.Close));
            disposeCount.ShouldEqual(0);
        }

        protected override IViewManager GetViewManager() => new ViewManager(ComponentCollectionManager);

        protected override IPresenter GetPresenter() => new Presenter(ComponentCollectionManager);

        protected override IViewModelManager GetViewModelManager() => new ViewModelManager(ComponentCollectionManager);

        protected override INavigationDispatcher GetNavigationDispatcher() => new NavigationDispatcher(ComponentCollectionManager);
    }
}