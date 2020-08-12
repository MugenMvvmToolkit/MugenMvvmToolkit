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
    public class NavigationEntryManagerTest : UnitTestBase
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
            var provider = new TestNavigationProvider {Id = "t"};
            var dispatcher = new NavigationDispatcher();
            var component = new NavigationEntryManager();
            var contexts = new List<NavigationContext>();
            NavigationContext? ctx = null;
            var invokedCount = 0;

            for (var i = 0; i < count; i++)
            {
                var listener = new TestNavigationDispatcherEntryListener(dispatcher)
                {
                    OnNavigationEntryAdded = (entry, arg3) =>
                    {
                        ++invokedCount;
                        arg3.ShouldEqual(ctx);
                        entry.NavigationProvider.ShouldEqual(arg3!.NavigationProvider);
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
            {
                foreach (var navigationType in NavigationType.GetAll())
                {
                    for (var i = 0; i < count; i++)
                    {
                        ctx = new NavigationContext(this, provider, Guid.NewGuid().ToString(), navigationType, mode);
                        ctx.Metadata.Set(Key, i);
                        component.OnNavigating(dispatcher, ctx);
                        contexts.Add(ctx);
                    }
                }
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
            {
                foreach (var navigationType in NavigationType.GetAll())
                {
                    for (var i = 0; i < count; i++)
                    {
                        ctx = new NavigationContext(this, provider, Guid.NewGuid().ToString(), navigationType, mode);
                        ctx.Metadata.Set(Key, i);
                        component.OnNavigating(dispatcher, ctx);
                        contexts.Add(ctx);
                    }
                }
            }

            for (var i = 0; i < count; i++)
            {
                var listener = new TestNavigationDispatcherEntryListener(dispatcher)
                {
                    OnNavigationEntryAdded = (entry, arg3) => throw new NotSupportedException(),
                    OnNavigationEntryRemoved = (entry, arg3) => throw new NotSupportedException(),
                    OnNavigationEntryUpdated = (entry, arg3) =>
                    {
                        ++invokedCount;
                        arg3!.ShouldEqual(ctx);
                        entry.NavigationProvider.ShouldEqual(arg3!.NavigationProvider);
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
                component.OnNavigating(dispatcher, navigationContext);
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
                {
                    foreach (var navigationType in NavigationType.GetAll())
                    {
                        for (var i = 0; i < count; i++)
                        {
                            ctx = new NavigationContext(this, provider, Guid.NewGuid().ToString(), navigationType, mode);
                            ctx.Metadata.Set(Key, i);
                            component.OnNavigating(dispatcher, ctx);
                            contexts.Add(ctx);
                        }
                    }
                }

                for (var i = 0; i < count; i++)
                {
                    var listener = new TestNavigationDispatcherEntryListener(dispatcher)
                    {
                        OnNavigationEntryAdded = (entry, arg3) => throw new NotSupportedException(),
                        OnNavigationEntryRemoved = (entry, arg3) =>
                        {
                            ++invokedCount;
                            arg3.ShouldEqual(ctx);
                            entry.NavigationProvider.ShouldEqual(arg3!.NavigationProvider);
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
                    ctx = new NavigationContext(this, navigationContext.NavigationProvider, navigationContext.NavigationId, navigationContext.NavigationType, closeMode, navigationContext.Metadata);
                    component.OnNavigating(dispatcher, ctx);
                }

                invokedCount.ShouldEqual(contexts.Count * count);
                contexts.Clear();
                ValidateEntries(component.TryGetNavigationEntries(null!, null).AsList(), contexts);
            }
        }

        private static void ValidateEntries(IReadOnlyList<INavigationEntry>? entries, List<NavigationContext> contexts)
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
                var navigationContext = contexts.Single(context => context.NavigationId == entry.NavigationId);
                navigationContext.NavigationType.ShouldEqual(entry.NavigationType);
                navigationContext.NavigationProvider.ShouldEqual(entry.NavigationProvider);
                navigationContext.Metadata.Get(Key).ShouldEqual(entry.Metadata.Get(Key));
            }
        }

        #endregion
    }
}