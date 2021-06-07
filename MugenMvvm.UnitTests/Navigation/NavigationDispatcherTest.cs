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
using MugenMvvm.Tests.Navigation;
using MugenMvvm.UnitTests.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Navigation
{
    public class NavigationDispatcherTest : ComponentOwnerTestBase<NavigationDispatcher>
    {
        [Fact]
        public void GetCallbacksShouldReturnEmptyListNoComponents() =>
            NavigationDispatcher.GetNavigationCallbacks(new NavigationEntry(this, TestNavigationProvider.Instance, "tes", NavigationType.Page)).AsList().ShouldBeEmpty();

        [Fact]
        public void GetNavigationContextShouldThrowNoComponents() =>
            ShouldThrow<InvalidOperationException>(() =>
                NavigationDispatcher.GetNavigationContext(this, TestNavigationProvider.Instance, "t", NavigationType.Alert, NavigationMode.Close));

        [Fact]
        public void GetNavigationEntriesShouldReturnEmptyListNoComponents() =>
            NavigationDispatcher.GetNavigationEntries(DefaultMetadata).AsList().ShouldBeEmpty();

        [Fact]
        public async Task OnNavigatingAsyncShouldReturnTrueNoComponents()
        {
            var navigationContext = GetNavigationContext(this);
            (await NavigationDispatcher.OnNavigatingAsync(navigationContext)).ShouldBeTrue();
        }

        protected override INavigationDispatcher GetNavigationDispatcher() => GetComponentOwner(ComponentCollectionManager);

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetNavigationContextShouldBeHandledByComponents(int count)
        {
            var context = GetNavigationContext(this);
            var invokeCount = 0;
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                NavigationDispatcher.AddComponent(new TestNavigationContextProviderComponent
                {
                    Priority = -i,
                    TryGetNavigationContext = (d, t, provider, s, arg3, arg4, arg5) =>
                    {
                        ++invokeCount;
                        d.ShouldEqual(NavigationDispatcher);
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
                });
            }

            NavigationDispatcher.GetNavigationContext(this, context.NavigationProvider, context.NavigationId, context.NavigationType, context.NavigationMode, DefaultMetadata)
                                .ShouldEqual(context);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetNavigationEntriesShouldBeHandledByComponents(int count)
        {
            var entries = new HashSet<INavigationEntry>();
            for (var i = 0; i < count; i++)
                entries.Add(new NavigationEntry(this, TestNavigationProvider.Instance, i.ToString(), NavigationType.Page));
            for (var i = 0; i < count; i++)
            {
                var info = entries.ElementAt(i);
                NavigationDispatcher.AddComponent(new TestNavigationEntryProviderComponent
                {
                    Priority = -i,
                    TryGetNavigationEntries = (d, ctx) =>
                    {
                        d.ShouldEqual(NavigationDispatcher);
                        ctx.ShouldEqual(DefaultMetadata);
                        return new[] { info };
                    }
                });
            }

            var result = NavigationDispatcher.GetNavigationEntries(DefaultMetadata).AsList();
            result.Count.ShouldEqual(count);
            foreach (var navigationEntry in result)
                entries.Remove(navigationEntry);
            entries.Count.ShouldEqual(0);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetCallbacksShouldBeHandledByComponents(int count)
        {
            var navEntry = new NavigationEntry(this, TestNavigationProvider.Instance, "tes", NavigationType.Alert);
            var callbacks = new HashSet<INavigationCallback>();
            for (var i = 0; i < count; i++)
                callbacks.Add(new NavigationCallback(NavigationCallbackType.Close, i.ToString(), NavigationType.Alert));
            for (var i = 0; i < count; i++)
            {
                var info = callbacks.ElementAt(i);
                NavigationDispatcher.AddComponent(new TestNavigationCallbackManagerComponent
                {
                    Priority = -i,
                    TryGetNavigationCallbacks = (d, entry, ctx) =>
                    {
                        d.ShouldEqual(NavigationDispatcher);
                        entry.ShouldEqual(navEntry);
                        ctx.ShouldEqual(DefaultMetadata);
                        return new[] { info };
                    }
                });
            }

            var result = NavigationDispatcher.GetNavigationCallbacks(navEntry, DefaultMetadata).AsList();
            result.Count.ShouldEqual(count);
            foreach (var callback in result)
                callbacks.Remove(callback);
            callbacks.Count.ShouldEqual(0);
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
        public async Task OnNavigatingAsyncShouldBeHandledByComponents(int count, int state) //0 - true, 1 - false, 2 - canceled, 3 - exception, 4 - precanceled
        {
            var cts = new CancellationTokenSource();
            var token = cts.Token;
            var navigatingCount = 0;
            var navigationContext = GetNavigationContext(this);
            var callbacks = new List<TaskCompletionSource<bool>>();
            for (var i = 0; i < count; i++)
                callbacks.Add(new TaskCompletionSource<bool>());
            for (var i = 0; i < count; i++)
            {
                var source = callbacks[i];
                NavigationDispatcher.AddComponent(new TestNavigationConditionComponent
                {
                    Priority = -i,
                    CanNavigateAsync = (d, context, arg3) =>
                    {
                        d.ShouldEqual(NavigationDispatcher);
                        context.ShouldEqual(navigationContext);
                        arg3.ShouldEqual(token);
                        return source.Task.AsValueTask();
                    }
                });
                NavigationDispatcher.AddComponent(new TestNavigationListener
                {
                    OnNavigating = (d, context) =>
                    {
                        ++navigatingCount;
                        d.ShouldEqual(NavigationDispatcher);
                        context.ShouldEqual(navigationContext);
                    }
                });
            }

            var result = NavigationDispatcher.OnNavigatingAsync(navigationContext, token);
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
            result.AsTask().Exception!.InnerExceptions.Contains(ex).ShouldBeTrue();
            navigatingCount.ShouldEqual(0);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void OnNavigatingShouldBeHandledByComponents(int count)
        {
            var invokeCount = 0;
            var navigationContext = GetNavigationContext(this);
            for (var i = 0; i < count; i++)
            {
                NavigationDispatcher.AddComponent(new TestNavigationListener
                {
                    Priority = -i,
                    OnNavigating = (d, ctx) =>
                    {
                        ++invokeCount;
                        d.ShouldEqual(NavigationDispatcher);
                        ctx.ShouldEqual(navigationContext);
                    }
                });
            }

            NavigationDispatcher.OnNavigating(navigationContext);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void OnNavigatedShouldBeHandledByComponents(int count)
        {
            var invokeCount = 0;
            var navigationContext = GetNavigationContext(this);
            for (var i = 0; i < count; i++)
            {
                NavigationDispatcher.AddComponent(new TestNavigationListener
                {
                    Priority = -i,
                    OnNavigated = (d, ctx) =>
                    {
                        ++invokeCount;
                        d.ShouldEqual(NavigationDispatcher);
                        ctx.ShouldEqual(navigationContext);
                    }
                });
            }

            NavigationDispatcher.OnNavigated(navigationContext);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void OnNavigationFailedShouldBeHandledByComponents(int count)
        {
            var invokeCount = 0;
            var exception = new Exception();
            var navigationContext = GetNavigationContext(this);
            for (var i = 0; i < count; i++)
            {
                NavigationDispatcher.AddComponent(new TestNavigationErrorListener
                {
                    OnNavigationFailed = (d, ctx, e) =>
                    {
                        ++invokeCount;
                        d.ShouldEqual(NavigationDispatcher);
                        ctx.ShouldEqual(navigationContext);
                        e.ShouldEqual(exception);
                    }
                });
            }

            NavigationDispatcher.OnNavigationFailed(navigationContext, exception);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void OnNavigationCanceledShouldBeHandledByComponents(int count)
        {
            var invokeCount = 0;
            var navigationContext = GetNavigationContext(this);
            for (var i = 0; i < count; i++)
            {
                var component = new TestNavigationErrorListener
                {
                    OnNavigationCanceled = (d, ctx, token) =>
                    {
                        ++invokeCount;
                        d.ShouldEqual(NavigationDispatcher);
                        ctx.ShouldEqual(navigationContext);
                        token.ShouldEqual(DefaultCancellationToken);
                    }
                };
                NavigationDispatcher.AddComponent(component);
            }

            NavigationDispatcher.OnNavigationCanceled(navigationContext, DefaultCancellationToken);
            invokeCount.ShouldEqual(count);
        }

        protected override NavigationDispatcher GetComponentOwner(IComponentCollectionManager? componentCollectionManager = null) => new(componentCollectionManager);
    }
}