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
using MugenMvvm.UnitTest.Components;
using MugenMvvm.UnitTest.Navigation.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Navigation
{
    public class NavigationDispatcherTest : ComponentOwnerTestBase<NavigationDispatcher>
    {
        #region Methods

        [Fact]
        public void GetNavigationContextShouldThrowNoComponents()
        {
            var navigationDispatcher = new NavigationDispatcher();
            ShouldThrow<InvalidOperationException>(() => navigationDispatcher.GetNavigationContext(new TestNavigationProvider(), "t", NavigationType.Alert, NavigationMode.Close));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetNavigationContextShouldBeHandledByComponents(int count)
        {
            var context = new NavigationContext(new TestNavigationProvider(), "t", NavigationType.Alert, NavigationMode.Close);
            var invokeCount = 0;
            var dispatcher = new NavigationDispatcher();
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                var component = new TestNavigationContextProviderComponent
                {
                    Priority = -i,
                    TryGetNavigationContext = (provider, s, arg3, arg4, arg5) =>
                    {
                        ++invokeCount;
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

            dispatcher.GetNavigationContext(context.NavigationProvider, context.NavigationId, context.NavigationType, context.NavigationMode, DefaultMetadata).ShouldEqual(context);
            invokeCount.ShouldEqual(count);
        }

        [Fact]
        public void GetNavigationEntriesShouldReturnEmptyListNoComponents()
        {
            new NavigationDispatcher().GetNavigationEntries(DefaultMetadata).AsList().ShouldBeEmpty();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetNavigationEntriesShouldBeHandledByComponents(int count)
        {
            var dispatcher = new NavigationDispatcher();
            var entries = new HashSet<INavigationEntry>();
            for (var i = 0; i < count; i++)
                entries.Add(new NavigationEntry(new TestNavigationProvider(), i.ToString(), NavigationType.Page));
            for (var i = 0; i < count; i++)
            {
                var info = entries.ElementAt(i);
                var component = new TestNavigationEntryProviderComponent
                {
                    Priority = -i,
                    TryGetNavigationEntries = (ctx) =>
                    {
                        ctx.ShouldEqual(DefaultMetadata);
                        return new[] { info };
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
        public void GetCallbacksShouldReturnEmptyListNoComponents()
        {
            new NavigationDispatcher().GetNavigationCallbacks(new NavigationEntry(new TestNavigationProvider(), "tes", NavigationType.Page)).AsList().ShouldBeEmpty();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetCallbacksShouldBeHandledByComponents(int count)
        {
            var navEntry = new NavigationEntry(new TestNavigationProvider(), "tes", NavigationType.Alert);
            var dispatcher = new NavigationDispatcher();
            var callbacks = new HashSet<INavigationCallback>();
            for (var i = 0; i < count; i++)
                callbacks.Add(new NavigationCallback(NavigationCallbackType.Close, i.ToString(), NavigationType.Alert));
            for (var i = 0; i < count; i++)
            {
                var info = callbacks.ElementAt(i);
                var component = new TestNavigationCallbackManagerComponent
                {
                    Priority = -i,
                    TryGetNavigationCallbacks = (entry, t, ctx) =>
                    {
                        entry.ShouldEqual(navEntry);
                        t.ShouldEqual(navEntry.GetType());
                        ctx.ShouldEqual(DefaultMetadata);
                        return new[] { info };
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
            var navigationContext = new NavigationContext(new TestNavigationProvider(), "t", NavigationType.Alert, NavigationMode.Close);
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
        public void OnNavigatingAsyncShouldBeHandledByComponents(int count, int state)
        {
            var cts = new CancellationTokenSource();
            var token = cts.Token;
            var navigationContext = new NavigationContext(new TestNavigationProvider(), "t", NavigationType.Alert, NavigationMode.Close);
            var dispatcher = new NavigationDispatcher();
            var callbacks = new List<TaskCompletionSource<bool>>();
            for (var i = 0; i < count; i++)
                callbacks.Add(new TaskCompletionSource<bool>());
            for (var i = 0; i < count; i++)
            {
                var source = callbacks.ElementAt(i);
                var component = new TestNavigationDispatcherNavigatingListener
                {
                    Priority = -i,
                    OnNavigatingAsync = (navigationDispatcher, context, arg3) =>
                    {
                        navigationDispatcher.ShouldEqual(dispatcher);
                        context.ShouldEqual(navigationContext);
                        arg3.ShouldEqual(token);
                        return source.Task;
                    }
                };
                dispatcher.AddComponent(component);
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
                result.IsCompleted.ShouldBeTrue();
                result.Result.ShouldEqual(state == 0);
                return;
            }

            if (state == 2 || state == 4)
            {
                if (state == 2)
                    callbacks.Last().TrySetCanceled(token);
                result.IsCompleted.ShouldBeTrue();
                result.IsCanceled.ShouldBeTrue();
                return;
            }

            var ex = new Exception();
            callbacks.Last().TrySetException(ex);
            result.IsCompleted.ShouldBeTrue();
            result.IsFaulted.ShouldBeTrue();
            result.Exception.InnerExceptions.Contains(ex).ShouldBeTrue();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void OnNavigatedShouldBeHandledByComponents(int count)
        {
            var invokeCount = 0;
            var navigationContext = new NavigationContext(new TestNavigationProvider(), "t", NavigationType.Alert, NavigationMode.Close);
            var dispatcher = new NavigationDispatcher();
            for (var i = 0; i < count; i++)
            {
                var component = new TestNavigationDispatcherNavigatedListener
                {
                    Priority = -i,
                    OnNavigated = (d, ctx) =>
                    {
                        ++invokeCount;
                        d.ShouldEqual(dispatcher);
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
            var navigationContext = new NavigationContext(new TestNavigationProvider(), "t", NavigationType.Alert, NavigationMode.Close);
            var dispatcher = new NavigationDispatcher();
            for (var i = 0; i < count; i++)
            {
                var component = new TestNavigationDispatcherErrorListener
                {
                    OnNavigationFailed = (d, ctx, e) =>
                    {
                        ++invokeCount;
                        d.ShouldEqual(dispatcher);
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
            var navigationContext = new NavigationContext(new TestNavigationProvider(), "t", NavigationType.Alert, NavigationMode.Close);
            var dispatcher = new NavigationDispatcher();
            for (var i = 0; i < count; i++)
            {
                var component = new TestNavigationDispatcherErrorListener
                {
                    OnNavigationCanceled = (d, ctx, token) =>
                    {
                        ++invokeCount;
                        d.ShouldEqual(dispatcher);
                        ctx.ShouldEqual(navigationContext);
                        token.ShouldEqual(cancellationToken);
                    }
                };
                dispatcher.AddComponent(component);
            }

            dispatcher.OnNavigationCanceled(navigationContext, cancellationToken);
            invokeCount.ShouldEqual(count);
        }

        protected override NavigationDispatcher GetComponentOwner(IComponentCollectionProvider? collectionProvider = null)
        {
            return new NavigationDispatcher(collectionProvider);
        }

        #endregion
    }
}