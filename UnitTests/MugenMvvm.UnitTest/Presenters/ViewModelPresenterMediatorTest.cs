using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Navigation;
using MugenMvvm.Navigation.Components;
using MugenMvvm.Presenters;
using MugenMvvm.Requests;
using MugenMvvm.Threading;
using MugenMvvm.UnitTest.Navigation.Internal;
using MugenMvvm.UnitTest.Presenters.Internal;
using MugenMvvm.UnitTest.Threading.Internal;
using MugenMvvm.UnitTest.ViewModels.Internal;
using MugenMvvm.UnitTest.Views.Internal;
using MugenMvvm.Views;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Presenters
{
    public class ViewModelPresenterMediatorTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ShouldUseViewPresenter()
        {
            var viewRequest = new object();
            var vm = new TestViewModel();
            var mapping = new ViewMapping("id", typeof(object), vm.GetType(), DefaultMetadata);
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
                    return Task.FromResult<IView>(view);
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
            };
            viewPresenter.Activate = (p, v, m) =>
            {
                p.ShouldEqual(mediator);
                v.ShouldEqual(view.Target);
                m.ShouldNotBeNull();
                ++activateCount;
            };
            viewPresenter.Close = (p, v, m) =>
            {
                p.ShouldEqual(mediator);
                v.ShouldEqual(view.Target);
                m.ShouldNotBeNull();
                ++closeCount;
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

        [Fact]
        public void ShouldListenViewLifecycle()
        {
            var vm = new TestViewModel();
            var mapping = new ViewMapping("id", typeof(object), vm.GetType(), DefaultMetadata);
            var view = new View(mapping, new object(), vm);
            int navigatingCount = 0;
            int navigatedCount = 0;

            var viewPresenter = new TestViewPresenter();
            var navigationDispatcher = new NavigationDispatcher();
            navigationDispatcher.AddComponent(new NavigationContextProvider());
            navigationDispatcher.AddComponent(new TestNavigationDispatcherNavigatingListener
            {
                OnNavigating = context =>
                {
                    ++navigatingCount;
                }
            });
            navigationDispatcher.AddComponent(new TestNavigationDispatcherNavigatedListener
            {
                OnNavigated = context =>
                {
                    ++navigatedCount;
                }
            });
            var viewManager = new ViewManager();
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (viewMapping, r, token, m) => Task.FromResult<IView>(view)
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
            var mapping = new ViewMapping("id", typeof(object), vm.GetType(), DefaultMetadata);
            var view = new View(mapping, new object(), vm);

            var viewPresenter = new TestViewPresenter();
            var navigationDispatcher = new NavigationDispatcher();
            navigationDispatcher.AddComponent(new NavigationContextProvider());
            var viewManager = new ViewManager();
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (viewMapping, r, token, m) => Task.FromResult<IView>(view)
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
            var mapping = new ViewMapping("id", typeof(object), vm.GetType(), DefaultMetadata);
            var view = new View(mapping, new object(), vm);

            int navigatedCount = 0;
            var viewPresenter = new TestViewPresenter();
            var navigationDispatcher = new NavigationDispatcher();
            navigationDispatcher.AddComponent(new NavigationContextProvider());
            var viewManager = new ViewManager();
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (viewMapping, r, token, m) => Task.FromResult<IView>(view)
            });
            bool shown = false;
            navigationDispatcher.AddComponent(new TestNavigationDispatcherNavigatedListener
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
            var threadDispatcher = new ThreadDispatcher();
            threadDispatcher.AddComponent(new TestThreadDispatcherComponent());
            var mediator = new ViewModelPresenterMediator<object>(vm, mapping, viewPresenter, viewManager, null, navigationDispatcher, threadDispatcher);
            mediator.TryShow(null, default, DefaultMetadata);

            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Closed);
            navigatedCount.ShouldEqual(0);

            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Appeared);
            navigatedCount.ShouldEqual(1);

            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Disappeared);
            navigatedCount.ShouldEqual(2);
        }

        #endregion
    }
}