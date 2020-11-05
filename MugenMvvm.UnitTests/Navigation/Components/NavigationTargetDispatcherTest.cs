using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Internal;
using MugenMvvm.Navigation;
using MugenMvvm.Navigation.Components;
using MugenMvvm.UnitTests.Navigation.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Navigation.Components
{
    public class NavigationTargetDispatcherTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void OnNavigatingShouldInvokeCallbackClose()
        {
            var dispatcher = new NavigationDispatcher();
            var prevTarget = new HasNavigatingHandler(dispatcher);
            var target = new HasNavigatingHandler(dispatcher);
            var context = new NavigationContext(target, Default.NavigationProvider, "-", NavigationType.Page, NavigationMode.Close);
            var targetInvokeCount = 0;
            var prevTargetInvokeCount = 0;

            prevTarget.OnNavigatingFrom = (o, navigationContext) => throw new NotSupportedException();
            prevTarget.OnNavigatingTo = (o, navigationContext) =>
            {
                navigationContext.ShouldEqual(context);
                o.ShouldEqual(target);
                ++prevTargetInvokeCount;
            };
            target.OnNavigatingFrom = (o, navigationContext) =>
            {
                navigationContext.ShouldEqual(context);
                o.ShouldEqual(prevTarget);
                ++targetInvokeCount;
            };
            target.OnNavigatingTo = (o, navigationContext) => throw new NotSupportedException();

            var prevEntry = new NavigationEntry(prevTarget, Default.NavigationProvider, "-", NavigationType.Page);
            dispatcher.AddComponent(new NavigationTargetDispatcher());
            dispatcher.AddComponent(new TestNavigationEntryProviderComponent
            {
                TryGetNavigationEntries = m => ItemOrList.FromItem<INavigationEntry>(prevEntry)
            });

            dispatcher.OnNavigating(context);
            targetInvokeCount.ShouldEqual(1);
            prevTargetInvokeCount.ShouldEqual(1);
        }

        [Fact]
        public void OnNavigatingShouldInvokeCallbackNew()
        {
            var dispatcher = new NavigationDispatcher();
            var prevTarget = new HasNavigatingHandler(dispatcher);
            var target = new HasNavigatingHandler(dispatcher);
            var context = new NavigationContext(target, Default.NavigationProvider, "-", NavigationType.Page, NavigationMode.New);
            var targetInvokeCount = 0;
            var prevTargetInvokeCount = 0;

            prevTarget.OnNavigatingFrom = (o, navigationContext) =>
            {
                navigationContext.ShouldEqual(context);
                o.ShouldEqual(target);
                ++prevTargetInvokeCount;
            };
            prevTarget.OnNavigatingTo = (o, navigationContext) => throw new NotSupportedException();
            target.OnNavigatingFrom = (o, navigationContext) => throw new NotSupportedException();
            target.OnNavigatingTo = (o, navigationContext) =>
            {
                navigationContext.ShouldEqual(context);
                o.ShouldEqual(prevTarget);
                ++targetInvokeCount;
            };

            var prevEntry = new NavigationEntry(prevTarget, Default.NavigationProvider, "-", NavigationType.Page);
            dispatcher.AddComponent(new NavigationTargetDispatcher());
            dispatcher.AddComponent(new TestNavigationEntryProviderComponent
            {
                TryGetNavigationEntries = m => ItemOrList.FromItem<INavigationEntry>(prevEntry)
            });

            dispatcher.OnNavigating(context);
            targetInvokeCount.ShouldEqual(1);
            prevTargetInvokeCount.ShouldEqual(1);
        }

        [Fact]
        public void OnNavigatedShouldInvokeCallbackClose()
        {
            var dispatcher = new NavigationDispatcher();
            var prevTarget = new HasNavigatedHandler(dispatcher);
            var target = new HasNavigatedHandler(dispatcher);
            var context = new NavigationContext(target, Default.NavigationProvider, "-", NavigationType.Page, NavigationMode.Close);
            var targetInvokeCount = 0;
            var prevTargetInvokeCount = 0;

            prevTarget.OnNavigatedFrom = (o, navigationContext) => throw new NotSupportedException();
            prevTarget.OnNavigatedTo = (o, navigationContext) =>
            {
                navigationContext.ShouldEqual(context);
                o.ShouldEqual(target);
                ++prevTargetInvokeCount;
            };
            target.OnNavigatedFrom = (o, navigationContext) =>
            {
                navigationContext.ShouldEqual(context);
                o.ShouldEqual(prevTarget);
                ++targetInvokeCount;
            };
            target.OnNavigatedTo = (o, navigationContext) => throw new NotSupportedException();

            var prevEntry = new NavigationEntry(prevTarget, Default.NavigationProvider, "-", NavigationType.Page);
            dispatcher.AddComponent(new NavigationTargetDispatcher());
            dispatcher.AddComponent(new TestNavigationEntryProviderComponent
            {
                TryGetNavigationEntries = m => ItemOrList.FromItem<INavigationEntry>(prevEntry)
            });

            dispatcher.OnNavigated(context);
            targetInvokeCount.ShouldEqual(1);
            prevTargetInvokeCount.ShouldEqual(1);
        }

        [Fact]
        public void OnNavigatedShouldInvokeCallbackNew()
        {
            var dispatcher = new NavigationDispatcher();
            var prevTarget = new HasNavigatedHandler(dispatcher);
            var target = new HasNavigatedHandler(dispatcher);
            var context = new NavigationContext(target, Default.NavigationProvider, "-", NavigationType.Page, NavigationMode.New);
            var targetInvokeCount = 0;
            var prevTargetInvokeCount = 0;

            prevTarget.OnNavigatedFrom = (o, navigationContext) =>
            {
                navigationContext.ShouldEqual(context);
                o.ShouldEqual(target);
                ++prevTargetInvokeCount;
            };
            prevTarget.OnNavigatedTo = (o, navigationContext) => throw new NotSupportedException();
            target.OnNavigatedFrom = (o, navigationContext) => throw new NotSupportedException();
            target.OnNavigatedTo = (o, navigationContext) =>
            {
                navigationContext.ShouldEqual(context);
                o.ShouldEqual(prevTarget);
                ++targetInvokeCount;
            };

            var prevEntry = new NavigationEntry(prevTarget, Default.NavigationProvider, "-", NavigationType.Page);
            dispatcher.AddComponent(new NavigationTargetDispatcher());
            dispatcher.AddComponent(new TestNavigationEntryProviderComponent
            {
                TryGetNavigationEntries = m => ItemOrList.FromItem<INavigationEntry>(prevEntry)
            });

            dispatcher.OnNavigated(context);
            targetInvokeCount.ShouldEqual(1);
            prevTargetInvokeCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public async Task CanNavigateAsyncShouldInvokeCallbackClose(bool includeTarget, bool includePrevTarget)
        {
            var dispatcher = new NavigationDispatcher();
            var tcsPrevTarget = new TaskCompletionSource<bool>();
            var tcsTarget = new TaskCompletionSource<bool>();
            var prevTarget = new HasNavigationCondition(dispatcher);
            var target = new HasNavigationCondition(dispatcher);
            var context = new NavigationContext(includeTarget ? target : new object(), Default.NavigationProvider, "-", NavigationType.Page, NavigationMode.Close);
            var targetInvokeCount = 0;
            var prevTargetInvokeCount = 0;

            prevTarget.CanNavigateFromAsync = (o, navigationContext, c) => throw new NotSupportedException();
            prevTarget.CanNavigateToAsync = (o, navigationContext, c) =>
            {
                ++prevTargetInvokeCount;
                o.ShouldEqual(navigationContext.Target);
                navigationContext.ShouldEqual(context);
                return tcsPrevTarget.Task;
            };
            target.CanNavigateFromAsync = (o, navigationContext, c) =>
            {
                ++targetInvokeCount;
                if (includePrevTarget)
                    o.ShouldEqual(prevTarget);
                else
                    o.ShouldBeNull();
                navigationContext.ShouldEqual(context);
                return tcsTarget.Task;
            };
            target.CanNavigateToAsync = (o, navigationContext, c) => throw new NotSupportedException();

            var prevEntry = new NavigationEntry(prevTarget, Default.NavigationProvider, "-", NavigationType.Page);
            dispatcher.AddComponent(new NavigationTargetDispatcher());
            dispatcher.AddComponent(new TestNavigationEntryProviderComponent
            {
                TryGetNavigationEntries = m =>
                {
                    if (includePrevTarget)
                        return ItemOrList.FromItem<INavigationEntry>(prevEntry);
                    return default;
                }
            });

            var task = dispatcher.OnNavigatingAsync(context);
            if (includeTarget)
            {
                task.IsCompleted.ShouldBeFalse();
                targetInvokeCount.ShouldEqual(1);
                prevTargetInvokeCount.ShouldEqual(0);
                tcsTarget.SetResult(true);
            }

            if (includePrevTarget)
            {
                task.IsCompleted.ShouldBeFalse();
                tcsPrevTarget.SetResult(true);
                await task;
                prevTargetInvokeCount.ShouldEqual(1);
            }

            await task;
            task.IsCompleted.ShouldBeTrue();
            task.Result.ShouldBeTrue();
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public async Task CanNavigateAsyncShouldInvokeCallbackNew(bool includeTarget, bool includePrevTarget)
        {
            var dispatcher = new NavigationDispatcher();
            var tcsPrevTarget = new TaskCompletionSource<bool>();
            var tcsTarget = new TaskCompletionSource<bool>();
            var prevTarget = new HasNavigationCondition(dispatcher);
            var target = new HasNavigationCondition(dispatcher);
            var context = new NavigationContext(includeTarget ? target : new object(), Default.NavigationProvider, "-", NavigationType.Page, NavigationMode.New);
            var targetInvokeCount = 0;
            var prevTargetInvokeCount = 0;

            prevTarget.CanNavigateFromAsync = (o, navigationContext, c) =>
            {
                ++prevTargetInvokeCount;
                o.ShouldEqual(navigationContext.Target);
                navigationContext.ShouldEqual(context);
                return tcsPrevTarget.Task;
            };
            prevTarget.CanNavigateToAsync = (o, navigationContext, c) => throw new NotSupportedException();
            target.CanNavigateFromAsync = (o, navigationContext, c) => throw new NotSupportedException();
            target.CanNavigateToAsync = (o, navigationContext, c) =>
            {
                ++targetInvokeCount;
                if (includePrevTarget)
                    o.ShouldEqual(prevTarget);
                else
                    o.ShouldBeNull();
                navigationContext.ShouldEqual(context);
                return tcsTarget.Task;
            };

            var prevEntry = new NavigationEntry(prevTarget, Default.NavigationProvider, "-", NavigationType.Page);
            dispatcher.AddComponent(new NavigationTargetDispatcher());
            dispatcher.AddComponent(new TestNavigationEntryProviderComponent
            {
                TryGetNavigationEntries = m =>
                {
                    if (includePrevTarget)
                        return ItemOrList.FromItem<INavigationEntry>(prevEntry);
                    return default;
                }
            });

            var task = dispatcher.OnNavigatingAsync(context);
            if (includePrevTarget)
            {
                task.IsCompleted.ShouldBeFalse();
                prevTargetInvokeCount.ShouldEqual(1);
                targetInvokeCount.ShouldEqual(0);
                tcsPrevTarget.SetResult(true);
            }

            if (includeTarget)
            {
                task.IsCompleted.ShouldBeFalse();
                tcsTarget.SetResult(true);
                await task;
                targetInvokeCount.ShouldEqual(1);
            }

            await task;
            task.IsCompleted.ShouldBeTrue();
            task.Result.ShouldBeTrue();
        }

        [Fact]
        public void NavigationShouldInvokeCallbacks()
        {
            var dispatcher = new NavigationDispatcher();
            var target = new HasCloseNavigationHandler(dispatcher);
            dispatcher.AddComponent(new NavigationTargetDispatcher());
            var closeCtx = new NavigationContext(target, Default.NavigationProvider, "0", NavigationType.Popup, NavigationMode.Close);
            var newCtx = new NavigationContext(target, Default.NavigationProvider, "0", NavigationType.Popup, NavigationMode.New);
            var closingCount = 0;
            var closeCount = 0;
            target.OnClosing = context =>
            {
                context.ShouldEqual(closeCtx);
                closingCount++;
            };
            target.OnClosed = context =>
            {
                context.ShouldEqual(closeCtx);
                ++closeCount;
            };

            dispatcher.OnNavigating(closeCtx);
            closingCount.ShouldEqual(1);
            closeCount.ShouldEqual(0);

            dispatcher.OnNavigated(closeCtx);
            closingCount.ShouldEqual(1);
            closeCount.ShouldEqual(1);

            dispatcher.OnNavigating(newCtx);
            dispatcher.OnNavigated(newCtx);
            closingCount.ShouldEqual(1);
            closeCount.ShouldEqual(1);
        }

        [Fact]
        public void NavigationConditionShouldInvokeCallback()
        {
            var dispatcher = new NavigationDispatcher();
            var target = new HasCloseNavigationCondition(dispatcher);
            dispatcher.AddComponent(new NavigationTargetDispatcher());
            var cts = new CancellationTokenSource().Token;
            var closeCtx = new NavigationContext(target, Default.NavigationProvider, "0", NavigationType.Popup, NavigationMode.Close);
            var newCtx = new NavigationContext(target, Default.NavigationProvider, "0", NavigationType.Popup, NavigationMode.New);
            var closeCount = 0;
            var result = Task.FromResult(false);
            target.CanCloseAsync = (context, token) =>
            {
                context.ShouldEqual(closeCtx);
                token.ShouldEqual(cts);
                ++closeCount;
                return result;
            };

            dispatcher.OnNavigatingAsync(closeCtx, cts).Result.ShouldBeFalse();
            closeCount.ShouldEqual(1);

            dispatcher.OnNavigatingAsync(newCtx, cts).Result.ShouldBeTrue();
            closeCount.ShouldEqual(1);
        }

        #endregion

        #region Nested types

        private sealed class HasCloseNavigationCondition : IHasCloseNavigationCondition
        {
            #region Fields

            private readonly INavigationDispatcher _navigationDispatcher;

            #endregion

            #region Constructors

            public HasCloseNavigationCondition(INavigationDispatcher navigationDispatcher)
            {
                _navigationDispatcher = navigationDispatcher;
            }

            #endregion

            #region Properties

            public Func<INavigationContext, CancellationToken, Task<bool>>? CanCloseAsync { get; set; }

            #endregion

            #region Implementation of interfaces

            Task<bool> IHasCloseNavigationCondition.CanCloseAsync(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, CancellationToken cancellationToken)
            {
                _navigationDispatcher.ShouldEqual(navigationDispatcher);
                return CanCloseAsync?.Invoke(navigationContext, cancellationToken) ?? Default.TrueTask;
            }

            #endregion
        }

        private sealed class HasCloseNavigationHandler : IHasCloseNavigationHandler
        {
            #region Fields

            private readonly INavigationDispatcher _navigationDispatcher;

            #endregion

            #region Constructors

            public HasCloseNavigationHandler(INavigationDispatcher navigationDispatcher)
            {
                _navigationDispatcher = navigationDispatcher;
            }

            #endregion

            #region Properties

            public Action<INavigationContext>? OnClosing { get; set; }

            public Action<INavigationContext>? OnClosed { get; set; }

            #endregion

            #region Implementation of interfaces

            void IHasCloseNavigationHandler.OnClosing(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
            {
                _navigationDispatcher.ShouldEqual(navigationDispatcher);
                OnClosing?.Invoke(navigationContext);
            }

            void IHasCloseNavigationHandler.OnClosed(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
            {
                _navigationDispatcher.ShouldEqual(navigationDispatcher);
                OnClosed?.Invoke(navigationContext);
            }

            #endregion
        }

        private sealed class HasNavigationCondition : IHasNavigationCondition
        {
            #region Fields

            private readonly INavigationDispatcher _navigationDispatcher;

            #endregion

            #region Constructors

            public HasNavigationCondition(INavigationDispatcher navigationDispatcher)
            {
                _navigationDispatcher = navigationDispatcher;
            }

            #endregion

            #region Properties

            public Func<object?, INavigationContext, CancellationToken, Task<bool>?>? CanNavigateFromAsync { get; set; }

            public Func<object?, INavigationContext, CancellationToken, Task<bool>?>? CanNavigateToAsync { get; set; }

            #endregion

            #region Implementation of interfaces

            Task<bool> IHasNavigationCondition.CanNavigateFromAsync(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, object? toTarget, CancellationToken cancellationToken)
            {
                _navigationDispatcher.ShouldEqual(navigationDispatcher);
                return CanNavigateFromAsync?.Invoke(toTarget, navigationContext, cancellationToken) ?? Default.TrueTask;
            }

            Task<bool> IHasNavigationCondition.CanNavigateToAsync(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, object? fromTarget, CancellationToken cancellationToken)
            {
                _navigationDispatcher.ShouldEqual(navigationDispatcher);
                return CanNavigateToAsync?.Invoke(fromTarget, navigationContext, cancellationToken) ?? Default.TrueTask;
            }

            #endregion
        }

        private sealed class HasNavigatingHandler : IHasNavigatingHandler
        {
            #region Fields

            private readonly INavigationDispatcher _navigationDispatcher;

            #endregion

            #region Constructors

            public HasNavigatingHandler(INavigationDispatcher navigationDispatcher)
            {
                _navigationDispatcher = navigationDispatcher;
            }

            #endregion

            #region Properties

            public Action<object?, INavigationContext>? OnNavigatingFrom { get; set; }

            public Action<object?, INavigationContext>? OnNavigatingTo { get; set; }

            #endregion

            #region Implementation of interfaces

            void IHasNavigatingHandler.OnNavigatingFrom(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, object? toTarget)
            {
                _navigationDispatcher.ShouldEqual(navigationDispatcher);
                OnNavigatingFrom?.Invoke(toTarget, navigationContext);
            }

            void IHasNavigatingHandler.OnNavigatingTo(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, object? fromTarget)
            {
                _navigationDispatcher.ShouldEqual(navigationDispatcher);
                OnNavigatingTo?.Invoke(fromTarget, navigationContext);
            }

            #endregion
        }

        private sealed class HasNavigatedHandler : IHasNavigatedHandler
        {
            #region Fields

            private readonly INavigationDispatcher _navigationDispatcher;

            #endregion

            #region Constructors

            public HasNavigatedHandler(INavigationDispatcher navigationDispatcher)
            {
                _navigationDispatcher = navigationDispatcher;
            }

            #endregion

            #region Properties

            public Action<object?, INavigationContext>? OnNavigatedFrom { get; set; }

            public Action<object?, INavigationContext>? OnNavigatedTo { get; set; }

            #endregion

            #region Implementation of interfaces

            void IHasNavigatedHandler.OnNavigatedFrom(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, object? toTarget)
            {
                _navigationDispatcher.ShouldEqual(navigationDispatcher);
                OnNavigatedFrom?.Invoke(toTarget, navigationContext);
            }

            void IHasNavigatedHandler.OnNavigatedTo(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, object? fromTarget)
            {
                _navigationDispatcher.ShouldEqual(navigationDispatcher);
                OnNavigatedTo?.Invoke(fromTarget, navigationContext);
            }

            #endregion
        }

        #endregion
    }
}