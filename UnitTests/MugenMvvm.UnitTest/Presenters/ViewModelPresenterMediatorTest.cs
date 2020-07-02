using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;
using MugenMvvm.Navigation;
using MugenMvvm.Navigation.Components;
using MugenMvvm.Requests;
using MugenMvvm.Threading;
using MugenMvvm.UnitTest.Navigation.Internal;
using MugenMvvm.UnitTest.Presenters.Internal;
using MugenMvvm.UnitTest.Threading.Internal;
using MugenMvvm.UnitTest.ViewModels.Internal;
using MugenMvvm.UnitTest.Views.Internal;
using MugenMvvm.Views;
using MugenMvvm.Wrapping;
using MugenMvvm.Wrapping.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Presenters
{
    public class ViewModelPresenterMediatorTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void InitializeShouldInitializeFields()
        {
            var invokeCount = 0;
            var mediator = new TestViewModelPresenterMediatorBase<object>
            {
                OnInitializedHandler = context =>
                {
                    ++invokeCount;
                    context.ShouldEqual(DefaultMetadata);
                }
            };
            ShouldThrow<InvalidOperationException>(() =>
            {
                var _ = mediator.Id;
            });
            ShouldThrow<InvalidOperationException>(() =>
            {
                var _ = mediator.ViewModel;
            });
            ShouldThrow<InvalidOperationException>(() =>
            {
                var _ = mediator.Mapping;
            });

            var vm = new TestViewModel();
            var mapping = new ViewMapping("id", typeof(object), vm.GetType(), DefaultMetadata);
            mediator.Initialize(vm, mapping, DefaultMetadata);
            invokeCount.ShouldEqual(1);
            mediator.Id.ShouldEqual($"{mediator.GetType().FullName}{mapping.Id}");
            mediator.ViewModel.ShouldEqual(vm);
            mediator.Mapping.ShouldEqual(mapping);
            mediator.View.ShouldBeNull();
            mediator.CurrentView.ShouldBeNull();
        }

        [Fact]
        public void InitializeTwiceShouldThrow()
        {
            var invokeCount = 0;
            var mediator = new TestViewModelPresenterMediatorBase<object>
            {
                OnInitializedHandler = context =>
                {
                    ++invokeCount;
                    context.ShouldEqual(DefaultMetadata);
                }
            };
            var vm = new TestViewModel();
            var mapping = new ViewMapping("id", typeof(object), vm.GetType(), DefaultMetadata);
            mediator.Initialize(vm, mapping, DefaultMetadata);
            mediator.Initialize(vm, mapping, DefaultMetadata);
            invokeCount.ShouldEqual(1);

            ShouldThrow<InvalidOperationException>(() => mediator.Initialize(vm, new ViewMapping("id1", typeof(object), vm.GetType(), DefaultMetadata), DefaultMetadata));
            ShouldThrow<InvalidOperationException>(() => mediator.Initialize(new TestViewModel(), mapping, DefaultMetadata));
            invokeCount.ShouldEqual(1);
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
            int navigatedCount = 0;
            int errorCount = 0;
            int cancelCount = 0;
            int contextCount = 0;

            var navigationDispatcher = new NavigationDispatcher();
            var viewManager = new ViewManager();
            var threadDispatcher = new ThreadDispatcher();
            var mediator = new TestViewModelPresenterMediatorBase<object>(viewManager, null, navigationDispatcher, threadDispatcher);
            mediator.ShowViewHandler = context => mediator.OnViewShown();
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
                OnNavigationCanceled = (dispatcher, context, arg3) =>
                {
                    ++cancelCount;
                    dispatcher.ShouldEqual(navigationDispatcher);
                    context.ShouldEqual(navigationContext);
                    arg3.ShouldEqual(cts.Token);
                },
                OnNavigationFailed = (dispatcher, context, arg3) =>
                {
                    ++errorCount;
                    dispatcher.ShouldEqual(navigationDispatcher);
                    context.ShouldEqual(navigationContext);
                    arg3.ShouldEqual(exception);
                }
            });
            navigationDispatcher.AddComponent(new TestNavigationDispatcherNavigatedListener
            {
                OnNavigated = (dispatcher, context) =>
                {
                    ++navigatedCount;
                    dispatcher.ShouldEqual(navigationDispatcher);
                    context.ShouldEqual(navigationContext);
                }
            });
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (viewMapping, r, t, token, m) =>
                {
                    if (state == 1)
                        return Task.FromResult<IView>(view);
                    if (state == 2)
                    {
                        cts.Cancel();
                        return Task.FromCanceled<IView>(cts.Token);
                    }
                    return Task.FromException<IView>(exception);
                }
            });
            threadDispatcher.AddComponent(new TestThreadDispatcherComponent());
            mediator.Initialize(vm, mapping, DefaultMetadata);
            var presenterResult = mediator.TryShow(null, cts.Token, DefaultMetadata)!;
            contextCount.ShouldEqual(1);
            presenterResult.NavigationProvider.ShouldEqual(mediator);
            presenterResult.NavigationType.ShouldEqual(mediator.NavigationType);
            presenterResult.Target.ShouldEqual(vm);
            presenterResult.NavigationId.ShouldEqual(navigationId);

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
            var mediator = new TestViewModelPresenterMediatorBase<object>(viewManager, null, navigationDispatcher, threadDispatcher);
            mediator.InitializeViewHandler = context =>
            {
                ++initCount;
            };
            mediator.ShowViewHandler = context => mediator.OnViewShown();
            navigationDispatcher.AddComponent(new NavigationContextProvider());
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (viewMapping, r, t, m, token) =>
                {
                    viewMapping.ShouldEqual(mapping);
                    r.ShouldEqual(new ViewModelViewRequest(vm, includeView ? view.Target : null));
                    t.ShouldEqual(typeof(ViewModelViewRequest));
                    token.ShouldEqual(cts.Token);
                    return Task.FromResult<IView>(view);
                }
            });
            threadDispatcher.AddComponent(new TestThreadDispatcherComponent());
            mediator.Initialize(vm, mapping, DefaultMetadata);
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
            var mediator = new TestViewModelPresenterMediatorBase<ViewModelPresenterMediatorTest>(viewManager, wrapperManager, navigationDispatcher, threadDispatcher)
            {
                InitializeViewHandler = context => ++initCount
            };
            mediator.ShowViewHandler = context => mediator.OnViewShown();
            navigationDispatcher.AddComponent(new NavigationContextProvider());
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (viewMapping, r, t, m, token) => Task.FromResult<IView>(view)
            });
            threadDispatcher.AddComponent(new TestThreadDispatcherComponent());
            wrapperManager.AddComponent(new DelegateWrapperManager<Type, object, object?>((type, type1, arg3, arg4) => true, (type, request, _, __) =>
            {
                type.ShouldEqual(typeof(ViewModelPresenterMediatorTest));
                request.ShouldEqual(view.Target);
                return this;
            }, null));

            mediator.Initialize(vm, mapping, DefaultMetadata);
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
            int showCount = 0;

            var navigationDispatcher = new NavigationDispatcher();
            var viewManager = new ViewManager();
            var threadDispatcher = new ThreadDispatcher();
            var mediator = new TestViewModelPresenterMediatorBase<object>(viewManager, null, navigationDispatcher, threadDispatcher);
            mediator.ShowViewHandler = context =>
            {
                ++showCount;
                context.NavigationMode.ShouldEqual(NavigationMode.New);
                mediator.OnViewShown();
            };

            navigationDispatcher.AddComponent(new NavigationContextProvider());
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (viewMapping, r, t, m, token) => Task.FromResult<IView>(view)
            });
            threadDispatcher.AddComponent(new TestThreadDispatcherComponent());
            mediator.Initialize(vm, mapping, DefaultMetadata);
            mediator.TryShow(null, cts.Token, DefaultMetadata);
            showCount.ShouldEqual(1);

            showCount = 0;
            mediator = new TestViewModelPresenterMediatorBase<object>(viewManager, null, navigationDispatcher, threadDispatcher);
            mediator.ShowViewHandler = context =>
            {
                ++showCount;
                context.NavigationMode.ShouldEqual(NavigationMode.Restore);
                mediator.OnViewShown();
            };
            mediator.Initialize(vm, mapping, DefaultMetadata);
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
            int showCount = 0;
            int activateCount = 0;
            int navigateCount = 0;
            int cancelCount = 0;

            var navigationDispatcher = new NavigationDispatcher();
            var viewManager = new ViewManager();
            var threadDispatcher = new ThreadDispatcher();
            var mediator = new TestViewModelPresenterMediatorBase<object>(viewManager, null, navigationDispatcher, threadDispatcher);
            mediator.ShowViewHandler = context =>
            {
                ++showCount;
                context.NavigationMode.ShouldEqual(NavigationMode.New);
                mediator.OnViewShown();
            };
            mediator.ActivateViewHandler = context =>
            {
                ++activateCount;
                context.NavigationMode.ShouldEqual(NavigationMode.Refresh);
                if (result)
                    mediator.OnViewActivated();
                return result;
            };
            mediator.OnNavigatedHandler = (context) =>
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
                TryInitializeAsync = (viewMapping, r, t, m, token) => Task.FromResult<IView>(view)
            });
            threadDispatcher.AddComponent(new TestThreadDispatcherComponent());
            mediator.Initialize(vm, mapping, DefaultMetadata);
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
            int cleanupCount = 0;
            int showCount = 0;
            int activateCount = 0;

            var navigationDispatcher = new NavigationDispatcher();
            var viewManager = new ViewManager();
            var threadDispatcher = new ThreadDispatcher();
            var mediator = new TestViewModelPresenterMediatorBase<object>(viewManager, null, navigationDispatcher, threadDispatcher);
            mediator.CleanupViewHandler = context =>
            {
                ++cleanupCount;
            };
            mediator.ShowViewHandler = context =>
            {
                ++showCount;
                mediator.OnViewShown();
            };
            mediator.ActivateViewHandler = context =>
            {
                ++activateCount;
                mediator.OnViewActivated();
                return true;
            };

            navigationDispatcher.AddComponent(new NavigationContextProvider());
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (viewMapping, r, t, m, token) =>
                {
                    var viewModelViewRequest = (ViewModelViewRequest)r;
                    if (viewModelViewRequest.View == null)
                        return Task.FromResult<IView>(view);
                    return Task.FromResult<IView>(newView);
                },
                TryCleanupAsync = (v, r, rt, m, t) =>
                {
                    v.ShouldEqual(view);
                    t.ShouldNotEqual(default);
                    return Task.CompletedTask;
                }
            });
            threadDispatcher.AddComponent(new TestThreadDispatcherComponent());
            mediator.Initialize(vm, mapping, DefaultMetadata);
            mediator.TryShow(null, cts.Token, DefaultMetadata);
            mediator.View.ShouldEqual(view);
            mediator.CurrentView.ShouldEqual(view.Target);
            cleanupCount.ShouldEqual(0);
            activateCount.ShouldEqual(0);
            showCount.ShouldEqual(1);

            mediator.TryShow(newView.Target, cts.Token, default);
            mediator.View.ShouldEqual(newView);
            mediator.CurrentView.ShouldEqual(newView.Target);
            cleanupCount.ShouldEqual(1);
            showCount.ShouldEqual(1);
            activateCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TryShowShouldWaitNavigation(bool defaultImpl)
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
                TryInitializeAsync = (viewMapping, r, t, token, m) => Task.FromResult<IView>(view)
            });
            var threadDispatcher = new ThreadDispatcher();
            threadDispatcher.AddComponent(new TestThreadDispatcherComponent());
            var mediator = new TestViewModelPresenterMediatorBase<object>(viewManager, null, navigationDispatcher, threadDispatcher) { ShowViewHandler = context => { ++showCount; } };
            mediator.Initialize(vm, mapping, DefaultMetadata);

            Action invoke;
            if (defaultImpl)
            {
                var entries = new List<INavigationEntry>
                {
                    new NavigationEntry(this, mediator, "1", NavigationType.Popup)
                };
                var callbacks = new List<NavigationCallback>
                {
                    new NavigationCallback(NavigationCallbackType.Showing, "t", mediator.NavigationType),
                    new NavigationCallback(NavigationCallbackType.Closing, "t", mediator.NavigationType)
                };
                navigationDispatcher.AddComponent(new TestNavigationEntryProviderComponent
                {
                    TryGetNavigationEntries = context =>
                    {
                        context.ShouldEqual(DefaultMetadata);
                        return entries;
                    }
                });
                navigationDispatcher.AddComponent(new TestNavigationCallbackManagerComponent
                {
                    TryGetNavigationCallbacks = (o, type, arg3) => callbacks
                });
                invoke = () => callbacks[0].SetResult(new NavigationContext(this, Default.NavigationProvider, "t", mediator.NavigationType, NavigationMode.New));
            }
            else
            {
                var tcs = new TaskCompletionSource<object?>();
                mediator.WaitNavigationBeforeShowAsyncHandler = (token, context) =>
                {
                    token.ShouldEqual(cts);
                    context.ShouldEqual(DefaultMetadata);
                    return tcs.Task;
                };
                invoke = () => tcs.SetResult(null);
            }

            mediator.TryShow(null, cts, DefaultMetadata);
            showCount.ShouldEqual(0);
            invoke();
            showCount.ShouldEqual(1);
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
            navigationDispatcher.AddComponent(new TestNavigationDispatcherNavigatingListener
            {
                OnNavigatingAsync = (dispatcher, context, t) =>
                {
                    dispatcher.ShouldEqual(navigationDispatcher);
                    context.ShouldEqual(navigationContext);
                    t.ShouldEqual(cts);
                    return tcs.Task;
                }
            });
            navigationDispatcher.AddComponent(new TestNavigationDispatcherErrorListener
            {
                OnNavigationCanceled = (dispatcher, context, t) =>
                {
                    ++cancelCount;
                    dispatcher.ShouldEqual(navigationDispatcher);
                    context.ShouldEqual(navigationContext);
                    t.ShouldEqual(cts);
                }
            });

            var viewManager = new ViewManager();
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (viewMapping, r, t, token, m) => Task.FromResult<IView>(view)
            });

            var threadDispatcher = new ThreadDispatcher();
            threadDispatcher.AddComponent(new TestThreadDispatcherComponent());

            var mediator = new TestViewModelPresenterMediatorBase<object>(viewManager, null, navigationDispatcher, threadDispatcher) { ShowViewHandler = context => { ++showCount; } };
            mediator.Initialize(vm, mapping, DefaultMetadata);
            mediator.TryShow(null, cts, DefaultMetadata);
            tcs.SetResult(result);
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
            var mediator = new TestViewModelPresenterMediatorBase<object>(viewManager, null, navigationDispatcher, threadDispatcher);
            mediator.InitializeViewHandler = context =>
            {
                ++initCount;
            };
            mediator.CleanupViewHandler = context =>
            {
                ++clearMediatorCount;
                mediator.CurrentView.ShouldEqual(view.Target);
            };
            mediator.CloseViewHandler = context => mediator.OnViewClosed();
            mediator.ShowViewHandler = context => mediator.OnViewShown();
            navigationDispatcher.AddComponent(new NavigationContextProvider());
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (viewMapping, r, t, m, token) => Task.FromResult<IView>(view),
                TryCleanupAsync = (v, o, arg3, arg4, arg5) =>
                {
                    ++clearCount;
                    v.ShouldEqual(view);
                    return Task.CompletedTask;
                }
            });
            threadDispatcher.AddComponent(new TestThreadDispatcherComponent());
            mediator.Initialize(vm, mapping, DefaultMetadata);
            mediator.TryShow(null, cts.Token, DefaultMetadata);
            initCount.ShouldEqual(1);
            clearMediatorCount.ShouldEqual(0);
            clearCount.ShouldEqual(0);

            mediator.TryClose(default, DefaultMetadata);
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
            bool canClose = true;
            var tcs = new TaskCompletionSource<bool>();

            var navigationDispatcher = new NavigationDispatcher();
            navigationDispatcher.AddComponent(new TestNavigationContextProviderComponent
            {
                TryGetNavigationContext = (o, provider, arg3, arg4, arg5, arg6) => navigationContext
            });
            navigationDispatcher.AddComponent(new TestNavigationDispatcherNavigatingListener
            {
                OnNavigatingAsync = (dispatcher, context, t) =>
                {
                    dispatcher.ShouldEqual(navigationDispatcher);
                    context.ShouldEqual(navigationContext);
                    t.ShouldEqual(cts);
                    if (canClose)
                        return Task.FromResult<bool>(true);
                    return tcs.Task;
                }
            });
            navigationDispatcher.AddComponent(new TestNavigationDispatcherErrorListener
            {
                OnNavigationCanceled = (dispatcher, context, t) =>
                {
                    ++cancelCount;
                    dispatcher.ShouldEqual(navigationDispatcher);
                    context.ShouldEqual(navigationContext);
                    t.ShouldEqual(cts);
                }
            });

            var viewManager = new ViewManager();
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (viewMapping, r, t, token, m) => Task.FromResult<IView>(view)
            });

            var threadDispatcher = new ThreadDispatcher();
            threadDispatcher.AddComponent(new TestThreadDispatcherComponent());

            var mediator = new TestViewModelPresenterMediatorBase<object>(viewManager, null, navigationDispatcher, threadDispatcher);
            mediator.ShowViewHandler = context => mediator.OnViewShown();
            mediator.CloseViewHandler = context =>
            {
                ++closeCount;
                mediator.OnViewClosed();
            };
            mediator.Initialize(vm, mapping, DefaultMetadata);
            mediator.TryShow(null, cts, DefaultMetadata);

            canClose = false;
            mediator.TryClose(cts, DefaultMetadata);
            tcs.SetResult(result);
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
            bool canClose = true;
            var tcs = new TaskCompletionSource<bool>();

            var navigationDispatcher = new NavigationDispatcher();
            navigationDispatcher.AddComponent(new TestNavigationContextProviderComponent
            {
                TryGetNavigationContext = (o, provider, arg3, arg4, arg5, arg6) => navigationContext
            });
            navigationDispatcher.AddComponent(new TestNavigationDispatcherNavigatingListener
            {
                OnNavigatingAsync = (dispatcher, context, t) =>
                {
                    dispatcher.ShouldEqual(navigationDispatcher);
                    context.ShouldEqual(navigationContext);
                    if (canClose)
                        return Task.FromResult(true);
                    return tcs.Task;
                }
            });

            var viewManager = new ViewManager();
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (viewMapping, r, t, token, m) => Task.FromResult<IView>(view)
            });

            var threadDispatcher = new ThreadDispatcher();
            threadDispatcher.AddComponent(new TestThreadDispatcherComponent());

            var mediator = new TestViewModelPresenterMediatorBase<object>(viewManager, null, navigationDispatcher, threadDispatcher);
            mediator.ShowViewHandler = context => mediator.OnViewShown();
            mediator.CloseViewHandler = context =>
            {
                mediator.OnViewClosed();
            };
            mediator.Initialize(vm, mapping, DefaultMetadata);
            mediator.TryShow(null, default, DefaultMetadata);

            canClose = false;
            var cancelEventArgs = new CancelEventArgs();
            mediator.OnViewClosing(this, cancelEventArgs);
            cancelEventArgs.Cancel.ShouldBeTrue();
            tcs.SetResult(true);
            mediator.OnViewClosing(this, cancelEventArgs);
            cancelEventArgs.Cancel.ShouldBeFalse();
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
            int navigatedCount = 0;
            int errorCount = 0;
            int cancelCount = 0;
            int contextCount = 0;
            bool ignore = true;

            var navigationDispatcher = new NavigationDispatcher();
            var viewManager = new ViewManager();
            var threadDispatcher = new ThreadDispatcher();
            var mediator = new TestViewModelPresenterMediatorBase<object>(viewManager, null, navigationDispatcher, threadDispatcher);
            mediator.ShowViewHandler = context => mediator.OnViewShown();
            mediator.CloseViewHandler = context =>
            {
                if (state == 3)
                    throw exception;
                mediator.OnViewClosed();
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
                    m.ShouldEqual(DefaultMetadata);
                    return navigationContext;
                }
            });
            navigationDispatcher.AddComponent(new TestNavigationDispatcherErrorListener
            {
                OnNavigationCanceled = (dispatcher, context, arg3) =>
                {
                    ++cancelCount;
                    dispatcher.ShouldEqual(navigationDispatcher);
                    context.ShouldEqual(navigationContext);
                    arg3.ShouldEqual(cts.Token);
                },
                OnNavigationFailed = (dispatcher, context, arg3) =>
                {
                    ++errorCount;
                    dispatcher.ShouldEqual(navigationDispatcher);
                    context.ShouldEqual(navigationContext);
                    arg3.ShouldEqual(exception);
                }
            });
            navigationDispatcher.AddComponent(new TestNavigationDispatcherNavigatedListener
            {
                OnNavigated = (dispatcher, context) =>
                {
                    ++navigatedCount;
                    dispatcher.ShouldEqual(navigationDispatcher);
                    context.ShouldEqual(navigationContext);
                }
            });
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (viewMapping, r, t, token, m) => Task.FromResult<IView>(view)
            });
            threadDispatcher.AddComponent(new TestThreadDispatcherComponent());
            mediator.Initialize(vm, mapping, DefaultMetadata);
            mediator.TryShow(null, cts.Token, DefaultMetadata);
            contextCount = 0;
            navigatedCount = 0;
            cancelCount = 0;
            errorCount = 0;
            ignore = false;
            if (state == 2)
                cts.Cancel();
            var presenterResult = mediator.TryClose(cts.Token, DefaultMetadata)!;
            contextCount.ShouldEqual(1);
            presenterResult.NavigationProvider.ShouldEqual(mediator);
            presenterResult.NavigationType.ShouldEqual(mediator.NavigationType);
            presenterResult.Target.ShouldEqual(vm);
            presenterResult.NavigationId.ShouldEqual(navigationId);

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
        public void TryCloseShouldWaitNavigation(bool defaultImpl)
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
                TryInitializeAsync = (viewMapping, r, t, token, m) => Task.FromResult<IView>(view)
            });
            var threadDispatcher = new ThreadDispatcher();
            threadDispatcher.AddComponent(new TestThreadDispatcherComponent());
            var mediator = new TestViewModelPresenterMediatorBase<object>(viewManager, null, navigationDispatcher, threadDispatcher) { CloseViewHandler = context => ++closeCount };
            mediator.ShowViewHandler = context => mediator.OnViewShown();
            mediator.Initialize(vm, mapping, DefaultMetadata);

            Action? invoke = null;
            if (!defaultImpl)
            {
                var tcs = new TaskCompletionSource<object?>();
                mediator.WaitNavigationBeforeCloseAsyncHandler = (token, context) =>
                {
                    token.ShouldEqual(cts);
                    context.ShouldEqual(DefaultMetadata);
                    return tcs.Task;
                };
                invoke = () => tcs.SetResult(null);
            }

            mediator.TryShow(null, cts, DefaultMetadata);
            mediator.TryClose(cts, DefaultMetadata);
            if (invoke == null)
                closeCount.ShouldEqual(1);
            else
            {
                closeCount.ShouldEqual(0);
                invoke();
                closeCount.ShouldEqual(1);
            }
        }

        #endregion
    }
}