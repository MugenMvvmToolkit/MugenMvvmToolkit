using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;
using MugenMvvm.Navigation;
using MugenMvvm.Navigation.Components;
using MugenMvvm.Requests;
using MugenMvvm.Tests.Navigation;
using MugenMvvm.Tests.ViewModels;
using MugenMvvm.Tests.Views;
using MugenMvvm.UnitTests.Presentation.Internal;
using MugenMvvm.ViewModels;
using MugenMvvm.Views;
using MugenMvvm.Wrapping;
using MugenMvvm.Wrapping.Components;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Presentation
{
    public class ViewModelPresenterMediatorBaseTest : UnitTestBase
    {
        private readonly NavigationContext _navigationContext;
        private readonly TestViewModel _vm;
        private readonly ViewMapping _mapping;
        private readonly View _view;

        public ViewModelPresenterMediatorBaseTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _vm = new TestViewModel();
            _mapping = new ViewMapping("id", _vm.GetType(), typeof(object), Metadata);
            _view = new View(_mapping, new object(), _vm);
            _navigationContext = new NavigationContext(this, NavigationProvider.System, "t", NavigationType.Popup, NavigationMode.New);
            NavigationDispatcher.AddComponent(new NavigationContextProvider());
            ViewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (_, _, _, _, _) => new ValueTask<IView?>(_view)
            });
        }

        [Fact]
        public void ConstructorShouldInitializeFields()
        {
            var mediator = GetMediator<object>(_vm, _mapping);
            mediator.Id.ShouldEqual($"{mediator.GetType().FullName}{_mapping.Id}");
            mediator.ViewModel.ShouldEqual(_vm);
            mediator.Mapping.ShouldEqual(_mapping);
            mediator.View.ShouldBeNull();
            mediator.CurrentView.ShouldBeNull();
        }

        [Fact]
        public void OnClosingShouldWaitNavigationDispatcherOnNavigating()
        {
            var navigationContext = new NavigationContext(this, NavigationProvider.System, "t", NavigationType.Popup, NavigationMode.New);
            var canClose = true;
            var tcs = new TaskCompletionSource<bool>();

            NavigationDispatcher.AddComponent(new TestNavigationContextProviderComponent
            {
                Priority = int.MaxValue,
                TryGetNavigationContext = (_, o, provider, arg3, arg4, arg5, arg6) => navigationContext
            });
            NavigationDispatcher.AddComponent(new TestNavigationConditionComponent
            {
                CanNavigateAsync = (_, context, t) =>
                {
                    context.ShouldEqual(navigationContext);
                    if (canClose)
                        return new ValueTask<bool>(true);
                    return tcs.Task.AsValueTask();
                }
            });

            var mediator = GetMediator<object>(_vm, _mapping);
            mediator.ShowViewHandler = context =>
            {
                mediator.OnViewShown(Metadata);
                return null;
            };
            mediator.CloseViewHandler = context =>
            {
                mediator.OnViewClosed(Metadata);
                return null;
            };
            mediator.TryShow(null, default, Metadata);

            canClose = false;
            var cancelEventArgs = new CancelableRequest();
            mediator.OnViewClosing(cancelEventArgs, Metadata);
            cancelEventArgs.Cancel!.Value.ShouldBeTrue();
            tcs.SetResult(true);
            cancelEventArgs.Cancel = null;
            mediator.OnViewClosing(cancelEventArgs, Metadata);
            cancelEventArgs.Cancel!.Value.ShouldBeFalse();
        }

        [Fact]
        public void TryCloseShouldCheckNavigationType()
        {
            var closeCount = 0;
            var mediator = GetMediator<object>(_vm, _mapping);
            mediator.CloseViewHandler = context =>
            {
                ++closeCount;
                return null;
            };
            mediator.NavigationTypeField = NavigationType.Page;
            mediator.ShowViewHandler = context =>
            {
                mediator.OnViewShown(Metadata);
                return null;
            };

            mediator.TryShow(null, DefaultCancellationToken, Metadata);
            mediator.TryClose(null, DefaultCancellationToken, NavigationMetadata.NavigationType.ToContext(NavigationType.Background));
            closeCount.ShouldEqual(0);

            mediator.TryClose(null, DefaultCancellationToken, NavigationMetadata.NavigationType.ToContext(mediator.NavigationTypeField));
            closeCount.ShouldEqual(1);
        }

        [Fact]
        public void TryCloseShouldCheckView()
        {
            var closeCount = 0;
            var mediator = GetMediator<object>(_vm, _mapping);
            mediator.CloseViewHandler = context =>
            {
                ++closeCount;
                return null;
            };
            mediator.ShowViewHandler = context =>
            {
                mediator.OnViewShown(Metadata);
                return null;
            };
            mediator.TryShow(null, DefaultCancellationToken, Metadata);

            mediator.TryClose(this, DefaultCancellationToken, Metadata);
            closeCount.ShouldEqual(0);

            mediator.TryClose(_view.Target, DefaultCancellationToken, Metadata);
            closeCount.ShouldEqual(1);
        }

        [Fact]
        public void TryCloseShouldClearView()
        {
            var initCount = 0;
            var clearCount = 0;
            var clearMediatorCount = 0;

            var mediator = GetMediator<object>(_vm, _mapping);
            mediator.InitializeViewHandler = context => { ++initCount; };
            mediator.CleanupViewHandler = context =>
            {
                ++clearMediatorCount;
                mediator.CurrentView.ShouldEqual(_view.Target);
            };
            mediator.CloseViewHandler = context =>
            {
                mediator.OnViewClosed(Metadata);
                return null;
            };
            mediator.ShowViewHandler = context =>
            {
                mediator.OnViewShown(Metadata);
                return null;
            };

            ViewManager.RemoveComponents<TestViewManagerComponent>();
            ViewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (_, _, _, _, _) => new ValueTask<IView?>(_view),
                TryCleanupAsync = (_, v, _, _, _) =>
                {
                    ++clearCount;
                    v.ShouldEqual(_view);
                    return Default.TrueTask;
                }
            });

            mediator.TryShow(null, DefaultCancellationToken, Metadata);
            initCount.ShouldEqual(1);
            clearMediatorCount.ShouldEqual(0);
            clearCount.ShouldEqual(0);

            mediator.TryClose(null, default, Metadata);
            initCount.ShouldEqual(1);
            clearMediatorCount.ShouldEqual(1);
            clearCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void TryCloseShouldNotifyNavigationDispatcher(int state)
        {
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
            var metadata = Metadata;

            var mediator = GetMediator<object>(_vm, _mapping);
            mediator.ShowViewHandler = context =>
            {
                mediator.OnViewShown(Metadata);
                return null;
            };
            mediator.CloseViewHandler = context =>
            {
                if (state == 3)
                    throw exception;
                mediator.OnViewClosed(Metadata);
                return null;
            };
            NavigationDispatcher.RemoveComponents<TestNavigationContextProviderComponent>();
            NavigationDispatcher.AddComponent(new TestNavigationContextProviderComponent
            {
                TryGetNavigationContext = (_, o, provider, nId, type, mode, m) =>
                {
                    ++contextCount;
                    o.ShouldEqual(_vm);
                    provider.ShouldEqual(mediator);
                    navigationId = nId;
                    type.ShouldEqual(mediator.NavigationType);
                    if (!ignore)
                        mode.ShouldEqual(NavigationMode.Close);
                    m.ShouldEqual(metadata);
                    return _navigationContext;
                }
            });
            NavigationDispatcher.AddComponent(new TestNavigationErrorListener
            {
                OnNavigationCanceled = (_, context, arg3) =>
                {
                    ++cancelCount;
                    context.ShouldEqual(_navigationContext);
                    arg3.ShouldEqual(cts.Token);
                },
                OnNavigationFailed = (_, context, arg3) =>
                {
                    ++errorCount;
                    context.ShouldEqual(_navigationContext);
                    arg3.ShouldEqual(exception);
                }
            });
            NavigationDispatcher.AddComponent(new TestNavigationConditionComponent
            {
                CanNavigateAsync = (_, context, t) =>
                {
                    ++navigatingConditionCount;
                    context.ShouldEqual(_navigationContext);
                    return null;
                }
            });
            NavigationDispatcher.AddComponent(new TestNavigationListener
            {
                OnNavigating = (_, context) =>
                {
                    ++navigatingCount;
                    context.ShouldEqual(_navigationContext);
                },
                OnNavigated = (_, context) =>
                {
                    ++navigatedCount;
                    context.ShouldEqual(_navigationContext);
                }
            });

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
            presenterResult.Target.ShouldEqual(_vm);
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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TryCloseShouldWaitNavigationDispatcherOnNavigating(bool result)
        {
            var closeCount = 0;
            var cancelCount = 0;
            var canClose = true;
            var tcs = new TaskCompletionSource<bool>();

            NavigationDispatcher.RemoveComponents<TestNavigationContextProviderComponent>();
            NavigationDispatcher.AddComponent(new TestNavigationContextProviderComponent
            {
                TryGetNavigationContext = (_, o, provider, arg3, arg4, arg5, arg6) => _navigationContext
            });
            NavigationDispatcher.AddComponent(new TestNavigationConditionComponent
            {
                CanNavigateAsync = (_, context, t) =>
                {
                    context.ShouldEqual(_navigationContext);
                    t.CanBeCanceled.ShouldBeTrue();
                    if (canClose)
                        return new ValueTask<bool>(true);
                    return tcs.Task.AsValueTask();
                }
            });
            NavigationDispatcher.AddComponent(new TestNavigationErrorListener
            {
                OnNavigationCanceled = (_, context, t) =>
                {
                    ++cancelCount;
                    context.ShouldEqual(_navigationContext);
                    t.ShouldEqual(DefaultCancellationToken);
                }
            });

            var mediator = GetMediator<object>(_vm, _mapping);
            mediator.ShowViewHandler = context =>
            {
                mediator.OnViewShown(Metadata);
                return null;
            };
            mediator.CloseViewHandler = context =>
            {
                ++closeCount;
                mediator.OnViewClosed(Metadata);
                return null;
            };
            mediator.TryShow(null, DefaultCancellationToken, Metadata);

            canClose = false;
            mediator.TryClose(null, DefaultCancellationToken, Metadata);
            tcs.SetResult(result);
            if (result)
            {
                WaitCompletion(20, () => closeCount == 1);
                closeCount.ShouldEqual(1);
            }
            else
            {
                WaitCompletion(20, () => cancelCount == 1);
                closeCount.ShouldEqual(0);
                cancelCount.ShouldEqual(1);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TryShowShouldActivateView(bool result)
        {
            var showCount = 0;
            var activateCount = 0;
            var navigateCount = 0;
            var cancelCount = 0;

            var mediator = GetMediator<object>(_vm, _mapping);
            mediator.ShowViewHandler = context =>
            {
                ++showCount;
                context.NavigationMode.ShouldEqual(NavigationMode.New);
                mediator.OnViewShown(Metadata);
                return null;
            };
            mediator.ActivateViewHandler = context =>
            {
                ++activateCount;
                context.NavigationMode.ShouldEqual(NavigationMode.Refresh);
                if (result)
                    mediator.OnViewActivated(Metadata);
                return new ValueTask<bool>(result);
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

            mediator.TryShow(null, DefaultCancellationToken, Metadata);
            showCount.ShouldEqual(1);
            navigateCount.ShouldEqual(1);
            cancelCount.ShouldEqual(0);
            activateCount.ShouldEqual(0);

            mediator.TryShow(null, DefaultCancellationToken, Metadata);
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
        public void TryShowShouldCheckNavigationType()
        {
            var showCount = 0;
            var mediator = GetMediator<object>(_vm, _mapping);
            mediator.NavigationTypeField = NavigationType.Page;
            mediator.ShowViewHandler = context =>
            {
                ++showCount;
                return null;
            };
            mediator.TryShow(null, DefaultCancellationToken, NavigationMetadata.NavigationType.ToContext(NavigationType.Background));
            showCount.ShouldEqual(0);

            mediator.TryShow(null, DefaultCancellationToken, NavigationMetadata.NavigationType.ToContext(mediator.NavigationTypeField));
            showCount.ShouldEqual(1);
        }

        [Fact]
        public void TryShowShouldClearViewFailed()
        {
            var initCount = 0;
            var clearCount = 0;
            var clearMediatorCount = 0;

            var mediator = GetMediator<object>(_vm, _mapping);
            mediator.InitializeViewHandler = context => { ++initCount; };
            mediator.CleanupViewHandler = context =>
            {
                ++clearMediatorCount;
                mediator.CurrentView.ShouldEqual(_view.Target);
            };
            mediator.ShowViewHandler = context => throw new Exception();

            ViewManager.RemoveComponents<TestViewManagerComponent>();
            ViewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (_, _, _, _, _) => new ValueTask<IView?>(_view),
                TryCleanupAsync = (_, v, _, _, _) =>
                {
                    ++clearCount;
                    v.ShouldEqual(_view);
                    return Default.TrueTask;
                }
            });

            mediator.TryShow(null, DefaultCancellationToken, Metadata);
            initCount.ShouldEqual(1);
            clearMediatorCount.ShouldEqual(1);
            clearCount.ShouldEqual(1);
        }

        [Fact]
        public void TryShowShouldDetectRestore()
        {
            var showCount = 0;
            var mediator = GetMediator<object>(_vm, _mapping);
            mediator.ShowViewHandler = context =>
            {
                ++showCount;
                context.NavigationMode.ShouldEqual(NavigationMode.New);
                mediator.OnViewShown(Metadata);
                return null;
            };

            mediator.TryShow(null, DefaultCancellationToken, Metadata);
            showCount.ShouldEqual(1);

            showCount = 0;
            mediator = GetMediator<object>(_vm, _mapping);
            mediator.ShowViewHandler = context =>
            {
                ++showCount;
                context.NavigationMode.ShouldEqual(NavigationMode.Restore);
                mediator.OnViewShown(Metadata);
                return null;
            };
            mediator.TryShow(null, DefaultCancellationToken, Metadata);
            showCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void TryShowShouldNotifyNavigationDispatcher(int state)
        {
            var cts = new CancellationTokenSource();
            var exception = new Exception();
            string? navigationId = null;
            var navigatingCount = 0;
            var navigatedCount = 0;
            var navigatingConditionCount = 0;
            var errorCount = 0;
            var cancelCount = 0;
            var contextCount = 0;

            var mediator = GetMediator<object>(_vm, _mapping);
            mediator.ShowViewHandler = context =>
            {
                mediator.OnViewShown(Metadata);
                return null;
            };
            NavigationDispatcher.RemoveComponents<TestNavigationContextProviderComponent>();
            ;
            NavigationDispatcher.AddComponent(new TestNavigationContextProviderComponent
            {
                TryGetNavigationContext = (_, o, provider, nId, type, mode, m) =>
                {
                    ++contextCount;
                    o.ShouldEqual(_vm);
                    provider.ShouldEqual(mediator);
                    navigationId = nId;
                    type.ShouldEqual(mediator.NavigationType);
                    mode.ShouldEqual(NavigationMode.New);
                    m.ShouldEqual(Metadata);
                    return _navigationContext;
                }
            });
            NavigationDispatcher.AddComponent(new TestNavigationErrorListener
            {
                OnNavigationCanceled = (_, context, arg3) =>
                {
                    ++cancelCount;
                    context.ShouldEqual(_navigationContext);
                    arg3.ShouldEqual(cts.Token);
                },
                OnNavigationFailed = (_, context, arg3) =>
                {
                    ++errorCount;
                    context.ShouldEqual(_navigationContext);
                    arg3.ShouldEqual(exception);
                }
            });
            NavigationDispatcher.AddComponent(new TestNavigationConditionComponent
            {
                CanNavigateAsync = (_, context, t) =>
                {
                    ++navigatingConditionCount;
                    context.ShouldEqual(_navigationContext);
                    return null;
                }
            });
            NavigationDispatcher.AddComponent(new TestNavigationListener
            {
                OnNavigating = (_, context) =>
                {
                    ++navigatingCount;
                    context.ShouldEqual(_navigationContext);
                },
                OnNavigated = (_, context) =>
                {
                    ++navigatedCount;
                    context.ShouldEqual(_navigationContext);
                }
            });
            ViewManager.RemoveComponents<TestViewManagerComponent>();
            ViewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (_, _, _, _, _) =>
                {
                    if (state == 1)
                        return new ValueTask<IView?>(_view);
                    if (state == 2)
                    {
                        cts.Cancel();
                        return Task.FromCanceled<IView?>(cts.Token).AsValueTask();
                    }

                    return exception.ToTask<IView?>().AsValueTask();
                }
            });

            var presenterResult = mediator.TryShow(null, cts.Token, Metadata)!;
            contextCount.ShouldEqual(1);
            presenterResult.NavigationProvider.ShouldEqual(mediator);
            presenterResult.NavigationType.ShouldEqual(mediator.NavigationType);
            presenterResult.Target.ShouldEqual(_vm);
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
            var initCount = 0;
            var mediator = GetMediator<object>(_vm, _mapping);
            mediator.InitializeViewHandler = context => { ++initCount; };
            mediator.ShowViewHandler = context =>
            {
                mediator.OnViewShown(Metadata);
                return null;
            };
            ViewManager.RemoveComponents<TestViewManagerComponent>();
            ViewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (_, viewMapping, r, _, token) =>
                {
                    viewMapping.ShouldEqual(_mapping);
                    if (includeView)
                    {
                        var request = (ViewModelViewRequest)r;
                        request.ViewModel.ShouldEqual(_vm);
                        request.View.ShouldEqual(_view.Target);
                    }
                    else
                        r.ShouldEqual(_vm);

                    token.CanBeCanceled.ShouldBeTrue();
                    return new ValueTask<IView?>(_view);
                }
            });

            mediator.TryShow(includeView ? _view.Target : null, DefaultCancellationToken, Metadata);
            initCount.ShouldEqual(1);
            mediator.View.ShouldEqual(_view);
            mediator.CurrentView.ShouldEqual(_view.Target);
        }

        [Fact]
        public void TryShowShouldUpdateView()
        {
            var newView = new View(_mapping, new object(), _vm);
            var cleanupCount = 0;
            var showCount = 0;
            var activateCount = 0;

            var mediator = GetMediator<object>(_vm, _mapping);
            mediator.CleanupViewHandler = context => { ++cleanupCount; };
            mediator.ShowViewHandler = context =>
            {
                ++showCount;
                mediator.OnViewShown(Metadata);
                return null;
            };
            mediator.ActivateViewHandler = context =>
            {
                ++activateCount;
                mediator.OnViewActivated(Metadata);
                return new ValueTask<bool>(true);
            };

            var cleanupViewCount = 0;
            ViewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (_, _, r, _, _) =>
                {
                    MugenExtensions.TryGetViewModelView(r, out object? v);
                    if (v == null)
                        return new ValueTask<IView?>(_view);
                    return new ValueTask<IView?>(newView);
                },
                TryCleanupAsync = (_, v, _, _, _) =>
                {
                    ++cleanupViewCount;
                    v.ShouldEqual(_view);
                    return Default.TrueTask;
                }
            });

            mediator.TryShow(null, DefaultCancellationToken, Metadata);
            mediator.View.ShouldEqual(_view);
            mediator.CurrentView.ShouldEqual(_view.Target);
            cleanupCount.ShouldEqual(0);
            activateCount.ShouldEqual(0);
            showCount.ShouldEqual(1);
            cleanupViewCount.ShouldEqual(0);

            mediator.TryShow(newView.Target, DefaultCancellationToken, default);
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
            var showCount = 0;
            var cancelCount = 0;
            var tcs = new TaskCompletionSource<bool>();

            NavigationDispatcher.RemoveComponents<TestNavigationContextProviderComponent>();
            NavigationDispatcher.AddComponent(new TestNavigationContextProviderComponent
            {
                TryGetNavigationContext = (_, o, provider, arg3, arg4, arg5, arg6) => _navigationContext
            });
            NavigationDispatcher.AddComponent(new TestNavigationConditionComponent
            {
                CanNavigateAsync = (_, context, t) =>
                {
                    context.ShouldEqual(_navigationContext);
                    t.CanBeCanceled.ShouldBeTrue();
                    return tcs.Task.AsValueTask();
                }
            });
            NavigationDispatcher.AddComponent(new TestNavigationErrorListener
            {
                OnNavigationCanceled = (_, context, t) =>
                {
                    ++cancelCount;
                    context.ShouldEqual(_navigationContext);
                    t.CanBeCanceled.ShouldBeTrue();
                }
            });

            var mediator = GetMediator<object>(_vm, _mapping);
            mediator.ShowViewHandler = context =>
            {
                ++showCount;
                return null;
            };
            mediator.TryShow(null, DefaultCancellationToken, Metadata);
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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TryShowShouldWrapView(bool includeView)
        {
            var initCount = 0;
            var wrappedView = "v";

            var mediator = GetMediator<string>(_vm, _mapping);
            mediator.InitializeViewHandler = context => ++initCount;
            mediator.ShowViewHandler = context =>
            {
                mediator.OnViewShown(Metadata);
                return null;
            };

            WrapperManager.AddComponent(new DelegateWrapperManager<Type, object>((type, type1, arg4) => true, (type, request, __) =>
            {
                type.ShouldEqual(typeof(string));
                request.ShouldEqual(_view);
                return wrappedView;
            }));

            mediator.TryShow(includeView ? _view.Target : null, DefaultCancellationToken, Metadata);
            initCount.ShouldEqual(1);
            mediator.View.ShouldEqual(_view);
            mediator.CurrentView.ShouldEqual(wrappedView);
        }

        protected override INavigationDispatcher GetNavigationDispatcher() => new NavigationDispatcher(ComponentCollectionManager);

        protected override IViewManager GetViewManager() => new ViewManager(ComponentCollectionManager);

        protected override IWrapperManager GetWrapperManager() => new WrapperManager(ComponentCollectionManager);

        protected override IViewModelManager GetViewModelManager() => new ViewModelManager(ComponentCollectionManager);

        protected virtual TestViewModelPresenterMediatorBase<T> GetMediator<T>(IViewModelBase viewModel, IViewMapping viewMapping) where T : class =>
            new(viewModel, viewMapping, ViewManager, WrapperManager, NavigationDispatcher, ThreadDispatcher, ViewModelManager);
    }
}