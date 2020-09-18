using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;
using MugenMvvm.Navigation;
using MugenMvvm.Navigation.Components;
using MugenMvvm.Requests;
using MugenMvvm.Threading;
using MugenMvvm.UnitTests.Navigation.Internal;
using MugenMvvm.UnitTests.Presenters.Internal;
using MugenMvvm.UnitTests.Threading.Internal;
using MugenMvvm.UnitTests.ViewModels.Internal;
using MugenMvvm.UnitTests.Views.Internal;
using MugenMvvm.Views;
using MugenMvvm.Wrapping;
using MugenMvvm.Wrapping.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Presenters
{
    public class ViewModelPresenterMediatorBaseTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ConstructorShouldInitializeFields()
        {
            var vm = new TestViewModel();
            var mapping = new ViewMapping("id", typeof(object), vm.GetType(), DefaultMetadata);
            var mediator = new TestViewModelPresenterMediatorBase<object>(vm, mapping);
            mediator.Id.ShouldEqual($"{mediator.GetType().FullName}{mapping.Id}");
            mediator.ViewModel.ShouldEqual(vm);
            mediator.Mapping.ShouldEqual(mapping);
            mediator.View.ShouldBeNull();
            mediator.CurrentView.ShouldBeNull();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void TryShowShouldNotifyNavigationDispatcher(int state)
        {
            var vm = new TestViewModel();
            var mapping = new ViewMapping("id", typeof(object), vm.GetType(), DefaultMetadata);
            var view = new View(mapping, new object(), vm);
            var navigationContext = new NavigationContext(this, Default.NavigationProvider, "t", NavigationType.Popup, NavigationMode.New);
            var exception = new Exception();
            var cts = new CancellationTokenSource();
            string? navigationId = null;
            var navigatingCount = 0;
            var navigatedCount = 0;
            var navigatingConditionCount = 0;
            var errorCount = 0;
            var cancelCount = 0;
            var contextCount = 0;

            var navigationDispatcher = new NavigationDispatcher();
            var viewManager = new ViewManager();
            var threadDispatcher = new ThreadDispatcher();
            var mediator = new TestViewModelPresenterMediatorBase<object>(vm, mapping, viewManager, null, navigationDispatcher, threadDispatcher);
            mediator.ShowViewHandler = context =>
            {
                mediator.OnViewShown(DefaultMetadata);
                return null;
            };
            navigationDispatcher.AddComponent(new TestNavigationContextProviderComponent
            {
                TryGetNavigationContext = (o, provider, nId, type, mode, m) =>
                {
                    ++contextCount;
                    o.ShouldEqual(vm);
                    provider.ShouldEqual(mediator);
                    navigationId = nId;
                    type.ShouldEqual(mediator.NavigationType);
                    mode.ShouldEqual(NavigationMode.New);
                    m.ShouldEqual(DefaultMetadata);
                    return navigationContext;
                }
            });
            navigationDispatcher.AddComponent(new TestNavigationDispatcherErrorListener
            {
                OnNavigationCanceled = (context, arg3) =>
                {
                    ++cancelCount;
                    context.ShouldEqual(navigationContext);
                    arg3.ShouldEqual(cts.Token);
                },
                OnNavigationFailed = (context, arg3) =>
                {
                    ++errorCount;
                    context.ShouldEqual(navigationContext);
                    arg3.ShouldEqual(exception);
                }
            });
            navigationDispatcher.AddComponent(new TestConditionNavigationDispatcherComponent
            {
                CanNavigateAsync = (context, t) =>
                {
                    ++navigatingConditionCount;
                    context.ShouldEqual(navigationContext);
                    return null;
                }
            });
            navigationDispatcher.AddComponent(new TestNavigationDispatcherNavigatingListener
            {
                OnNavigating = context =>
                {
                    ++navigatingCount;
                    context.ShouldEqual(navigationContext);
                }
            });
            navigationDispatcher.AddComponent(new TestNavigationDispatcherNavigatedListener
            {
                OnNavigated = context =>
                {
                    ++navigatedCount;
                    context.ShouldEqual(navigationContext);
                }
            });
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (viewMapping, r, token, m) =>
                {
                    if (state == 1)
                        return new ValueTask<IView?>(view);
                    if (state == 2)
                    {
                        cts.Cancel();
                        return Task.FromCanceled<IView?>(cts.Token).AsValueTask();
                    }

                    return exception.ToTask<IView?>().AsValueTask();
                }
            });
            threadDispatcher.AddComponent(new TestThreadDispatcherComponent());
            var presenterResult = mediator.TryShow(null, cts.Token, DefaultMetadata)!;
            contextCount.ShouldEqual(1);
            presenterResult.NavigationProvider.ShouldEqual(mediator);
            presenterResult.NavigationType.ShouldEqual(mediator.NavigationType);
            presenterResult.Target.ShouldEqual(vm);
            presenterResult.NavigationId.ShouldEqual(navigationId);
            navigatingCount.ShouldEqual(1);
            navigatingConditionCount.ShouldEqual(1);

            if (state == 1)
            {
                navigatedCount.ShouldEqual(1);
                cancelCount.ShouldEqual(0);
                errorCount.ShouldEqual(0);
            }
            else if (state == 2)
            {
                navigatedCount.ShouldEqual(0);
                cancelCount.ShouldEqual(1);
                errorCount.ShouldEqual(0);
            }
            else
            {
                navigatedCount.ShouldEqual(0);
                cancelCount.ShouldEqual(0);
                errorCount.ShouldEqual(1);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TryShowShouldRequestViewInitialization(bool includeView)
        {
            var vm = new TestViewModel();
            var mapping = new ViewMapping("id", typeof(object), vm.GetType(), DefaultMetadata);
            var view = new View(mapping, new object(), vm);
            var cts = new CancellationTokenSource();
            var initCount = 0;

            var navigationDispatcher = new NavigationDispatcher();
            var viewManager = new ViewManager();
            var threadDispatcher = new ThreadDispatcher();
            var mediator = new TestViewModelPresenterMediatorBase<object>(vm, mapping, viewManager, null, navigationDispatcher, threadDispatcher);
            mediator.InitializeViewHandler = context => { ++initCount; };
            mediator.ShowViewHandler = context =>
            {
                mediator.OnViewShown(DefaultMetadata);
                return null;
            };
            navigationDispatcher.AddComponent(new NavigationContextProvider());
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (viewMapping, r, m, token) =>
                {
                    viewMapping.ShouldEqual(mapping);
                    if (includeView)
                    {
                        var request = (ViewModelViewRequest) r;
                        request.ViewModel.ShouldEqual(vm);
                        request.View.ShouldEqual(view.Target);
                    }
                    else
                        r.ShouldEqual(vm);

                    token.CanBeCanceled.ShouldBeTrue();
                    return new ValueTask<IView?>(view);
                }
            });
            threadDispatcher.AddComponent(new TestThreadDispatcherComponent());
            mediator.TryShow(includeView ? view.Target : null, cts.Token, DefaultMetadata);
            initCount.ShouldEqual(1);
            mediator.View.ShouldEqual(view);
            mediator.CurrentView.ShouldEqual(view.Target);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TryShowShouldWrapView(bool includeView)
        {
            var vm = new TestViewModel();
            var mapping = new ViewMapping("id", typeof(object), vm.GetType(), DefaultMetadata);
            var view = new View(mapping, new object(), vm);
            var cts = new CancellationTokenSource();
            var initCount = 0;

            var navigationDispatcher = new NavigationDispatcher();
            var viewManager = new ViewManager();
            var threadDispatcher = new ThreadDispatcher();
            var wrapperManager = new WrapperManager();
            var mediator = new TestViewModelPresenterMediatorBase<ViewModelPresenterMediatorBaseTest>(vm, mapping, viewManager, wrapperManager, navigationDispatcher, threadDispatcher)
            {
                InitializeViewHandler = context => ++initCount
            };
            mediator.ShowViewHandler = context =>
            {
                mediator.OnViewShown(DefaultMetadata);
                return null;
            };
            navigationDispatcher.AddComponent(new NavigationContextProvider());
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (viewMapping, r, m, token) => new ValueTask<IView?>(view)
            });
            threadDispatcher.AddComponent(new TestThreadDispatcherComponent());
            wrapperManager.AddComponent(new DelegateWrapperManager<Type, object>((type, type1, arg4) => true, (type, request, __) =>
            {
                type.ShouldEqual(typeof(ViewModelPresenterMediatorBaseTest));
                request.ShouldEqual(view);
                return this;
            }));

            mediator.TryShow(includeView ? view.Target : null, cts.Token, DefaultMetadata);
            initCount.ShouldEqual(1);
            mediator.View.ShouldEqual(view);
            mediator.CurrentView.ShouldEqual(this);
        }

        [Fact]
        public void TryShowShouldDetectRestore()
        {
            var vm = new TestViewModel();
            var mapping = new ViewMapping("id", typeof(object), vm.GetType(), DefaultMetadata);
            var view = new View(mapping, new object(), vm);
            var cts = new CancellationTokenSource();
            var showCount = 0;

            var navigationDispatcher = new NavigationDispatcher();
            var viewManager = new ViewManager();
            var threadDispatcher = new ThreadDispatcher();
            var mediator = new TestViewModelPresenterMediatorBase<object>(vm, mapping, viewManager, null, navigationDispatcher, threadDispatcher);
            mediator.ShowViewHandler = context =>
            {
                ++showCount;
                context.NavigationMode.ShouldEqual(NavigationMode.New);
                mediator.OnViewShown(DefaultMetadata);
                return null;
            };

            navigationDispatcher.AddComponent(new NavigationContextProvider());
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (viewMapping, r, m, token) => new ValueTask<IView?>(view)
            });
            threadDispatcher.AddComponent(new TestThreadDispatcherComponent());
            mediator.TryShow(null, cts.Token, DefaultMetadata);
            showCount.ShouldEqual(1);

            showCount = 0;
            mediator = new TestViewModelPresenterMediatorBase<object>(vm, mapping, viewManager, null, navigationDispatcher, threadDispatcher);
            mediator.ShowViewHandler = context =>
            {
                ++showCount;
                context.NavigationMode.ShouldEqual(NavigationMode.Restore);
                mediator.OnViewShown(DefaultMetadata);
                return null;
            };
            mediator.TryShow(null, cts.Token, DefaultMetadata);
            showCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TryShowShouldActivateView(bool result)
        {
            var vm = new TestViewModel();
            var mapping = new ViewMapping("id", typeof(object), vm.GetType(), DefaultMetadata);
            var view = new View(mapping, new object(), vm);
            var cts = new CancellationTokenSource();
            var showCount = 0;
            var activateCount = 0;
            var navigateCount = 0;
            var cancelCount = 0;

            var navigationDispatcher = new NavigationDispatcher();
            var viewManager = new ViewManager();
            var threadDispatcher = new ThreadDispatcher();
            var mediator = new TestViewModelPresenterMediatorBase<object>(vm, mapping, viewManager, null, navigationDispatcher, threadDispatcher);
            mediator.ShowViewHandler = context =>
            {
                ++showCount;
                context.NavigationMode.ShouldEqual(NavigationMode.New);
                mediator.OnViewShown(DefaultMetadata);
                return null;
            };
            mediator.ActivateViewHandler = context =>
            {
                ++activateCount;
                context.NavigationMode.ShouldEqual(NavigationMode.Refresh);
                if (result)
                    mediator.OnViewActivated(DefaultMetadata);
                return Task.FromResult(result);
            };
            mediator.OnNavigatedHandler = context =>
            {
                ++navigateCount;
                return true;
            };
            mediator.OnNavigationCanceledHandler = (context, token) =>
            {
                ++cancelCount;
                return true;
            };

            navigationDispatcher.AddComponent(new NavigationContextProvider());
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (viewMapping, r, m, token) => new ValueTask<IView?>(view)
            });
            threadDispatcher.AddComponent(new TestThreadDispatcherComponent());
            mediator.TryShow(null, cts.Token, DefaultMetadata);
            showCount.ShouldEqual(1);
            navigateCount.ShouldEqual(1);
            cancelCount.ShouldEqual(0);
            activateCount.ShouldEqual(0);

            mediator.TryShow(null, cts.Token, DefaultMetadata);
            showCount.ShouldEqual(1);
            activateCount.ShouldEqual(1);
            if (result)
            {
                navigateCount.ShouldEqual(2);
                cancelCount.ShouldEqual(0);
            }
            else
            {
                navigateCount.ShouldEqual(1);
                cancelCount.ShouldEqual(1);
            }
        }

        [Fact]
        public void TryShowShouldUpdateView()
        {
            var vm = new TestViewModel();
            var mapping = new ViewMapping("id", typeof(object), vm.GetType(), DefaultMetadata);
            var view = new View(mapping, new object(), vm);
            var newView = new View(mapping, new object(), vm);
            var cts = new CancellationTokenSource();
            var cleanupCount = 0;
            var showCount = 0;
            var activateCount = 0;

            var navigationDispatcher = new NavigationDispatcher();
            var viewManager = new ViewManager();
            var threadDispatcher = new ThreadDispatcher();
            var mediator = new TestViewModelPresenterMediatorBase<object>(vm, mapping, viewManager, null, navigationDispatcher, threadDispatcher)
            {
                CleanupViewHandler = context => { ++cleanupCount; }
            };
            mediator.ShowViewHandler = context =>
            {
                ++showCount;
                mediator.OnViewShown(DefaultMetadata);
                return null;
            };
            mediator.ActivateViewHandler = context =>
            {
                ++activateCount;
                mediator.OnViewActivated(DefaultMetadata);
                return Default.TrueTask;
            };

            var cleanupViewCount = 0;
            navigationDispatcher.AddComponent(new NavigationContextProvider());
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (viewMapping, r, m, token) =>
                {
                    MugenExtensions.TryGetViewModelView(r, out object? v);
                    if (v == null)
                        return new ValueTask<IView?>(view);
                    return new ValueTask<IView?>(newView);
                },
                TryCleanupAsync = (v, r, m, t) =>
                {
                    ++cleanupViewCount;
                    v.ShouldEqual(view);
                    return Default.TrueTask;
                }
            });
            threadDispatcher.AddComponent(new TestThreadDispatcherComponent());
            mediator.TryShow(null, cts.Token, DefaultMetadata);
            mediator.View.ShouldEqual(view);
            mediator.CurrentView.ShouldEqual(view.Target);
            cleanupCount.ShouldEqual(0);
            activateCount.ShouldEqual(0);
            showCount.ShouldEqual(1);
            cleanupViewCount.ShouldEqual(0);

            mediator.TryShow(newView.Target, cts.Token, default);
            mediator.View.ShouldEqual(newView);
            mediator.CurrentView.ShouldEqual(newView.Target);
            cleanupCount.ShouldEqual(1);
            showCount.ShouldEqual(1);
            activateCount.ShouldEqual(1);
            cleanupViewCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TryShowShouldWaitNavigationDispatcherOnNavigating(bool result)
        {
            var vm = new TestViewModel();
            var mapping = new ViewMapping("id", typeof(object), vm.GetType(), DefaultMetadata);
            var view = new View(mapping, new object(), vm);
            var cts = new CancellationTokenSource().Token;
            var navigationContext = new NavigationContext(this, Default.NavigationProvider, "t", NavigationType.Popup, NavigationMode.New);
            var showCount = 0;
            var cancelCount = 0;
            var tcs = new TaskCompletionSource<bool>();

            var navigationDispatcher = new NavigationDispatcher();
            navigationDispatcher.AddComponent(new TestNavigationContextProviderComponent
            {
                TryGetNavigationContext = (o, provider, arg3, arg4, arg5, arg6) => navigationContext
            });
            navigationDispatcher.AddComponent(new TestConditionNavigationDispatcherComponent
            {
                CanNavigateAsync = (context, t) =>
                {
                    context.ShouldEqual(navigationContext);
                    t.CanBeCanceled.ShouldBeTrue();
                    return tcs.Task;
                }
            });
            navigationDispatcher.AddComponent(new TestNavigationDispatcherErrorListener
            {
                OnNavigationCanceled = (context, t) =>
                {
                    ++cancelCount;
                    context.ShouldEqual(navigationContext);
                    t.CanBeCanceled.ShouldBeTrue();
                }
            });

            var viewManager = new ViewManager();
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (viewMapping, r, token, m) => new ValueTask<IView?>(view)
            });

            var threadDispatcher = new ThreadDispatcher();
            threadDispatcher.AddComponent(new TestThreadDispatcherComponent());

            var mediator = new TestViewModelPresenterMediatorBase<object>(vm, mapping, viewManager, null, navigationDispatcher, threadDispatcher)
            {
                ShowViewHandler = context =>
                {
                    ++showCount;
                    return null;
                }
            };
            mediator.TryShow(null, cts, DefaultMetadata);
            tcs.SetResult(result);
            WaitCompletion();
            if (result)
                showCount.ShouldEqual(1);
            else
            {
                showCount.ShouldEqual(0);
                cancelCount.ShouldEqual(1);
            }
        }

        [Fact]
        public void TryCloseShouldClearView()
        {
            var vm = new TestViewModel();
            var mapping = new ViewMapping("id", typeof(object), vm.GetType(), DefaultMetadata);
            var view = new View(mapping, new object(), vm);
            var cts = new CancellationTokenSource();
            var initCount = 0;
            var clearCount = 0;
            var clearMediatorCount = 0;

            var navigationDispatcher = new NavigationDispatcher();
            var viewManager = new ViewManager();
            var threadDispatcher = new ThreadDispatcher();
            var mediator = new TestViewModelPresenterMediatorBase<object>(vm, mapping, viewManager, null, navigationDispatcher, threadDispatcher);
            mediator.InitializeViewHandler = context => { ++initCount; };
            mediator.CleanupViewHandler = context =>
            {
                ++clearMediatorCount;
                mediator.CurrentView.ShouldEqual(view.Target);
            };
            mediator.CloseViewHandler = context =>
            {
                mediator.OnViewClosed(DefaultMetadata);
                return null;
            };
            mediator.ShowViewHandler = context =>
            {
                mediator.OnViewShown(DefaultMetadata);
                return null;
            };
            navigationDispatcher.AddComponent(new NavigationContextProvider());
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (viewMapping, r, m, token) => new ValueTask<IView?>(view),
                TryCleanupAsync = (v, o, arg3, arg5) =>
                {
                    ++clearCount;
                    v.ShouldEqual(view);
                    return Default.TrueTask;
                }
            });
            threadDispatcher.AddComponent(new TestThreadDispatcherComponent());
            mediator.TryShow(null, cts.Token, DefaultMetadata);
            initCount.ShouldEqual(1);
            clearMediatorCount.ShouldEqual(0);
            clearCount.ShouldEqual(0);

            mediator.TryClose(null, default, DefaultMetadata);
            initCount.ShouldEqual(1);
            clearMediatorCount.ShouldEqual(1);
            clearCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TryCloseShouldWaitNavigationDispatcherOnNavigating(bool result)
        {
            var vm = new TestViewModel();
            var mapping = new ViewMapping("id", typeof(object), vm.GetType(), DefaultMetadata);
            var view = new View(mapping, new object(), vm);
            var cts = new CancellationTokenSource().Token;
            var navigationContext = new NavigationContext(this, Default.NavigationProvider, "t", NavigationType.Popup, NavigationMode.New);
            var closeCount = 0;
            var cancelCount = 0;
            var canClose = true;
            var tcs = new TaskCompletionSource<bool>();

            var navigationDispatcher = new NavigationDispatcher();
            navigationDispatcher.AddComponent(new TestNavigationContextProviderComponent
            {
                TryGetNavigationContext = (o, provider, arg3, arg4, arg5, arg6) => navigationContext
            });
            navigationDispatcher.AddComponent(new TestConditionNavigationDispatcherComponent
            {
                CanNavigateAsync = (context, t) =>
                {
                    context.ShouldEqual(navigationContext);
                    t.CanBeCanceled.ShouldBeTrue();
                    if (canClose)
                        return Task.FromResult(true);
                    return tcs.Task;
                }
            });
            navigationDispatcher.AddComponent(new TestNavigationDispatcherErrorListener
            {
                OnNavigationCanceled = (context, t) =>
                {
                    ++cancelCount;
                    context.ShouldEqual(navigationContext);
                    t.ShouldEqual(cts);
                }
            });

            var viewManager = new ViewManager();
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (viewMapping, r, token, m) => new ValueTask<IView?>(view)
            });

            var threadDispatcher = new ThreadDispatcher();
            threadDispatcher.AddComponent(new TestThreadDispatcherComponent());

            var mediator = new TestViewModelPresenterMediatorBase<object>(vm, mapping, viewManager, null, navigationDispatcher, threadDispatcher);
            mediator.ShowViewHandler = context =>
            {
                mediator.OnViewShown(DefaultMetadata);
                return null;
            };
            mediator.CloseViewHandler = context =>
            {
                ++closeCount;
                mediator.OnViewClosed(DefaultMetadata);
                return null;
            };
            mediator.TryShow(null, cts, DefaultMetadata);

            canClose = false;
            mediator.TryClose(null, cts, DefaultMetadata);
            tcs.SetResult(result);
            WaitCompletion();
            if (result)
                closeCount.ShouldEqual(1);
            else
            {
                closeCount.ShouldEqual(0);
                cancelCount.ShouldEqual(1);
            }
        }

        [Fact]
        public void OnClosingShouldWaitNavigationDispatcherOnNavigating()
        {
            var vm = new TestViewModel();
            var mapping = new ViewMapping("id", typeof(object), vm.GetType(), DefaultMetadata);
            var view = new View(mapping, new object(), vm);
            var navigationContext = new NavigationContext(this, Default.NavigationProvider, "t", NavigationType.Popup, NavigationMode.New);
            var canClose = true;
            var tcs = new TaskCompletionSource<bool>();

            var navigationDispatcher = new NavigationDispatcher();
            navigationDispatcher.AddComponent(new TestNavigationContextProviderComponent
            {
                TryGetNavigationContext = (o, provider, arg3, arg4, arg5, arg6) => navigationContext
            });
            navigationDispatcher.AddComponent(new TestConditionNavigationDispatcherComponent
            {
                CanNavigateAsync = (context, t) =>
                {
                    context.ShouldEqual(navigationContext);
                    if (canClose)
                        return Task.FromResult(true);
                    return tcs.Task;
                }
            });

            var viewManager = new ViewManager();
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (viewMapping, r, token, m) => new ValueTask<IView?>(view)
            });

            var threadDispatcher = new ThreadDispatcher();
            threadDispatcher.AddComponent(new TestThreadDispatcherComponent());

            var mediator = new TestViewModelPresenterMediatorBase<object>(vm, mapping, viewManager, null, navigationDispatcher, threadDispatcher);
            mediator.ShowViewHandler = context =>
            {
                mediator.OnViewShown(DefaultMetadata);
                return null;
            };
            mediator.CloseViewHandler = context =>
            {
                mediator.OnViewClosed(DefaultMetadata);
                return null;
            };
            mediator.TryShow(null, default, DefaultMetadata);

            canClose = false;
            var cancelEventArgs = new CancelableRequest();
            mediator.OnViewClosing(cancelEventArgs, DefaultMetadata);
            cancelEventArgs.Cancel!.Value.ShouldBeTrue();
            tcs.SetResult(true);
            cancelEventArgs.Cancel = null;
            mediator.OnViewClosing(cancelEventArgs, DefaultMetadata);
            cancelEventArgs.Cancel!.Value.ShouldBeFalse();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void TryCloseShouldNotifyNavigationDispatcher(int state)
        {
            var vm = new TestViewModel();
            var mapping = new ViewMapping("id", typeof(object), vm.GetType(), DefaultMetadata);
            var view = new View(mapping, new object(), vm);
            var navigationContext = new NavigationContext(this, Default.NavigationProvider, "t", NavigationType.Popup, NavigationMode.New);
            var exception = new Exception();
            var cts = new CancellationTokenSource();
            string? navigationId = null;
            var navigatedCount = 0;
            var navigatingCount = 0;
            var navigatingConditionCount = 0;
            var errorCount = 0;
            var cancelCount = 0;
            var contextCount = 0;
            var ignore = true;
            var metadata = DefaultMetadata;

            var navigationDispatcher = new NavigationDispatcher();
            var viewManager = new ViewManager();
            var threadDispatcher = new ThreadDispatcher();
            var mediator = new TestViewModelPresenterMediatorBase<object>(vm, mapping, viewManager, null, navigationDispatcher, threadDispatcher);
            mediator.ShowViewHandler = context =>
            {
                mediator.OnViewShown(DefaultMetadata);
                return null;
            };
            mediator.CloseViewHandler = context =>
            {
                if (state == 3)
                    throw exception;
                mediator.OnViewClosed(DefaultMetadata);
                return null;
            };
            navigationDispatcher.AddComponent(new TestNavigationContextProviderComponent
            {
                TryGetNavigationContext = (o, provider, nId, type, mode, m) =>
                {
                    ++contextCount;
                    o.ShouldEqual(vm);
                    provider.ShouldEqual(mediator);
                    navigationId = nId;
                    type.ShouldEqual(mediator.NavigationType);
                    if (!ignore)
                        mode.ShouldEqual(NavigationMode.Close);
                    m.ShouldEqual(metadata);
                    return navigationContext;
                }
            });
            navigationDispatcher.AddComponent(new TestNavigationDispatcherErrorListener
            {
                OnNavigationCanceled = (context, arg3) =>
                {
                    ++cancelCount;
                    context.ShouldEqual(navigationContext);
                    arg3.ShouldEqual(cts.Token);
                },
                OnNavigationFailed = (context, arg3) =>
                {
                    ++errorCount;
                    context.ShouldEqual(navigationContext);
                    arg3.ShouldEqual(exception);
                }
            });
            navigationDispatcher.AddComponent(new TestConditionNavigationDispatcherComponent
            {
                CanNavigateAsync = (context, t) =>
                {
                    ++navigatingConditionCount;
                    context.ShouldEqual(navigationContext);
                    return null;
                }
            });
            navigationDispatcher.AddComponent(new TestNavigationDispatcherNavigatingListener
            {
                OnNavigating = context =>
                {
                    ++navigatingCount;
                    context.ShouldEqual(navigationContext);
                }
            });
            navigationDispatcher.AddComponent(new TestNavigationDispatcherNavigatedListener
            {
                OnNavigated = context =>
                {
                    ++navigatedCount;
                    context.ShouldEqual(navigationContext);
                }
            });
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (viewMapping, r, token, m) => new ValueTask<IView?>(view)
            });
            threadDispatcher.AddComponent(new TestThreadDispatcherComponent());
            mediator.TryShow(null, cts.Token, metadata);
            contextCount = 0;
            navigatedCount = 0;
            navigatingCount = 0;
            navigatingConditionCount = 0;
            cancelCount = 0;
            errorCount = 0;
            ignore = false;
            if (state == 2)
                cts.Cancel();
            var presenterResult = mediator.TryClose(null, cts.Token, metadata)!;
            contextCount.ShouldEqual(1);
            presenterResult.NavigationProvider.ShouldEqual(mediator);
            presenterResult.NavigationType.ShouldEqual(mediator.NavigationType);
            presenterResult.Target.ShouldEqual(vm);
            presenterResult.NavigationId.ShouldEqual(navigationId);
            navigatingCount.ShouldEqual(state == 2 ? 0 : 1);
            navigatingConditionCount.ShouldEqual(state == 2 ? 0 : 1);

            if (state == 1)
            {
                navigatedCount.ShouldEqual(1);
                cancelCount.ShouldEqual(0);
                errorCount.ShouldEqual(0);
            }
            else if (state == 2)
            {
                navigatedCount.ShouldEqual(0);
                cancelCount.ShouldEqual(1);
                errorCount.ShouldEqual(0);
            }
            else
            {
                navigatedCount.ShouldEqual(0);
                cancelCount.ShouldEqual(0);
                errorCount.ShouldEqual(1);
            }
        }

        [Fact]
        public void TryCloseShouldCheckView()
        {
            var vm = new TestViewModel();
            var mapping = new ViewMapping("id", typeof(object), vm.GetType(), DefaultMetadata);
            var view = new View(mapping, new object(), vm);
            var cts = new CancellationTokenSource().Token;
            var closeCount = 0;

            var navigationDispatcher = new NavigationDispatcher();
            navigationDispatcher.AddComponent(new NavigationContextProvider());
            var viewManager = new ViewManager();
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (viewMapping, r, token, m) => new ValueTask<IView?>(view)
            });
            var threadDispatcher = new ThreadDispatcher();
            threadDispatcher.AddComponent(new TestThreadDispatcherComponent());
            var mediator = new TestViewModelPresenterMediatorBase<object>(vm, mapping, viewManager, null, navigationDispatcher, threadDispatcher)
            {
                CloseViewHandler = context =>
                {
                    ++closeCount;
                    return null;
                }
            };
            mediator.ShowViewHandler = context =>
            {
                mediator.OnViewShown(DefaultMetadata);
                return null;
            };
            mediator.TryShow(null, cts, DefaultMetadata);

            mediator.TryClose(this, cts, DefaultMetadata);
            closeCount.ShouldEqual(0);

            mediator.TryClose(view.Target, cts, DefaultMetadata);
            closeCount.ShouldEqual(1);
        }


        [Fact]
        public void TryShowShouldCheckNavigationType()
        {
            var vm = new TestViewModel();
            var mapping = new ViewMapping("id", typeof(object), vm.GetType(), DefaultMetadata);
            var view = new View(mapping, new object(), vm);
            var cts = new CancellationTokenSource().Token;
            var showCount = 0;

            var navigationDispatcher = new NavigationDispatcher();
            navigationDispatcher.AddComponent(new NavigationContextProvider());
            var viewManager = new ViewManager();
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (viewMapping, r, token, m) => new ValueTask<IView?>(view)
            });
            var threadDispatcher = new ThreadDispatcher();
            threadDispatcher.AddComponent(new TestThreadDispatcherComponent());
            var mediator = new TestViewModelPresenterMediatorBase<object>(vm, mapping, viewManager, null, navigationDispatcher, threadDispatcher)
            {
                ShowViewHandler = context =>
                {
                    ++showCount;
                    return null;
                },
                NavigationTypeField = NavigationType.Page
            };
            mediator.TryShow(null, cts, NavigationMetadata.NavigationType.ToContext(NavigationType.Background));
            showCount.ShouldEqual(0);

            mediator.TryShow(null, cts, NavigationMetadata.NavigationType.ToContext(mediator.NavigationTypeField));
            showCount.ShouldEqual(1);
        }

        [Fact]
        public void TryCloseShouldCheckNavigationType()
        {
            var vm = new TestViewModel();
            var mapping = new ViewMapping("id", typeof(object), vm.GetType(), DefaultMetadata);
            var view = new View(mapping, new object(), vm);
            var cts = new CancellationTokenSource().Token;
            var closeCount = 0;

            var navigationDispatcher = new NavigationDispatcher();
            navigationDispatcher.AddComponent(new NavigationContextProvider());
            var viewManager = new ViewManager();
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (viewMapping, r, token, m) => new ValueTask<IView?>(view)
            });
            var threadDispatcher = new ThreadDispatcher();
            threadDispatcher.AddComponent(new TestThreadDispatcherComponent());
            var mediator = new TestViewModelPresenterMediatorBase<object>(vm, mapping, viewManager, null, navigationDispatcher, threadDispatcher)
            {
                CloseViewHandler = context =>
                {
                    ++closeCount;
                    return null;
                }
            };
            mediator.NavigationTypeField = NavigationType.Page;
            mediator.ShowViewHandler = context =>
            {
                mediator.OnViewShown(DefaultMetadata);
                return null;
            };
            mediator.TryShow(null, cts, DefaultMetadata);

            mediator.TryClose(null, cts, NavigationMetadata.NavigationType.ToContext(NavigationType.Background));
            closeCount.ShouldEqual(0);

            mediator.TryClose(null, cts, NavigationMetadata.NavigationType.ToContext(mediator.NavigationTypeField));
            closeCount.ShouldEqual(1);
        }

        #endregion
    }
}