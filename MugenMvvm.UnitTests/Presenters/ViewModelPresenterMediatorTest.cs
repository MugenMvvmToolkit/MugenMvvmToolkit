using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Internal;
using MugenMvvm.Navigation;
using MugenMvvm.Navigation.Components;
using MugenMvvm.Presenters;
using MugenMvvm.Requests;
using MugenMvvm.Threading;
using MugenMvvm.UnitTests.Internal.Internal;
using MugenMvvm.UnitTests.Navigation.Internal;
using MugenMvvm.UnitTests.Presenters.Internal;
using MugenMvvm.UnitTests.Threading.Internal;
using MugenMvvm.UnitTests.ViewModels.Internal;
using MugenMvvm.UnitTests.Views.Internal;
using MugenMvvm.Views;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Presenters
{
    public class ViewModelPresenterMediatorTest : UnitTestBase
    {
        #region Constructors

        public ViewModelPresenterMediatorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
        }

        #endregion

        #region Methods

        [Fact]
        public void ShouldUseViewPresenter()
        {
            var viewRequest = new object();
            var vm = new TestViewModel();
            var mapping = new ViewMapping("id", vm.GetType(), typeof(object), DefaultMetadata);
            var view = new View(mapping, new object(), vm);
            var showCount = 0;
            var initializeCount = 0;
            var closeCount = 0;
            var activateCount = 0;
            var cleanupCount = 0;
            var getViewRequestCount = 0;

            var viewPresenter = new TestViewPresenter();
            var navigationDispatcher = new NavigationDispatcher();
            navigationDispatcher.AddComponent(new NavigationContextProvider());
            var viewManager = new ViewManager();
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (viewMapping, r, token, m) =>
                {
                    r.ShouldEqual(viewRequest);
                    return new ValueTask<IView?>(view);
                }
            });
            var threadDispatcher = new ThreadDispatcher();
            threadDispatcher.AddComponent(new TestThreadDispatcherComponent());
            var mediator = new ViewModelPresenterMediator<object>(vm, mapping, viewPresenter, viewManager, null, navigationDispatcher, threadDispatcher);

            viewPresenter.TryGetViewRequest = (p, v, m) =>
            {
                p.ShouldEqual(mediator);
                m.ShouldNotBeNull();
                ++getViewRequestCount;
                return viewRequest;
            };
            viewPresenter.Initialize = (p, v, m) =>
            {
                p.ShouldEqual(mediator);
                v.ShouldEqual(view.Target);
                m.ShouldNotBeNull();
                ++initializeCount;
            };
            viewPresenter.Show = (p, v, m) =>
            {
                p.ShouldEqual(mediator);
                v.ShouldEqual(view.Target);
                m.ShouldNotBeNull();
                ++showCount;
                return Default.CompletedTask;
            };
            viewPresenter.Activate = (p, v, m) =>
            {
                p.ShouldEqual(mediator);
                v.ShouldEqual(view.Target);
                m.ShouldNotBeNull();
                ++activateCount;
                return Default.CompletedTask;
            };
            viewPresenter.Close = (p, v, m) =>
            {
                p.ShouldEqual(mediator);
                v.ShouldEqual(view.Target);
                m.ShouldNotBeNull();
                ++closeCount;
                return Default.CompletedTask;
            };
            viewPresenter.Cleanup = (p, v, m) =>
            {
                p.ShouldEqual(mediator);
                v.ShouldEqual(view.Target);
                m.ShouldNotBeNull();
                ++cleanupCount;
            };

            mediator.TryShow(null, default, DefaultMetadata);
            getViewRequestCount.ShouldEqual(1);
            initializeCount.ShouldEqual(1);
            showCount.ShouldEqual(1);
            activateCount.ShouldEqual(0);
            closeCount.ShouldEqual(0);
            cleanupCount.ShouldEqual(0);

            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Appeared);
            mediator.TryShow(null, default, DefaultMetadata);
            getViewRequestCount.ShouldEqual(1);
            initializeCount.ShouldEqual(1);
            showCount.ShouldEqual(1);
            activateCount.ShouldEqual(1);
            closeCount.ShouldEqual(0);
            cleanupCount.ShouldEqual(0);

            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Appeared);
            mediator.TryClose(null, default, DefaultMetadata);
            getViewRequestCount.ShouldEqual(1);
            initializeCount.ShouldEqual(1);
            showCount.ShouldEqual(1);
            activateCount.ShouldEqual(1);
            closeCount.ShouldEqual(1);
            cleanupCount.ShouldEqual(0);

            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Disappeared);
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
            var vm = new TestViewModel();
            var mapping = new ViewMapping("id", vm.GetType(), typeof(object), DefaultMetadata);
            var view = new View(mapping, new object(), vm);
            var navigatingCount = 0;
            var navigateCanceledCount = 0;
            var navigatedCount = 0;

            var viewPresenter = new TestViewPresenter();
            var navigationDispatcher = new NavigationDispatcher();
            navigationDispatcher.AddComponent(new NavigationContextProvider());
            navigationDispatcher.AddComponent(new TestNavigationListener
            {
                OnNavigating = context => ++navigatingCount,
                OnNavigated = context => ++navigatedCount
            });
            navigationDispatcher.AddComponent(new TestNavigationErrorListener
            {
                OnNavigationCanceled = (context, token) => ++navigateCanceledCount
            });
            var viewManager = new ViewManager();
            viewManager.Components.Add(new TestLifecycleTrackerComponent<ViewLifecycleState>
            {
                IsInState = (o, v, s, m) =>
                {
                    v.ShouldEqual(view.Target);
                    return s == state;
                }
            });
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (viewMapping, r, token, m) => new ValueTask<IView?>(view)
            });
            var threadDispatcher = new ThreadDispatcher();
            threadDispatcher.AddComponent(new TestThreadDispatcherComponent());
            var mediator = new ViewModelPresenterMediator<object>(vm, mapping, viewPresenter, viewManager, null, navigationDispatcher, threadDispatcher);
            mediator.TryShow(null, default, DefaultMetadata);

            if (state != ViewLifecycleState.Closed)
                mediator.View.ShouldEqual(view);

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

                viewManager.OnLifecycleChanged(view, ViewLifecycleState.Appeared);
                navigateCanceledCount.ShouldEqual(0);
                navigatingCount.ShouldEqual(1);
                navigatedCount.ShouldEqual(1);
            }
        }

        [Fact]
        public void ShouldListenViewLifecycle()
        {
            var vm = new TestViewModel();
            var mapping = new ViewMapping("id", vm.GetType(), typeof(object), DefaultMetadata);
            var view = new View(mapping, new object(), vm);
            var navigatingCount = 0;
            var navigatedCount = 0;

            var viewPresenter = new TestViewPresenter();
            var navigationDispatcher = new NavigationDispatcher();
            navigationDispatcher.AddComponent(new NavigationContextProvider());
            navigationDispatcher.AddComponent(new TestNavigationListener
            {
                OnNavigating = context => { ++navigatingCount; },
                OnNavigated = context => { ++navigatedCount; }
            });
            var viewManager = new ViewManager();
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (viewMapping, r, token, m) => new ValueTask<IView?>(view)
            });
            var threadDispatcher = new ThreadDispatcher();
            threadDispatcher.AddComponent(new TestThreadDispatcherComponent());
            var mediator = new ViewModelPresenterMediator<object>(vm, mapping, viewPresenter, viewManager, null, navigationDispatcher, threadDispatcher);
            mediator.TryShow(null, default, DefaultMetadata);
            mediator.View.ShouldEqual(view);

            navigatingCount.ShouldEqual(1);
            navigatedCount.ShouldEqual(0);

            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Appearing);
            navigatingCount.ShouldEqual(1);
            navigatedCount.ShouldEqual(0);

            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Appeared);
            navigatingCount.ShouldEqual(1);
            navigatedCount.ShouldEqual(1);

            navigatingCount = navigatedCount = 0;
            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Appearing);
            navigatingCount.ShouldEqual(1);
            navigatedCount.ShouldEqual(0);

            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Appeared);
            navigatingCount.ShouldEqual(1);
            navigatedCount.ShouldEqual(1);

            navigatingCount = navigatedCount = 0;

            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Closing, new CancelableRequest());
            navigatingCount.ShouldEqual(1);
            navigatedCount.ShouldEqual(0);

            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Disappeared);
            navigatingCount.ShouldEqual(1);
            navigatedCount.ShouldEqual(1);
        }

        [Fact]
        public void ShouldInitCleanViewFromLifecycle()
        {
            var vm = new TestViewModel();
            var mapping = new ViewMapping("id", vm.GetType(), typeof(object), DefaultMetadata);
            var view = new View(mapping, new object(), vm);

            var viewPresenter = new TestViewPresenter();
            var navigationDispatcher = new NavigationDispatcher();
            navigationDispatcher.AddComponent(new NavigationContextProvider());
            var viewManager = new ViewManager();
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (viewMapping, r, token, m) => new ValueTask<IView?>(view)
            });
            var threadDispatcher = new ThreadDispatcher();
            threadDispatcher.AddComponent(new TestThreadDispatcherComponent());
            var mediator = new ViewModelPresenterMediator<object>(vm, mapping, viewPresenter, viewManager, null, navigationDispatcher, threadDispatcher);
            mediator.TryShow(null, default, DefaultMetadata);
            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Appeared);
            mediator.View.ShouldEqual(view);

            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Cleared);
            mediator.View.ShouldBeNull();

            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Initializing);
            mediator.View.ShouldEqual(view);
        }

        [Fact]
        public void ShouldCloseAfterShow()
        {
            var vm = new TestViewModel();
            var mapping = new ViewMapping("id", vm.GetType(), typeof(object), DefaultMetadata);
            var view = new View(mapping, new object(), vm);

            var navigatedCount = 0;
            var viewPresenter = new TestViewPresenter();
            var navigationDispatcher = new NavigationDispatcher();
            navigationDispatcher.AddComponent(new NavigationContextProvider());
            var viewManager = new ViewManager();
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (viewMapping, r, token, m) => new ValueTask<IView?>(view)
            });
            var shown = false;
            navigationDispatcher.AddComponent(new TestNavigationListener
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
            var threadDispatcher = new ThreadDispatcher();
            threadDispatcher.AddComponent(new TestThreadDispatcherComponent());
            var mediator = new ViewModelPresenterMediator<object>(vm, mapping, viewPresenter, viewManager, null, navigationDispatcher, threadDispatcher);
            mediator.TryShow(null, default, DefaultMetadata);

            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Closed);
            navigatedCount.ShouldEqual(0);

            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Appeared);
            navigatedCount.ShouldEqual(1);
            WaitCompletion();

            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Disappeared);
            navigatedCount.ShouldEqual(2);
        }

        [Fact]
        public void ShouldCancelCloseReappearing()
        {
            var vm = new TestViewModel();
            var mapping = new ViewMapping("id", vm.GetType(), typeof(object), DefaultMetadata);
            var view = new View(mapping, new object(), vm);

            var navigatedCount = 0;
            var viewPresenter = new TestViewPresenter();
            var navigationDispatcher = new NavigationDispatcher();
            navigationDispatcher.AddComponent(new NavigationContextProvider());
            var viewManager = new ViewManager();
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (viewMapping, r, token, m) => new ValueTask<IView?>(view)
            });
            var shown = false;
            var canceled = false;
            navigationDispatcher.AddComponent(new TestNavigationListener
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
            navigationDispatcher.AddComponent(new TestNavigationErrorListener
            {
                OnNavigationCanceled = (context, token) =>
                {
                    canceled.ShouldBeFalse();
                    context.NavigationMode.ShouldEqual(NavigationMode.Close);
                    canceled = true;
                }
            });
            var threadDispatcher = new ThreadDispatcher();
            threadDispatcher.AddComponent(new TestThreadDispatcherComponent());
            var mediator = new ViewModelPresenterMediator<object>(vm, mapping, viewPresenter, viewManager, null, navigationDispatcher, threadDispatcher);
            mediator.TryShow(null, default, DefaultMetadata);

            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Appeared);
            navigatedCount.ShouldEqual(1);

            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Closing, new CancelableRequest());
            navigatedCount.ShouldEqual(1);

            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Appearing);
            canceled.ShouldBeTrue();
        }

        #endregion
    }
}