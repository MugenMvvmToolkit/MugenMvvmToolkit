using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Internal;
using MugenMvvm.Navigation;
using MugenMvvm.Navigation.Components;
using MugenMvvm.UnitTest.Navigation.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Navigation.Components
{
    public class NavigationTargetCallbackDispatcherTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void OnNavigatingShouldInvokeCallbackClose()
        {
            var prevTarget = new HasNavigatingCallback();
            var target = new HasNavigatingCallback();
            var context = new NavigationContext(target, Default.NavigationProvider, "-", NavigationType.Page, NavigationMode.Close);
            int targetInvokeCount = 0;
            int prevTargetInvokeCount = 0;

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
            var dispatcher = new NavigationDispatcher();
            dispatcher.AddComponent(new NavigationTargetCallbackDispatcher());
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
            var prevTarget = new HasNavigatingCallback();
            var target = new HasNavigatingCallback();
            var context = new NavigationContext(target, Default.NavigationProvider, "-", NavigationType.Page, NavigationMode.New);
            int targetInvokeCount = 0;
            int prevTargetInvokeCount = 0;

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
            var dispatcher = new NavigationDispatcher();
            dispatcher.AddComponent(new NavigationTargetCallbackDispatcher());
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
            var prevTarget = new HasNavigatedCallback();
            var target = new HasNavigatedCallback();
            var context = new NavigationContext(target, Default.NavigationProvider, "-", NavigationType.Page, NavigationMode.Close);
            int targetInvokeCount = 0;
            int prevTargetInvokeCount = 0;

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
            var dispatcher = new NavigationDispatcher();
            dispatcher.AddComponent(new NavigationTargetCallbackDispatcher());
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
            var prevTarget = new HasNavigatedCallback();
            var target = new HasNavigatedCallback();
            var context = new NavigationContext(target, Default.NavigationProvider, "-", NavigationType.Page, NavigationMode.New);
            int targetInvokeCount = 0;
            int prevTargetInvokeCount = 0;

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
            var dispatcher = new NavigationDispatcher();
            dispatcher.AddComponent(new NavigationTargetCallbackDispatcher());
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
            var tcsPrevTarget = new TaskCompletionSource<bool>();
            var tcsTarget = new TaskCompletionSource<bool>();
            var prevTarget = new HasNavigationCondition();
            var target = new HasNavigationCondition();
            var context = new NavigationContext(includeTarget ? target : new object(), Default.NavigationProvider, "-", NavigationType.Page, NavigationMode.Close);
            int targetInvokeCount = 0;
            int prevTargetInvokeCount = 0;

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
            var dispatcher = new NavigationDispatcher();
            dispatcher.AddComponent(new NavigationTargetCallbackDispatcher());
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
                prevTargetInvokeCount.ShouldEqual(1);
                tcsPrevTarget.SetResult(true);
            }

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
            var tcsPrevTarget = new TaskCompletionSource<bool>();
            var tcsTarget = new TaskCompletionSource<bool>();
            var prevTarget = new HasNavigationCondition();
            var target = new HasNavigationCondition();
            var context = new NavigationContext(includeTarget ? target : new object(), Default.NavigationProvider, "-", NavigationType.Page, NavigationMode.New);
            int targetInvokeCount = 0;
            int prevTargetInvokeCount = 0;

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
            var dispatcher = new NavigationDispatcher();
            dispatcher.AddComponent(new NavigationTargetCallbackDispatcher());
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
                targetInvokeCount.ShouldEqual(1);
                tcsTarget.SetResult(true);
            }

            task.IsCompleted.ShouldBeTrue();
            task.Result.ShouldBeTrue();
        }

        #endregion

        #region Nested types

        private sealed class HasNavigationCondition : IHasNavigationCondition
        {
            #region Properties

            public Func<object?, INavigationContext, CancellationToken, Task<bool>?>? CanNavigateFromAsync { get; set; }

            public Func<object?, INavigationContext, CancellationToken, Task<bool>?>? CanNavigateToAsync { get; set; }

            #endregion

            #region Implementation of interfaces

            Task<bool>? IHasNavigationCondition.CanNavigateFromAsync(object? toTarget, INavigationContext navigationContext, CancellationToken cancellationToken)
                => CanNavigateFromAsync?.Invoke(toTarget, navigationContext, cancellationToken);

            Task<bool>? IHasNavigationCondition.CanNavigateToAsync(object? fromTarget, INavigationContext navigationContext, CancellationToken cancellationToken)
                => CanNavigateToAsync?.Invoke(fromTarget, navigationContext, cancellationToken);

            #endregion
        }

        private sealed class HasNavigatingCallback : IHasNavigatingCallback
        {
            #region Properties

            public Action<object?, INavigationContext>? OnNavigatingFrom { get; set; }

            public Action<object?, INavigationContext>? OnNavigatingTo { get; set; }

            #endregion

            #region Implementation of interfaces

            void IHasNavigatingCallback.OnNavigatingFrom(object? toTarget, INavigationContext navigationContext) => OnNavigatingFrom?.Invoke(toTarget, navigationContext);

            void IHasNavigatingCallback.OnNavigatingTo(object? fromTarget, INavigationContext navigationContext) => OnNavigatingTo?.Invoke(fromTarget, navigationContext);

            #endregion
        }

        private sealed class HasNavigatedCallback : IHasNavigatedCallback
        {
            #region Properties

            public Action<object?, INavigationContext>? OnNavigatedFrom { get; set; }

            public Action<object?, INavigationContext>? OnNavigatedTo { get; set; }

            #endregion

            #region Implementation of interfaces

            void IHasNavigatedCallback.OnNavigatedFrom(object? toTarget, INavigationContext navigationContext) => OnNavigatedFrom?.Invoke(toTarget, navigationContext);

            void IHasNavigatedCallback.OnNavigatedTo(object? fromTarget, INavigationContext navigationContext) => OnNavigatedTo?.Invoke(fromTarget, navigationContext);

            #endregion
        }

        #endregion
    }
}