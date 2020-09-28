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
    public class NavigationCallbackTargetDispatcherTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void OnNavigatingShouldInvokeCallbackClose()
        {
            var dispatcher = new NavigationDispatcher();
            var prevTarget = new HasNavigatingCallback(dispatcher);
            var target = new HasNavigatingCallback(dispatcher);
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
            dispatcher.AddComponent(new NavigationCallbackTargetDispatcher());
            dispatcher.AddComponent(new TestNavigationEntryProviderComponent
            {
                TryGetNavigationEntries = m => prevEntry
            });

            dispatcher.OnNavigating(context);
            targetInvokeCount.ShouldEqual(1);
            prevTargetInvokeCount.ShouldEqual(1);
        }

        [Fact]
        public void OnNavigatingShouldInvokeCallbackNew()
        {
            var dispatcher = new NavigationDispatcher();
            var prevTarget = new HasNavigatingCallback(dispatcher);
            var target = new HasNavigatingCallback(dispatcher);
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
            dispatcher.AddComponent(new NavigationCallbackTargetDispatcher());
            dispatcher.AddComponent(new TestNavigationEntryProviderComponent
            {
                TryGetNavigationEntries = m => prevEntry
            });

            dispatcher.OnNavigating(context);
            targetInvokeCount.ShouldEqual(1);
            prevTargetInvokeCount.ShouldEqual(1);
        }

        [Fact]
        public void OnNavigatedShouldInvokeCallbackClose()
        {
            var dispatcher = new NavigationDispatcher();
            var prevTarget = new HasNavigatedCallback(dispatcher);
            var target = new HasNavigatedCallback(dispatcher);
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
            dispatcher.AddComponent(new NavigationCallbackTargetDispatcher());
            dispatcher.AddComponent(new TestNavigationEntryProviderComponent
            {
                TryGetNavigationEntries = m => prevEntry
            });

            dispatcher.OnNavigated(context);
            targetInvokeCount.ShouldEqual(1);
            prevTargetInvokeCount.ShouldEqual(1);
        }

        [Fact]
        public void OnNavigatedShouldInvokeCallbackNew()
        {
            var dispatcher = new NavigationDispatcher();
            var prevTarget = new HasNavigatedCallback(dispatcher);
            var target = new HasNavigatedCallback(dispatcher);
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
            dispatcher.AddComponent(new NavigationCallbackTargetDispatcher());
            dispatcher.AddComponent(new TestNavigationEntryProviderComponent
            {
                TryGetNavigationEntries = m => prevEntry
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
        public void CanNavigateAsyncShouldInvokeCallbackClose(bool includeTarget, bool includePrevTarget)
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
            dispatcher.AddComponent(new NavigationCallbackTargetDispatcher());
            dispatcher.AddComponent(new TestNavigationEntryProviderComponent
            {
                TryGetNavigationEntries = m =>
                {
                    if (includePrevTarget)
                        return prevEntry;
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
                WaitCompletion();
                prevTargetInvokeCount.ShouldEqual(1);
                tcsPrevTarget.SetResult(true);
            }

            task.WaitEx();
            task.IsCompleted.ShouldBeTrue();
            task.Result.ShouldBeTrue();
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void CanNavigateAsyncShouldInvokeCallbackNew(bool includeTarget, bool includePrevTarget)
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
            dispatcher.AddComponent(new NavigationCallbackTargetDispatcher());
            dispatcher.AddComponent(new TestNavigationEntryProviderComponent
            {
                TryGetNavigationEntries = m =>
                {
                    if (includePrevTarget)
                        return prevEntry;
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
                WaitCompletion();
                targetInvokeCount.ShouldEqual(1);
                tcsTarget.SetResult(true);
            }

            task.WaitEx();
            task.IsCompleted.ShouldBeTrue();
            task.Result.ShouldBeTrue();
        }

        #endregion

        #region Nested types

        private sealed class HasNavigationCondition : IHasNavigationConditionCallback
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

            Task<bool> IHasNavigationConditionCallback.CanNavigateFromAsync(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, object? toTarget, CancellationToken cancellationToken)
            {
                _navigationDispatcher.ShouldEqual(navigationDispatcher);
                return CanNavigateFromAsync?.Invoke(toTarget, navigationContext, cancellationToken) ?? Default.TrueTask;
            }

            Task<bool> IHasNavigationConditionCallback.CanNavigateToAsync(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, object? fromTarget, CancellationToken cancellationToken)
            {
                _navigationDispatcher.ShouldEqual(navigationDispatcher);
                return CanNavigateToAsync?.Invoke(fromTarget, navigationContext, cancellationToken) ?? Default.TrueTask;
            }

            #endregion
        }

        private sealed class HasNavigatingCallback : IHasNavigatingCallback
        {
            #region Fields

            private readonly INavigationDispatcher _navigationDispatcher;

            #endregion

            #region Constructors

            public HasNavigatingCallback(INavigationDispatcher navigationDispatcher)
            {
                _navigationDispatcher = navigationDispatcher;
            }

            #endregion

            #region Properties

            public Action<object?, INavigationContext>? OnNavigatingFrom { get; set; }

            public Action<object?, INavigationContext>? OnNavigatingTo { get; set; }

            #endregion

            #region Implementation of interfaces

            void IHasNavigatingCallback.OnNavigatingFrom(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, object? toTarget)
            {
                _navigationDispatcher.ShouldEqual(navigationDispatcher);
                OnNavigatingFrom?.Invoke(toTarget, navigationContext);
            }

            void IHasNavigatingCallback.OnNavigatingTo(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, object? fromTarget)
            {
                _navigationDispatcher.ShouldEqual(navigationDispatcher);
                OnNavigatingTo?.Invoke(fromTarget, navigationContext);
            }

            #endregion
        }

        private sealed class HasNavigatedCallback : IHasNavigatedCallback
        {
            #region Fields

            private readonly INavigationDispatcher _navigationDispatcher;

            #endregion

            #region Constructors

            public HasNavigatedCallback(INavigationDispatcher navigationDispatcher)
            {
                _navigationDispatcher = navigationDispatcher;
            }

            #endregion

            #region Properties

            public Action<object?, INavigationContext>? OnNavigatedFrom { get; set; }

            public Action<object?, INavigationContext>? OnNavigatedTo { get; set; }

            #endregion

            #region Implementation of interfaces

            void IHasNavigatedCallback.OnNavigatedFrom(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, object? toTarget)
            {
                _navigationDispatcher.ShouldEqual(navigationDispatcher);
                OnNavigatedFrom?.Invoke(toTarget, navigationContext);
            }

            void IHasNavigatedCallback.OnNavigatedTo(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, object? fromTarget)
            {
                _navigationDispatcher.ShouldEqual(navigationDispatcher);
                OnNavigatedTo?.Invoke(fromTarget, navigationContext);
            }

            #endregion
        }

        #endregion
    }
}