using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Metadata;
using MugenMvvm.Navigation;
using MugenMvvm.Navigation.Components;
using MugenMvvm.UnitTest.Navigation.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Navigation.Components
{
    public class NavigationEntryProviderComponentTest : UnitTestBase
    {
        #region Fields

        private static readonly IMetadataContextKey<int, int> Key = MetadataContextKey.FromKey<int, int>("i");

        #endregion

        #region Methods

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void OnNavigatedShouldTrackEntriesAdd(int count)
        {
            var provider = new TestNavigationProvider { Id = "t" };
            var dispatcher = new NavigationDispatcher();
            var component = new NavigationEntryProviderComponent();
            var contexts = new List<NavigationContext>();
            NavigationContext? ctx = null;
            int invokedCount = 0;

            for (int i = 0; i < count; i++)
            {
                var listener = new TestNavigationDispatcherEntryListener
                {
                    OnNavigationEntryAdded = (navigationDispatcher, entry, arg3) =>
                    {
                        ++invokedCount;
                        navigationDispatcher.ShouldEqual(dispatcher);
                        arg3.ShouldEqual(ctx);
                        entry.NavigationProvider.ShouldEqual(arg3!.NavigationProvider);
                        entry.NavigationType.ShouldEqual(arg3!.NavigationType);
                        entry.NavigationOperationId.ShouldEqual(arg3!.NavigationOperationId);
                        entry.Metadata.Get(Key).ShouldEqual(entry.Metadata.Get(Key));
                    },
                    OnNavigationEntryRemoved = (navigationDispatcher, entry, arg3) => throw new NotSupportedException(),
                    OnNavigationEntryUpdated = (navigationDispatcher, entry, arg3) => throw new NotSupportedException()
                };
                dispatcher.AddComponent(listener);
            }

            foreach (var mode in NavigationMode.GetAll().Where(mode => mode.IsRefresh || mode.IsNew))
            {
                foreach (var navigationType in NavigationType.GetAll())
                {
                    for (var i = 0; i < count; i++)
                    {
                        ctx = new NavigationContext(provider, Guid.NewGuid().ToString(), navigationType, mode);
                        ctx.Metadata.Set(Key, i);
                        component.OnNavigated(dispatcher, ctx);
                        contexts.Add(ctx);
                    }
                }
            }

            ValidateEntries(component.TryGetNavigationEntries(null, null)!, contexts);

            foreach (var navigationType in NavigationType.GetAll())
                ValidateEntries(component.TryGetNavigationEntries(navigationType, DefaultMetadata)!, contexts.Where(context => context.NavigationType == navigationType).ToList());

            invokedCount.ShouldEqual(contexts.Count * count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void OnNavigatedShouldTrackEntriesUpdate(int count)
        {
            var provider = new TestNavigationProvider { Id = "t" };
            var dispatcher = new NavigationDispatcher();
            var component = new NavigationEntryProviderComponent();
            var contexts = new List<NavigationContext>();
            NavigationContext? ctx = null;
            int invokedCount = 0;

            foreach (var mode in NavigationMode.GetAll().Where(mode => mode.IsRefresh || mode.IsNew))
            {
                foreach (var navigationType in NavigationType.GetAll())
                {
                    for (var i = 0; i < count; i++)
                    {
                        ctx = new NavigationContext(provider, Guid.NewGuid().ToString(), navigationType, mode);
                        ctx.Metadata.Set(Key, i);
                        component.OnNavigated(dispatcher, ctx);
                        contexts.Add(ctx);
                    }
                }
            }

            for (int i = 0; i < count; i++)
            {
                var listener = new TestNavigationDispatcherEntryListener
                {
                    OnNavigationEntryAdded = (navigationDispatcher, entry, arg3) => throw new NotSupportedException(),
                    OnNavigationEntryRemoved = (navigationDispatcher, entry, arg3) => throw new NotSupportedException(),
                    OnNavigationEntryUpdated = (navigationDispatcher, entry, arg3) =>
                    {
                        ++invokedCount;
                        navigationDispatcher.ShouldEqual(dispatcher);
                        arg3!.ShouldEqual(ctx);
                        entry.NavigationProvider.ShouldEqual(arg3!.NavigationProvider);
                        entry.NavigationType.ShouldEqual(arg3!.NavigationType);
                        entry.NavigationOperationId.ShouldEqual(arg3!.NavigationOperationId);
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

            ValidateEntries(component.TryGetNavigationEntries(null, null)!, contexts);

            foreach (var navigationType in NavigationType.GetAll())
                ValidateEntries(component.TryGetNavigationEntries(navigationType, DefaultMetadata)!, contexts.Where(context => context.NavigationType == navigationType).ToList());

            invokedCount.ShouldEqual(contexts.Count * count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void OnNavigatedShouldTrackEntriesRemove(int count)
        {
            var provider = new TestNavigationProvider { Id = "t" };
            var dispatcher = new NavigationDispatcher();
            var component = new NavigationEntryProviderComponent();
            var contexts = new List<NavigationContext>();
            NavigationContext? ctx = null;

            foreach (var closeMode in NavigationMode.GetAll().Where(mode => mode.IsClose))
            {
                dispatcher.Components.Clear();
                int invokedCount = 0;
                foreach (var mode in NavigationMode.GetAll().Where(mode => mode.IsRefresh || mode.IsNew))
                {
                    foreach (var navigationType in NavigationType.GetAll())
                    {
                        for (var i = 0; i < count; i++)
                        {
                            ctx = new NavigationContext(provider, Guid.NewGuid().ToString(), navigationType, mode);
                            ctx.Metadata.Set(Key, i);
                            component.OnNavigated(dispatcher, ctx);
                            contexts.Add(ctx);
                        }
                    }
                }

                for (int i = 0; i < count; i++)
                {
                    var listener = new TestNavigationDispatcherEntryListener
                    {
                        OnNavigationEntryAdded = (navigationDispatcher, entry, arg3) => throw new NotSupportedException(),
                        OnNavigationEntryRemoved = (navigationDispatcher, entry, arg3) =>
                        {
                            ++invokedCount;
                            navigationDispatcher.ShouldEqual(dispatcher);
                            arg3.ShouldEqual(ctx);
                            entry.NavigationProvider.ShouldEqual(arg3!.NavigationProvider);
                            entry.NavigationType.ShouldEqual(arg3!.NavigationType);
                            entry.NavigationOperationId.ShouldEqual(arg3!.NavigationOperationId);
                            entry.Metadata.Get(Key).ShouldEqual(entry.Metadata.Get(Key));
                        },
                        OnNavigationEntryUpdated = (navigationDispatcher, entry, arg3) => throw new NotSupportedException(),
                    };
                    dispatcher.AddComponent(listener);
                }

                foreach (var navigationContext in contexts)
                {
                    ctx = new NavigationContext(navigationContext.NavigationProvider, navigationContext.NavigationOperationId, navigationContext.NavigationType, closeMode, navigationContext.Metadata);
                    component.OnNavigated(dispatcher, ctx);
                }

                invokedCount.ShouldEqual(contexts.Count * count);
                contexts.Clear();
                ValidateEntries(component.TryGetNavigationEntries(null, null)!, contexts);

                foreach (var navigationType in NavigationType.GetAll())
                    ValidateEntries(component.TryGetNavigationEntries(navigationType, DefaultMetadata)!, contexts.Where(context => context.NavigationType == navigationType).ToList());
            }
        }

        private static void ValidateEntries(IReadOnlyList<INavigationEntry> entries, List<NavigationContext> contexts)
        {
            entries.Count.ShouldEqual(contexts.Count);
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                var navigationContext = contexts.Single(context => context.NavigationOperationId == entry.NavigationOperationId);
                navigationContext.NavigationType.ShouldEqual(entry.NavigationType);
                navigationContext.NavigationProvider.ShouldEqual(entry.NavigationProvider);
                navigationContext.Metadata.Get(Key).ShouldEqual(entry.Metadata.Get(Key));
            }
        }

        #endregion
    }
}