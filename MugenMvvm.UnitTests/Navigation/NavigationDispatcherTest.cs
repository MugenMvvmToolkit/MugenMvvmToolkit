using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Navigation;
using MugenMvvm.UnitTests.Components;
using MugenMvvm.UnitTests.Navigation.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Navigation
{
    public class NavigationDispatcherTest : ComponentOwnerTestBase<NavigationDispatcher>
    {
        #region Methods

        [Fact]
        public void GetNavigationContextShouldThrowNoComponents()
        {
            var navigationDispatcher = new NavigationDispatcher();
            ShouldThrow<InvalidOperationException>(() => navigationDispatcher.GetNavigationContext(this, new TestNavigationProvider(), "t", NavigationType.Alert, NavigationMode.Close));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetNavigationContextShouldBeHandledByComponents(int count)
        {
            var context = new NavigationContext(this, new TestNavigationProvider(), "t", NavigationType.Alert, NavigationMode.Close);
            var invokeCount = 0;
            var dispatcher = new NavigationDispatcher();
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                var component = new TestNavigationContextProviderComponent(dispatcher)
                {
                    Priority = -i,
                    TryGetNavigationContext = (t, provider, s, arg3, arg4, arg5) =>
                    {
                        ++invokeCount;
                        t.ShouldEqual(this);
                        provider.ShouldEqual(context.NavigationProvider);
                        s.ShouldEqual(context.NavigationId);
                        arg3.ShouldEqual(context.NavigationType);
                        arg4.ShouldEqual(context.NavigationMode);
                        arg5.ShouldEqual(DefaultMetadata);
                        if (isLast)
                            return context;
                        return null;
                    }
                };
                dispatcher.AddComponent(component);
            }

            dispatcher.GetNavigationContext(this, context.NavigationProvider, context.NavigationId, context.NavigationType, context.NavigationMode, DefaultMetadata).ShouldEqual(context);
            invokeCount.ShouldEqual(count);
        }

        [Fact]
        public void GetNavigationEntriesShouldReturnEmptyListNoComponents() => new NavigationDispatcher().GetNavigationEntries(DefaultMetadata).AsList().ShouldBeEmpty();

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetNavigationEntriesShouldBeHandledByComponents(int count)
        {
            var dispatcher = new NavigationDispatcher();
            var entries = new HashSet<INavigationEntry>();
            for (var i = 0; i < count; i++)
                entries.Add(new NavigationEntry(this, new TestNavigationProvider(), i.ToString(), NavigationType.Page));
            for (var i = 0; i < count; i++)
            {
                var info = entries.ElementAt(i);
                var component = new TestNavigationEntryProviderComponent(dispatcher)
                {
                    Priority = -i,
                    TryGetNavigationEntries = ctx =>
                    {
                        ctx.ShouldEqual(DefaultMetadata);
                        return new[] {info};
                    }
                };
                dispatcher.AddComponent(component);
            }

            var result = dispatcher.GetNavigationEntries(DefaultMetadata).AsList();
            result.Count.ShouldEqual(count);
            foreach (var navigationEntry in result)
                entries.Remove(navigationEntry);
            entries.Count.ShouldEqual(0);
        }

        [Fact]
        public void GetCallbacksShouldReturnEmptyListNoComponents() =>
            new NavigationDispatcher().GetNavigationCallbacks(new NavigationEntry(this, new TestNavigationProvider(), "tes", NavigationType.Page)).AsList().ShouldBeEmpty();

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetCallbacksShouldBeHandledByComponents(int count)
        {
            var navEntry = new NavigationEntry(this, new TestNavigationProvider(), "tes", NavigationType.Alert);
            var dispatcher = new NavigationDispatcher();
            var callbacks = new HashSet<INavigationCallback>();
            for (var i = 0; i < count; i++)
                callbacks.Add(new NavigationCallback(NavigationCallbackType.Close, i.ToString(), NavigationType.Alert));
            for (var i = 0; i < count; i++)
            {
                var info = callbacks.ElementAt(i);
                var component = new TestNavigationCallbackManagerComponent(dispatcher)
                {
                    Priority = -i,
                    TryGetNavigationCallbacks = (entry, ctx) =>
                    {
                        entry.ShouldEqual(navEntry);
                        ctx.ShouldEqual(DefaultMetadata);
                        return new[] {info};
                    }
                };
                dispatcher.AddComponent(component);
            }

            var result = dispatcher.GetNavigationCallbacks(navEntry, DefaultMetadata).AsList();
            result.Count.ShouldEqual(count);
            foreach (var callback in result)
                callbacks.Remove(callback);
            callbacks.Count.ShouldEqual(0);
        }

        [Fact]
        public void OnNavigatingAsyncShouldReturnTrueNoComponents()
        {
            var navigationContext = new NavigationContext(this, new TestNavigationProvider(), "t", NavigationType.Alert, NavigationMode.Close);
            var dispatcher = new NavigationDispatcher();
            dispatcher.OnNavigatingAsync(navigationContext).Result.ShouldBeTrue();
        }

        [Theory]
        [InlineData(1, 0)]
        [InlineData(1, 1)]
        [InlineData(1, 2)]
        [InlineData(1, 3)]
        [InlineData(10, 0)]
        [InlineData(10, 1)]
        [InlineData(10, 2)]
        [InlineData(10, 3)]
        [InlineData(10, 4)]
        public async Task OnNavigatingAsyncShouldBeHandledByComponents(int count, int state)//0 - true, 1 - false, 2 - canceled, 3 - exception, 4 - precanceled
        {
            var cts = new CancellationTokenSource();
            var token = cts.Token;
            var navigatingCount = 0;
            var navigationContext = new NavigationContext(this, new TestNavigationProvider(), "t", NavigationType.Alert, NavigationMode.Close);
            var dispatcher = new NavigationDispatcher();
            var callbacks = new List<TaskCompletionSource<bool>>();
            for (var i = 0; i < count; i++)
                callbacks.Add(new TaskCompletionSource<bool>());
            for (var i = 0; i < count; i++)
            {
                var source = callbacks[i];
                var component = new TestNavigationConditionComponent(dispatcher)
                {
                    Priority = -i,
                    CanNavigateAsync = (context, arg3) =>
                    {
                        context.ShouldEqual(navigationContext);
                        arg3.ShouldEqual(token);
                        return source.Task;
                    }
                };
                dispatcher.AddComponent(component);
                dispatcher.AddComponent(new TestNavigationListener(dispatcher)
                {
                    OnNavigating = context =>
                    {
                        ++navigatingCount;
                        context.ShouldEqual(navigationContext);
                    }
                });
            }

            var result = dispatcher.OnNavigatingAsync(navigationContext, token);
            result.IsCompleted.ShouldBeFalse();

            if (state == 4)
                cts.Cancel();
            for (var i = 0; i < count - 1; i++)
                callbacks[i].TrySetResult(true);

            if (state != 4)
                result.IsCompleted.ShouldBeFalse();
            if (state == 0 || state == 1)
            {
                callbacks.Last().TrySetResult(state == 0);
                await result;
                result.IsCompleted.ShouldBeTrue();
                result.Result.ShouldEqual(state == 0);
                navigatingCount.ShouldEqual(state == 0 ? count : 0);
                return;
            }

            if (state == 2 || state == 4)
            {
                if (state == 2)
                    callbacks.Last().TrySetCanceled(token);
                await result.WaitSafeAsync();
                result.IsCompleted.ShouldBeTrue();
                result.IsCanceled.ShouldBeTrue();
                navigatingCount.ShouldEqual(0);
                return;
            }

            var ex = new Exception();
            callbacks.Last().TrySetException(ex);
            await result.WaitSafeAsync();
            result.IsCompleted.ShouldBeTrue();
            result.IsFaulted.ShouldBeTrue();
            result.Exception!.InnerExceptions.Contains(ex).ShouldBeTrue();
            navigatingCount.ShouldEqual(0);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void OnNavigatingShouldBeHandledByComponents(int count)
        {
            var invokeCount = 0;
            var navigationContext = new NavigationContext(this, new TestNavigationProvider(), "t", NavigationType.Alert, NavigationMode.Close);
            var dispatcher = new NavigationDispatcher();
            for (var i = 0; i < count; i++)
            {
                var component = new TestNavigationListener(dispatcher)
                {
                    Priority = -i,
                    OnNavigating = ctx =>
                    {
                        ++invokeCount;
                        ctx.ShouldEqual(navigationContext);
                    }
                };
                dispatcher.AddComponent(component);
            }

            dispatcher.OnNavigating(navigationContext);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void OnNavigatedShouldBeHandledByComponents(int count)
        {
            var invokeCount = 0;
            var navigationContext = new NavigationContext(this, new TestNavigationProvider(), "t", NavigationType.Alert, NavigationMode.Close);
            var dispatcher = new NavigationDispatcher();
            for (var i = 0; i < count; i++)
            {
                var component = new TestNavigationListener(dispatcher)
                {
                    Priority = -i,
                    OnNavigated = ctx =>
                    {
                        ++invokeCount;
                        ctx.ShouldEqual(navigationContext);
                    }
                };
                dispatcher.AddComponent(component);
            }

            dispatcher.OnNavigated(navigationContext);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void OnNavigationFailedShouldBeHandledByComponents(int count)
        {
            var invokeCount = 0;
            var exception = new Exception();
            var navigationContext = new NavigationContext(this, new TestNavigationProvider(), "t", NavigationType.Alert, NavigationMode.Close);
            var dispatcher = new NavigationDispatcher();
            for (var i = 0; i < count; i++)
            {
                var component = new TestNavigationErrorListener(dispatcher)
                {
                    OnNavigationFailed = (ctx, e) =>
                    {
                        ++invokeCount;
                        ctx.ShouldEqual(navigationContext);
                        e.ShouldEqual(exception);
                    }
                };
                dispatcher.AddComponent(component);
            }

            dispatcher.OnNavigationFailed(navigationContext, exception);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void OnNavigationCanceledShouldBeHandledByComponents(int count)
        {
            var invokeCount = 0;
            var cancellationToken = new CancellationToken(true);
            var navigationContext = new NavigationContext(this, new TestNavigationProvider(), "t", NavigationType.Alert, NavigationMode.Close);
            var dispatcher = new NavigationDispatcher();
            for (var i = 0; i < count; i++)
            {
                var component = new TestNavigationErrorListener(dispatcher)
                {
                    OnNavigationCanceled = (ctx, token) =>
                    {
                        ++invokeCount;
                        ctx.ShouldEqual(navigationContext);
                        token.ShouldEqual(cancellationToken);
                    }
                };
                dispatcher.AddComponent(component);
            }

            dispatcher.OnNavigationCanceled(navigationContext, cancellationToken);
            invokeCount.ShouldEqual(count);
        }

        protected override NavigationDispatcher GetComponentOwner(IComponentCollectionManager? collectionProvider = null) => new NavigationDispatcher(collectionProvider);

        #endregion
    }
}