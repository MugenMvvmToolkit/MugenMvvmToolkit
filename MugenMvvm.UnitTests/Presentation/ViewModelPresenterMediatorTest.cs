using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Navigation;
using MugenMvvm.Navigation.Components;
using MugenMvvm.Presentation;
using MugenMvvm.Requests;
using MugenMvvm.Tests.Internal;
using MugenMvvm.Tests.Navigation;
using MugenMvvm.Tests.Presentation;
using MugenMvvm.Tests.ViewModels;
using MugenMvvm.Tests.Views;
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
        private readonly View _view;

        private readonly ViewModelPresenterMediator<object> _mediator;

        public ViewModelPresenterMediatorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _viewPresenter = new TestViewPresenterMediator();
            var vm = new TestViewModel();
            var mapping = new ViewMapping("id", vm.GetType(), typeof(object), Metadata);
            _view = new View(mapping, new object(), vm);
            NavigationDispatcher.AddComponent(new NavigationContextProvider());
            ViewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (_, _, _, _, _) => new ValueTask<IView?>(_view)
            });

            _mediator = new ViewModelPresenterMediator<object>(vm, mapping, _viewPresenter, ViewManager, WrapperManager, NavigationDispatcher, ThreadDispatcher,
                ViewModelManager);
        }

        [Fact]
        public void ShouldCancelCloseReappearing()
        {
            var navigatedCount = 0;
            var shown = false;
            var canceled = false;
            NavigationDispatcher.AddComponent(new TestNavigationListener
            {
                OnNavigated = (_, context) =>
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
            NavigationDispatcher.AddComponent(new TestNavigationErrorListener
            {
                OnNavigationCanceled = (_, context, token) =>
                {
                    canceled.ShouldBeFalse();
                    context.NavigationMode.ShouldEqual(NavigationMode.Close);
                    canceled = true;
                }
            });

            _mediator.TryShow(null, default, Metadata);

            ViewManager.OnLifecycleChanged(_view, ViewLifecycleState.Appeared);
            navigatedCount.ShouldEqual(1);

            ViewManager.OnLifecycleChanged(_view, ViewLifecycleState.Closing, new CancelableRequest());
            navigatedCount.ShouldEqual(1);

            ViewManager.OnLifecycleChanged(_view, ViewLifecycleState.Appearing);
            canceled.ShouldBeTrue();
        }

        [Fact]
        public void ShouldCloseAfterShow()
        {
            var navigatedCount = 0;
            var shown = false;
            NavigationDispatcher.AddComponent(new TestNavigationListener
            {
                OnNavigated = (_, context) =>
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

            _mediator.TryShow(null, default, Metadata);

            ViewManager.OnLifecycleChanged(_view, ViewLifecycleState.Closed);
            navigatedCount.ShouldEqual(0);

            ViewManager.OnLifecycleChanged(_view, ViewLifecycleState.Appeared);
            navigatedCount.ShouldEqual(1);
            WaitCompletion();

            ViewManager.OnLifecycleChanged(_view, ViewLifecycleState.Disappeared);
            navigatedCount.ShouldEqual(2);
        }

        [Fact]
        public void ShouldInitCleanViewFromLifecycle()
        {
            _mediator.TryShow(null, default, Metadata);
            ViewManager.OnLifecycleChanged(_view, ViewLifecycleState.Appeared);
            _mediator.View.ShouldEqual(_view);

            ViewManager.OnLifecycleChanged(_view, ViewLifecycleState.Cleared);
            _mediator.View.ShouldBeNull();

            ViewManager.OnLifecycleChanged(_view, ViewLifecycleState.Initializing);
            _mediator.View.ShouldEqual(_view);
        }

        [Fact]
        public void ShouldListenViewLifecycle()
        {
            var navigatingCount = 0;
            var navigatedCount = 0;

            NavigationDispatcher.AddComponent(new TestNavigationListener
            {
                OnNavigating = (_, _) => { ++navigatingCount; },
                OnNavigated = (_, _) => { ++navigatedCount; }
            });


            _mediator.TryShow(null, default, Metadata);
            _mediator.View.ShouldEqual(_view);

            navigatingCount.ShouldEqual(1);
            navigatedCount.ShouldEqual(0);

            ViewManager.OnLifecycleChanged(_view, ViewLifecycleState.Appearing);
            navigatingCount.ShouldEqual(1);
            navigatedCount.ShouldEqual(0);

            ViewManager.OnLifecycleChanged(_view, ViewLifecycleState.Appeared);
            navigatingCount.ShouldEqual(1);
            navigatedCount.ShouldEqual(1);

            navigatingCount = navigatedCount = 0;
            ViewManager.OnLifecycleChanged(_view, ViewLifecycleState.Appearing);
            navigatingCount.ShouldEqual(1);
            navigatedCount.ShouldEqual(0);

            ViewManager.OnLifecycleChanged(_view, ViewLifecycleState.Appeared);
            navigatingCount.ShouldEqual(1);
            navigatedCount.ShouldEqual(1);

            navigatingCount = navigatedCount = 0;

            ViewManager.OnLifecycleChanged(_view, ViewLifecycleState.Closing, new CancelableRequest());
            navigatingCount.ShouldEqual(1);
            navigatedCount.ShouldEqual(0);

            ViewManager.OnLifecycleChanged(_view, ViewLifecycleState.Disappeared);
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

            ViewManager.RemoveComponents<TestViewManagerComponent>();
            ViewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (_, _, r, _, _) =>
                {
                    r.ShouldEqual(viewRequest);
                    return new ValueTask<IView?>(_view);
                }
            });

            _viewPresenter.TryGetViewRequest = (p, _, m) =>
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
            _viewPresenter.Show = (p, v, m, c) =>
            {
                p.ShouldEqual(_mediator);
                v.ShouldEqual(_view.Target);
                m.ShouldNotBeNull();
                ++showCount;
                return Task.CompletedTask;
            };
            _viewPresenter.Activate = (p, v, m, c) =>
            {
                p.ShouldEqual(_mediator);
                v.ShouldEqual(_view.Target);
                m.ShouldNotBeNull();
                ++activateCount;
                return Task.CompletedTask;
            };
            _viewPresenter.Close = (p, v, m, c) =>
            {
                p.ShouldEqual(_mediator);
                v.ShouldEqual(_view.Target);
                m.ShouldNotBeNull();
                ++closeCount;
                return Task.CompletedTask;
            };
            _viewPresenter.Cleanup = (p, v, m) =>
            {
                p.ShouldEqual(_mediator);
                v.ShouldEqual(_view.Target);
                m.ShouldNotBeNull();
                ++cleanupCount;
            };

            _mediator.TryShow(null, default, Metadata);
            getViewRequestCount.ShouldEqual(1);
            initializeCount.ShouldEqual(1);
            showCount.ShouldEqual(1);
            activateCount.ShouldEqual(0);
            closeCount.ShouldEqual(0);
            cleanupCount.ShouldEqual(0);

            ViewManager.OnLifecycleChanged(_view, ViewLifecycleState.Appeared);
            _mediator.TryShow(null, default, Metadata);
            getViewRequestCount.ShouldEqual(1);
            initializeCount.ShouldEqual(1);
            showCount.ShouldEqual(1);
            activateCount.ShouldEqual(1);
            closeCount.ShouldEqual(0);
            cleanupCount.ShouldEqual(0);

            ViewManager.OnLifecycleChanged(_view, ViewLifecycleState.Appeared);
            _mediator.TryClose(null, default, Metadata);
            getViewRequestCount.ShouldEqual(1);
            initializeCount.ShouldEqual(1);
            showCount.ShouldEqual(1);
            activateCount.ShouldEqual(1);
            closeCount.ShouldEqual(1);
            cleanupCount.ShouldEqual(0);

            ViewManager.OnLifecycleChanged(_view, ViewLifecycleState.Disappeared);
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

            NavigationDispatcher.AddComponent(new TestNavigationListener
            {
                OnNavigating = (_, _) => ++navigatingCount,
                OnNavigated = (_, _) => ++navigatedCount
            });
            NavigationDispatcher.AddComponent(new TestNavigationErrorListener
            {
                OnNavigationCanceled = (_, _, _) => ++navigateCanceledCount
            });

            ViewManager.Components.TryAdd(new TestLifecycleTrackerComponent<IViewManager, ViewLifecycleState>
            {
                IsInState = (_, v, s, _) =>
                {
                    v.ShouldEqual(_view.Target);
                    return s == state;
                }
            });

            _mediator.TryShow(null, default, Metadata);

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

                ViewManager.OnLifecycleChanged(_view, ViewLifecycleState.Appeared);
                navigateCanceledCount.ShouldEqual(0);
                navigatingCount.ShouldEqual(1);
                navigatedCount.ShouldEqual(1);
            }
        }

        protected override INavigationDispatcher GetNavigationDispatcher() => new NavigationDispatcher(ComponentCollectionManager);

        protected override IViewManager GetViewManager() => new ViewManager(ComponentCollectionManager);

        protected override IWrapperManager GetWrapperManager() => new WrapperManager(ComponentCollectionManager);

        protected override IViewModelManager GetViewModelManager() => new ViewModelManager(ComponentCollectionManager);
    }
}