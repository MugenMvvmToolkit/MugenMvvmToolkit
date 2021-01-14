using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Metadata;
using MugenMvvm.Navigation;
using MugenMvvm.Navigation.Components;
using MugenMvvm.Presenters;
using MugenMvvm.UnitTests.Navigation.Internal;
using MugenMvvm.UnitTests.Presenters.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Navigation.Components
{
    public class NavigationEntryManagerTest : UnitTestBase
    {
        private static readonly IMetadataContextKey<int> Key = MetadataContextKey.FromKey<int>("i");

        [Fact]
        public void OnNavigatedShouldUpdatePendingEntry()
        {
            var provider = new TestNavigationProvider {Id = "t"};
            var dispatcher = new NavigationDispatcher();
            var component = new NavigationEntryManager(dispatcher);
            var presenter = new Presenter();
            presenter.AddComponent(component);
            presenter.AddComponent(new TestPresenterComponent
            {
                TryShow = (o, context, arg3) => ItemOrIReadOnlyList.FromItem((IPresenterResult) o)
            });

            var navigationContext = new NavigationContext(this, provider, "id1", NavigationType.Background, NavigationMode.New);
            var presenterResult = new PresenterResult(this, "id1", provider, NavigationType.Background);

            presenter.TryShow(presenterResult);
            var entries = component.TryGetNavigationEntries(dispatcher, DefaultMetadata);
            entries.AsList().Single(entry => entry.NavigationId == navigationContext.NavigationId).IsPending.ShouldBeTrue();

            component.OnNavigated(dispatcher, navigationContext);
            entries = component.TryGetNavigationEntries(dispatcher, DefaultMetadata);
            entries.AsList().Single(entry => entry.NavigationId == navigationContext.NavigationId).IsPending.ShouldBeFalse();
        }

        [Fact]
        public void OnNavigationCanceledShouldRemoveOnlyPendingEntries()
        {
            var provider = new TestNavigationProvider {Id = "t"};
            var dispatcher = new NavigationDispatcher();
            var component = new NavigationEntryManager(dispatcher);
            var presenter = new Presenter();
            presenter.AddComponent(component);
            presenter.AddComponent(new TestPresenterComponent
            {
                TryShow = (o, context, arg3) => ItemOrIReadOnlyList.FromItem((IPresenterResult) o)
            });

            var navigationContext = new NavigationContext(this, provider, "id1", NavigationType.Background, NavigationMode.New);
            var presenterResult = new PresenterResult(this, "id2", provider, NavigationType.Background);
            component.OnNavigated(dispatcher, navigationContext);
            presenter.TryShow(presenterResult);

            var entries = component.TryGetNavigationEntries(dispatcher, DefaultMetadata);
            entries.AsList().Single(entry => entry.NavigationId == navigationContext.NavigationId).ShouldNotBeNull();
            entries.AsList().Single(entry => entry.NavigationId == presenterResult.NavigationId).ShouldNotBeNull();

            component.OnNavigationCanceled(dispatcher, navigationContext, default);
            component.OnNavigationCanceled(dispatcher, new NavigationContext(this, provider, "id2", NavigationType.Background, NavigationMode.New), default);

            entries = component.TryGetNavigationEntries(dispatcher, DefaultMetadata);
            entries.AsList().Single(entry => entry.NavigationId == navigationContext.NavigationId).ShouldNotBeNull();
            entries.AsList().SingleOrDefault(entry => entry.NavigationId == presenterResult.NavigationId).ShouldBeNull();
        }

        [Fact]
        public void OnNavigationFailedShouldRemoveOnlyPendingEntries()
        {
            var provider = new TestNavigationProvider {Id = "t"};
            var dispatcher = new NavigationDispatcher();
            var component = new NavigationEntryManager(dispatcher);
            var presenter = new Presenter();
            presenter.AddComponent(component);
            presenter.AddComponent(new TestPresenterComponent
            {
                TryShow = (o, context, arg3) => ItemOrIReadOnlyList.FromItem((IPresenterResult) o)
            });

            var navigationContext = new NavigationContext(this, provider, "id1", NavigationType.Background, NavigationMode.New);
            var presenterResult = new PresenterResult(this, "id2", provider, NavigationType.Background);
            component.OnNavigated(dispatcher, navigationContext);
            presenter.TryShow(presenterResult);

            var entries = component.TryGetNavigationEntries(dispatcher, DefaultMetadata);
            entries.AsList().Single(entry => entry.NavigationId == navigationContext.NavigationId).ShouldNotBeNull();
            entries.AsList().Single(entry => entry.NavigationId == presenterResult.NavigationId).ShouldNotBeNull();

            component.OnNavigationFailed(dispatcher, navigationContext, new Exception());
            component.OnNavigationFailed(dispatcher, new NavigationContext(this, provider, "id2", NavigationType.Background, NavigationMode.New), new Exception());

            entries = component.TryGetNavigationEntries(dispatcher, DefaultMetadata);
            entries.AsList().Single(entry => entry.NavigationId == navigationContext.NavigationId).ShouldNotBeNull();
            entries.AsList().SingleOrDefault(entry => entry.NavigationId == presenterResult.NavigationId).ShouldBeNull();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void OnNavigatingShouldTrackEntriesAdd(int count)
        {
            var provider = new TestNavigationProvider {Id = "t"};
            var dispatcher = new NavigationDispatcher();
            var component = new NavigationEntryManager();
            var contexts = new List<NavigationContext>();
            NavigationContext? ctx = null;
            var invokedCount = 0;

            for (var i = 0; i < count; i++)
            {
                var listener = new TestNavigationEntryListener(dispatcher)
                {
                    OnNavigationEntryAdded = (entry, arg3) =>
                    {
                        ++invokedCount;
                        arg3.ShouldEqual(ctx);
                        entry.IsPending.ShouldBeTrue();
                        entry.NavigationProvider.ShouldEqual(((INavigationContext) arg3!).NavigationProvider);
                        entry.NavigationType.ShouldEqual(arg3!.NavigationType);
                        entry.NavigationId.ShouldEqual(arg3!.NavigationId);
                        entry.Metadata.Get(Key).ShouldEqual(entry.Metadata.Get(Key));
                    },
                    OnNavigationEntryRemoved = (entry, arg3) => throw new NotSupportedException(),
                    OnNavigationEntryUpdated = (entry, arg3) => throw new NotSupportedException()
                };
                dispatcher.AddComponent(listener);
            }

            foreach (var mode in NavigationMode.GetAll().Where(mode => mode.IsRefresh || mode.IsNew))
            foreach (var navigationType in NavigationType.GetAll())
                for (var i = 0; i < count; i++)
                {
                    ctx = new NavigationContext(this, provider, Guid.NewGuid().ToString(), navigationType, mode);
                    ctx.Metadata.Set(Key, i);
                    component.OnNavigating(dispatcher, ctx);
                    contexts.Add(ctx);
                }

            ValidateEntries(component.TryGetNavigationEntries(null!, null).AsList(), contexts, true);
            invokedCount.ShouldEqual(contexts.Count * count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void OnNavigatedShouldTrackEntriesAdd(int count)
        {
            var provider = new TestNavigationProvider {Id = "t"};
            var dispatcher = new NavigationDispatcher();
            var component = new NavigationEntryManager();
            var contexts = new List<NavigationContext>();
            NavigationContext? ctx = null;
            var invokedCount = 0;

            for (var i = 0; i < count; i++)
            {
                var listener = new TestNavigationEntryListener(dispatcher)
                {
                    OnNavigationEntryAdded = (entry, arg3) =>
                    {
                        ++invokedCount;
                        arg3.ShouldEqual(ctx);
                        entry.IsPending.ShouldBeFalse();
                        entry.NavigationProvider.ShouldEqual(((INavigationContext) arg3!).NavigationProvider);
                        entry.NavigationType.ShouldEqual(arg3!.NavigationType);
                        entry.NavigationId.ShouldEqual(arg3!.NavigationId);
                        entry.Metadata.Get(Key).ShouldEqual(entry.Metadata.Get(Key));
                    },
                    OnNavigationEntryRemoved = (entry, arg3) => throw new NotSupportedException(),
                    OnNavigationEntryUpdated = (entry, arg3) => throw new NotSupportedException()
                };
                dispatcher.AddComponent(listener);
            }

            foreach (var mode in NavigationMode.GetAll().Where(mode => mode.IsRefresh || mode.IsNew))
            foreach (var navigationType in NavigationType.GetAll())
                for (var i = 0; i < count; i++)
                {
                    ctx = new NavigationContext(this, provider, Guid.NewGuid().ToString(), navigationType, mode);
                    ctx.Metadata.Set(Key, i);
                    component.OnNavigated(dispatcher, ctx);
                    contexts.Add(ctx);
                }

            ValidateEntries(component.TryGetNavigationEntries(null!, null).AsList(), contexts);
            invokedCount.ShouldEqual(contexts.Count * count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void OnNavigatedShouldTrackEntriesUpdate(int count)
        {
            var provider = new TestNavigationProvider {Id = "t"};
            var dispatcher = new NavigationDispatcher();
            var component = new NavigationEntryManager();
            var contexts = new List<NavigationContext>();
            NavigationContext? ctx = null;
            var invokedCount = 0;

            foreach (var mode in NavigationMode.GetAll().Where(mode => mode.IsRefresh || mode.IsNew))
            foreach (var navigationType in NavigationType.GetAll())
                for (var i = 0; i < count; i++)
                {
                    ctx = new NavigationContext(this, provider, Guid.NewGuid().ToString(), navigationType, mode);
                    ctx.Metadata.Set(Key, i);
                    component.OnNavigated(dispatcher, ctx);
                    contexts.Add(ctx);
                }

            for (var i = 0; i < count; i++)
            {
                var listener = new TestNavigationEntryListener(dispatcher)
                {
                    OnNavigationEntryAdded = (entry, arg3) => throw new NotSupportedException(),
                    OnNavigationEntryRemoved = (entry, arg3) => throw new NotSupportedException(),
                    OnNavigationEntryUpdated = (entry, arg3) =>
                    {
                        ++invokedCount;
                        arg3!.ShouldEqual(ctx);
                        entry.IsPending.ShouldBeFalse();
                        entry.NavigationProvider.ShouldEqual(((INavigationContext) arg3!).NavigationProvider);
                        entry.NavigationType.ShouldEqual(arg3!.NavigationType);
                        entry.NavigationId.ShouldEqual(arg3!.NavigationId);
                        entry.Metadata.Get(Key).ShouldEqual(entry.Metadata.Get(Key));
                    }
                };
                dispatcher.AddComponent(listener);
            }

            foreach (var navigationContext in contexts)
            {
                ctx = navigationContext;
                component.OnNavigated(dispatcher, navigationContext);
            }

            ValidateEntries(component.TryGetNavigationEntries(null!, null).AsList(), contexts);
            invokedCount.ShouldEqual(contexts.Count * count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void OnNavigatedShouldTrackEntriesRemove(int count)
        {
            var provider = new TestNavigationProvider {Id = "t"};
            var dispatcher = new NavigationDispatcher();
            var component = new NavigationEntryManager();
            var contexts = new List<NavigationContext>();
            NavigationContext? ctx = null;

            foreach (var closeMode in NavigationMode.GetAll().Where(mode => mode.IsClose))
            {
                dispatcher.Components.Clear();
                var invokedCount = 0;
                foreach (var mode in NavigationMode.GetAll().Where(mode => mode.IsRefresh || mode.IsNew))
                foreach (var navigationType in NavigationType.GetAll())
                    for (var i = 0; i < count; i++)
                    {
                        ctx = new NavigationContext(this, provider, Guid.NewGuid().ToString(), navigationType, mode);
                        ctx.Metadata.Set(Key, i);
                        component.OnNavigated(dispatcher, ctx);
                        contexts.Add(ctx);
                    }

                for (var i = 0; i < count; i++)
                {
                    var listener = new TestNavigationEntryListener(dispatcher)
                    {
                        OnNavigationEntryAdded = (entry, arg3) => throw new NotSupportedException(),
                        OnNavigationEntryRemoved = (entry, arg3) =>
                        {
                            ++invokedCount;
                            arg3.ShouldEqual(ctx);
                            entry.IsPending.ShouldBeFalse();
                            entry.NavigationProvider.ShouldEqual(((INavigationContext) arg3!).NavigationProvider);
                            entry.NavigationType.ShouldEqual(arg3!.NavigationType);
                            entry.NavigationId.ShouldEqual(arg3!.NavigationId);
                            entry.Metadata.Get(Key).ShouldEqual(entry.Metadata.Get(Key));
                        },
                        OnNavigationEntryUpdated = (entry, arg3) => throw new NotSupportedException()
                    };
                    dispatcher.AddComponent(listener);
                }

                foreach (var navigationContext in contexts)
                {
                    ctx = new NavigationContext(this, navigationContext.NavigationProvider, navigationContext.NavigationId, navigationContext.NavigationType, closeMode,
                        navigationContext.Metadata);
                    component.OnNavigated(dispatcher, ctx);
                }

                invokedCount.ShouldEqual(contexts.Count * count);
                contexts.Clear();
                ValidateEntries(component.TryGetNavigationEntries(null!, null).AsList(), contexts);
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryShowShouldTrackEntriesAdd(int count)
        {
            var provider = new TestNavigationProvider {Id = "t"};
            var dispatcher = new NavigationDispatcher();
            var component = new NavigationEntryManager(dispatcher);
            var presenter = new Presenter();
            presenter.AddComponent(component);
            presenter.AddComponent(new TestPresenterComponent
            {
                TryShow = (o, context, arg3) => ItemOrIReadOnlyList.FromItem((IPresenterResult) o)
            });

            var contexts = new List<IHasNavigationInfo>();
            IPresenterResult? ctx = null;
            var invokedCount = 0;

            for (var i = 0; i < count; i++)
            {
                var listener = new TestNavigationEntryListener(dispatcher)
                {
                    OnNavigationEntryAdded = (entry, arg3) =>
                    {
                        ++invokedCount;
                        arg3.ShouldEqual(ctx);
                        entry.IsPending.ShouldBeTrue();
                        entry.NavigationProvider.ShouldEqual(((IHasNavigationProvider) arg3!).NavigationProvider);
                        entry.NavigationType.ShouldEqual(arg3!.NavigationType);
                        entry.NavigationId.ShouldEqual(arg3!.NavigationId);
                        entry.Metadata.Get(Key).ShouldEqual(entry.Metadata.Get(Key));
                    },
                    OnNavigationEntryRemoved = (entry, arg3) => throw new NotSupportedException(),
                    OnNavigationEntryUpdated = (entry, arg3) => throw new NotSupportedException()
                };
                dispatcher.AddComponent(listener);
            }

            foreach (var navigationType in NavigationType.GetAll())
                for (var i = 0; i < count; i++)
                {
                    ctx = new PresenterResult(this, Guid.NewGuid().ToString(), provider, navigationType);
                    ctx.Metadata.Set(Key, i);
                    presenter.TryShow(ctx);
                    contexts.Add(ctx);
                }

            ValidateEntries(component.TryGetNavigationEntries(dispatcher, null).AsList(), contexts, true);
            invokedCount.ShouldEqual(contexts.Count * count);
        }

        private static void ValidateEntries(IReadOnlyList<INavigationEntry>? entries, IReadOnlyCollection<IHasNavigationInfo> contexts, bool isPending = false)
        {
            if (entries == null)
            {
                contexts.Count.ShouldEqual(0);
                return;
            }

            entries.Count.ShouldEqual(contexts.Count);
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                entry.IsPending.ShouldEqual(isPending);
                var navigationContext = contexts.Single(context => context.NavigationId == entry.NavigationId);
                navigationContext.NavigationType.ShouldEqual(entry.NavigationType);
                ((IHasNavigationProvider) navigationContext).NavigationProvider.ShouldEqual(entry.NavigationProvider);
                ((IMetadataOwner<IReadOnlyMetadataContext>) navigationContext).Metadata.Get(Key).ShouldEqual(entry.Metadata.Get(Key));
            }
        }
    }
}