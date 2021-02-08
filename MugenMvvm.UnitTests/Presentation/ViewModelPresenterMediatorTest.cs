using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Internal;
using MugenMvvm.Navigation;
using MugenMvvm.Navigation.Components;
using MugenMvvm.Presentation;
using MugenMvvm.Requests;
using MugenMvvm.UnitTests.Internal.Internal;
using MugenMvvm.UnitTests.Navigation.Internal;
using MugenMvvm.UnitTests.Presentation.Internal;
using MugenMvvm.UnitTests.ViewModels.Internal;
using MugenMvvm.UnitTests.Views.Internal;
using MugenMvvm.ViewModels;
using MugenMvvm.Views;
using MugenMvvm.Wrapping;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Presentation
{
    public class ViewModelPresenterMediatorTest : UnitTestBase
    {
        private readonly TestViewPresenterMediator _viewPresenter;
        private readonly TestViewModel _vm;
        private readonly View _view;
        private readonly ViewMapping _mapping;
        private readonly NavigationDispatcher _navigationDispatcher;
        private readonly WrapperManager _wrapperManager;
        private readonly ViewManager _viewManager;
        private readonly ViewModelManager _viewModelManager;
        private readonly ViewModelPresenterMediator<object> _mediator;

        public ViewModelPresenterMediatorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _viewPresenter = new TestViewPresenterMediator();
            _vm = new TestViewModel();
            _mapping = new ViewMapping("id", _vm.GetType(), typeof(object), DefaultMetadata);
            _view = new View(_mapping, new object(), _vm);
            _navigationDispatcher = new NavigationDispatcher(ComponentCollectionManager);
            _navigationDispatcher.AddComponent(new NavigationContextProvider());
            _viewManager = new ViewManager(ComponentCollectionManager);
            _viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (viewMapping, r, token, m) => new ValueTask<IView?>(_view)
            });
            _wrapperManager = new WrapperManager(ComponentCollectionManager);
            _viewModelManager = new ViewModelManager(ComponentCollectionManager);
            _mediator = new ViewModelPresenterMediator<object>(_vm, _mapping, _viewPresenter, _viewManager, _wrapperManager, _navigationDispatcher, ThreadDispatcher,
                _viewModelManager);
        }

        [Fact]
        public void ShouldCancelCloseReappearing()
        {
            var navigatedCount = 0;
            var shown = false;
            var canceled = false;
            _navigationDispatcher.AddComponent(new TestNavigationListener
            {
                OnNavigated = context =>
                {
                    if (!shown)
                    {
                        context.NavigationMode.ShouldEqual(NavigationMode.New);
                        shown = true;
                    }
                    else
                        context.NavigationMode.ShouldEqual(NavigationMode.Close);

                    ++navigatedCount;
                }
            });
            _navigationDispatcher.AddComponent(new TestNavigationErrorListener
            {
                OnNavigationCanceled = (context, token) =>
                {
                    canceled.ShouldBeFalse();
                    context.NavigationMode.ShouldEqual(NavigationMode.Close);
                    canceled = true;
                }
            });

            _mediator.TryShow(null, default, DefaultMetadata);

            _viewManager.OnLifecycleChanged(_view, ViewLifecycleState.Appeared);
            navigatedCount.ShouldEqual(1);

            _viewManager.OnLifecycleChanged(_view, ViewLifecycleState.Closing, new CancelableRequest());
            navigatedCount.ShouldEqual(1);

            _viewManager.OnLifecycleChanged(_view, ViewLifecycleState.Appearing);
            canceled.ShouldBeTrue();
        }

        [Fact]
        public void ShouldCloseAfterShow()
        {
            var navigatedCount = 0;
            var shown = false;
            _navigationDispatcher.AddComponent(new TestNavigationListener
            {
                OnNavigated = context =>
                {
                    ++navigatedCount;
                    if (!shown)
                    {
                        context.NavigationMode.ShouldEqual(NavigationMode.New);
                        shown = true;
                    }
                    else
                        context.NavigationMode.ShouldEqual(NavigationMode.Close);
                }
            });

            _mediator.TryShow(null, default, DefaultMetadata);

            _viewManager.OnLifecycleChanged(_view, ViewLifecycleState.Closed);
            navigatedCount.ShouldEqual(0);

            _viewManager.OnLifecycleChanged(_view, ViewLifecycleState.Appeared);
            navigatedCount.ShouldEqual(1);
            WaitCompletion();

            _viewManager.OnLifecycleChanged(_view, ViewLifecycleState.Disappeared);
            navigatedCount.ShouldEqual(2);
        }

        [Fact]
        public void ShouldInitCleanViewFromLifecycle()
        {
            _mediator.TryShow(null, default, DefaultMetadata);
            _viewManager.OnLifecycleChanged(_view, ViewLifecycleState.Appeared);
            _mediator.View.ShouldEqual(_view);

            _viewManager.OnLifecycleChanged(_view, ViewLifecycleState.Cleared);
            _mediator.View.ShouldBeNull();

            _viewManager.OnLifecycleChanged(_view, ViewLifecycleState.Initializing);
            _mediator.View.ShouldEqual(_view);
        }

        [Fact]
        public void ShouldListenViewLifecycle()
        {
            var navigatingCount = 0;
            var navigatedCount = 0;

            _navigationDispatcher.AddComponent(new TestNavigationListener
            {
                OnNavigating = context => { ++navigatingCount; },
                OnNavigated = context => { ++navigatedCount; }
            });


            _mediator.TryShow(null, default, DefaultMetadata);
            _mediator.View.ShouldEqual(_view);

            navigatingCount.ShouldEqual(1);
            navigatedCount.ShouldEqual(0);

            _viewManager.OnLifecycleChanged(_view, ViewLifecycleState.Appearing);
            navigatingCount.ShouldEqual(1);
            navigatedCount.ShouldEqual(0);

            _viewManager.OnLifecycleChanged(_view, ViewLifecycleState.Appeared);
            navigatingCount.ShouldEqual(1);
            navigatedCount.ShouldEqual(1);

            navigatingCount = navigatedCount = 0;
            _viewManager.OnLifecycleChanged(_view, ViewLifecycleState.Appearing);
            navigatingCount.ShouldEqual(1);
            navigatedCount.ShouldEqual(0);

            _viewManager.OnLifecycleChanged(_view, ViewLifecycleState.Appeared);
            navigatingCount.ShouldEqual(1);
            navigatedCount.ShouldEqual(1);

            navigatingCount = navigatedCount = 0;

            _viewManager.OnLifecycleChanged(_view, ViewLifecycleState.Closing, new CancelableRequest());
            navigatingCount.ShouldEqual(1);
            navigatedCount.ShouldEqual(0);

            _viewManager.OnLifecycleChanged(_view, ViewLifecycleState.Disappeared);
            navigatingCount.ShouldEqual(1);
            navigatedCount.ShouldEqual(1);
        }

        [Fact]
        public void ShouldUseViewPresenter()
        {
            var viewRequest = new object();
            var showCount = 0;
            var initializeCount = 0;
            var closeCount = 0;
            var activateCount = 0;
            var cleanupCount = 0;
            var getViewRequestCount = 0;

            _viewManager.RemoveComponents<TestViewManagerComponent>();
            _viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (viewMapping, r, token, m) =>
                {
                    r.ShouldEqual(viewRequest);
                    return new ValueTask<IView?>(_view);
                }
            });

            _viewPresenter.TryGetViewRequest = (p, v, m) =>
            {
                p.ShouldEqual(_mediator);
                m.ShouldNotBeNull();
                ++getViewRequestCount;
                return viewRequest;
            };
            _viewPresenter.Initialize = (p, v, m) =>
            {
                p.ShouldEqual(_mediator);
                v.ShouldEqual(_view.Target);
                m.ShouldNotBeNull();
                ++initializeCount;
            };
            _viewPresenter.Show = (p, v, m) =>
            {
                p.ShouldEqual(_mediator);
                v.ShouldEqual(_view.Target);
                m.ShouldNotBeNull();
                ++showCount;
                return Default.CompletedTask;
            };
            _viewPresenter.Activate = (p, v, m) =>
            {
                p.ShouldEqual(_mediator);
                v.ShouldEqual(_view.Target);
                m.ShouldNotBeNull();
                ++activateCount;
                return Default.CompletedTask;
            };
            _viewPresenter.Close = (p, v, m) =>
            {
                p.ShouldEqual(_mediator);
                v.ShouldEqual(_view.Target);
                m.ShouldNotBeNull();
                ++closeCount;
                return Default.CompletedTask;
            };
            _viewPresenter.Cleanup = (p, v, m) =>
            {
                p.ShouldEqual(_mediator);
                v.ShouldEqual(_view.Target);
                m.ShouldNotBeNull();
                ++cleanupCount;
            };

            _mediator.TryShow(null, default, DefaultMetadata);
            getViewRequestCount.ShouldEqual(1);
            initializeCount.ShouldEqual(1);
            showCount.ShouldEqual(1);
            activateCount.ShouldEqual(0);
            closeCount.ShouldEqual(0);
            cleanupCount.ShouldEqual(0);

            _viewManager.OnLifecycleChanged(_view, ViewLifecycleState.Appeared);
            _mediator.TryShow(null, default, DefaultMetadata);
            getViewRequestCount.ShouldEqual(1);
            initializeCount.ShouldEqual(1);
            showCount.ShouldEqual(1);
            activateCount.ShouldEqual(1);
            closeCount.ShouldEqual(0);
            cleanupCount.ShouldEqual(0);

            _viewManager.OnLifecycleChanged(_view, ViewLifecycleState.Appeared);
            _mediator.TryClose(null, default, DefaultMetadata);
            getViewRequestCount.ShouldEqual(1);
            initializeCount.ShouldEqual(1);
            showCount.ShouldEqual(1);
            activateCount.ShouldEqual(1);
            closeCount.ShouldEqual(1);
            cleanupCount.ShouldEqual(0);

            _viewManager.OnLifecycleChanged(_view, ViewLifecycleState.Disappeared);
            getViewRequestCount.ShouldEqual(1);
            initializeCount.ShouldEqual(1);
            showCount.ShouldEqual(1);
            activateCount.ShouldEqual(1);
            closeCount.ShouldEqual(1);
            cleanupCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(nameof(ViewLifecycleState.Initialized))]
        [InlineData(nameof(ViewLifecycleState.Closed))]
        [InlineData(nameof(ViewLifecycleState.Appeared))]
        [InlineData(nameof(ViewLifecycleState.Disappeared))]
        public void ShouldUseViewState(string stateString)
        {
            var state = ViewLifecycleState.Get(stateString);
            var navigatingCount = 0;
            var navigateCanceledCount = 0;
            var navigatedCount = 0;

            _navigationDispatcher.AddComponent(new TestNavigationListener
            {
                OnNavigating = context => ++navigatingCount,
                OnNavigated = context => ++navigatedCount
            });
            _navigationDispatcher.AddComponent(new TestNavigationErrorListener
            {
                OnNavigationCanceled = (context, token) => ++navigateCanceledCount
            });

            _viewManager.Components.TryAdd(new TestLifecycleTrackerComponent<ViewLifecycleState>
            {
                IsInState = (o, v, s, m) =>
                {
                    v.ShouldEqual(_view.Target);
                    return s == state;
                }
            });

            _mediator.TryShow(null, default, DefaultMetadata);

            if (state != ViewLifecycleState.Closed)
                _mediator.View.ShouldEqual(_view);

            navigatingCount.ShouldEqual(1);
            if (state == ViewLifecycleState.Appeared)
            {
                navigatedCount.ShouldEqual(1);
                navigateCanceledCount.ShouldEqual(0);
            }
            else if (state == ViewLifecycleState.Closed)
            {
                navigatedCount.ShouldEqual(0);
                navigateCanceledCount.ShouldEqual(1);
            }
            else if (state == ViewLifecycleState.Initialized || state == ViewLifecycleState.Disappeared)
            {
                navigatedCount.ShouldEqual(0);
                navigateCanceledCount.ShouldEqual(0);

                _viewManager.OnLifecycleChanged(_view, ViewLifecycleState.Appeared);
                navigateCanceledCount.ShouldEqual(0);
                navigatingCount.ShouldEqual(1);
                navigatedCount.ShouldEqual(1);
            }
        }
    }
}