using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presentation;
using MugenMvvm.Metadata;
using MugenMvvm.Navigation;
using MugenMvvm.Navigation.Components;
using MugenMvvm.Presentation;
using MugenMvvm.Tests.Navigation;
using MugenMvvm.Tests.Presentation;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Navigation.Components
{
    public class NavigationEntryManagerTest : UnitTestBase
    {
        private static readonly IMetadataContextKey<int> Key = MetadataContextKey.FromKey<int>("i");
        private readonly NavigationEntryManager _entryManager;

        public NavigationEntryManagerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _entryManager = new NavigationEntryManager();
            NavigationDispatcher.AddComponent(_entryManager);
            Presenter.AddComponent(_entryManager);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void OnNavigatedShouldTrackEntriesAdd(int count)
        {
            var contexts = new List<NavigationContext>();
            NavigationContext? ctx = null;
            var invokedCount = 0;

            for (var i = 0; i < count; i++)
            {
                NavigationDispatcher.AddComponent(new TestNavigationEntryListener
                {
                    OnNavigationEntryAdded = (m, entry, arg3) =>
                    {
                        ++invokedCount;
                        m.ShouldEqual(NavigationDispatcher);
                        arg3.ShouldEqual(ctx);
                        entry.IsPending.ShouldBeFalse();
                        entry.NavigationProvider.ShouldEqual(((INavigationContext)arg3!).NavigationProvider);
                        entry.NavigationType.ShouldEqual(arg3!.NavigationType);
                        entry.NavigationId.ShouldEqual(arg3!.NavigationId);
                        entry.Metadata.Get(Key).ShouldEqual(entry.Metadata.Get(Key));
                    },
                    OnNavigationEntryRemoved = (_, _, _) => throw new NotSupportedException(),
                    OnNavigationEntryUpdated = (_, _, _) => throw new NotSupportedException()
                });
            }

            foreach (var mode in NavigationMode.GetAll().Where(mode => mode.IsRefresh || mode.IsNew))
            foreach (var navigationType in NavigationType.GetAll())
            {
                for (var i = 0; i < count; i++)
                {
                    ctx = new NavigationContext(this, TestNavigationProvider.Instance, Guid.NewGuid().ToString(), navigationType, mode);
                    ctx.Metadata.Set(Key, i);
                    NavigationDispatcher.OnNavigated(ctx);
                    contexts.Add(ctx);
                }
            }

            ValidateEntries(NavigationDispatcher.GetNavigationEntries(), contexts);
            invokedCount.ShouldEqual(contexts.Count * count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void OnNavigatedShouldTrackEntriesRemove(int count)
        {
            var contexts = new List<NavigationContext>();
            NavigationContext? ctx = null;

            foreach (var closeMode in NavigationMode.GetAll().Where(mode => mode.IsClose))
            {
                NavigationDispatcher.Components.Clear();
                NavigationDispatcher.AddComponent(_entryManager);
                var invokedCount = 0;
                foreach (var mode in NavigationMode.GetAll().Where(mode => mode.IsRefresh || mode.IsNew))
                foreach (var navigationType in NavigationType.GetAll())
                {
                    for (var i = 0; i < count; i++)
                    {
                        ctx = new NavigationContext(this, TestNavigationProvider.Instance, Guid.NewGuid().ToString(), navigationType, mode);
                        ctx.Metadata.Set(Key, i);
                        NavigationDispatcher.OnNavigated(ctx);
                        contexts.Add(ctx);
                    }
                }

                for (var i = 0; i < count; i++)
                {
                    var listener = new TestNavigationEntryListener
                    {
                        OnNavigationEntryAdded = (_, _, _) => throw new NotSupportedException(),
                        OnNavigationEntryRemoved = (_, entry, arg3) =>
                        {
                            ++invokedCount;
                            arg3.ShouldEqual(ctx);
                            entry.IsPending.ShouldBeFalse();
                            entry.NavigationProvider.ShouldEqual(((INavigationContext)arg3!).NavigationProvider);
                            entry.NavigationType.ShouldEqual(arg3!.NavigationType);
                            entry.NavigationId.ShouldEqual(arg3!.NavigationId);
                            entry.Metadata.Get(Key).ShouldEqual(entry.Metadata.Get(Key));
                        },
                        OnNavigationEntryUpdated = (_, _, _) => throw new NotSupportedException()
                    };
                    NavigationDispatcher.AddComponent(listener);
                }

                foreach (var navigationContext in contexts)
                {
                    ctx = new NavigationContext(this, navigationContext.NavigationProvider, navigationContext.NavigationId, navigationContext.NavigationType, closeMode,
                        navigationContext.Metadata);
                    NavigationDispatcher.OnNavigated(ctx);
                }

                invokedCount.ShouldEqual(contexts.Count * count);
                contexts.Clear();
                ValidateEntries(NavigationDispatcher.GetNavigationEntries(), contexts);
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void OnNavigatedShouldTrackEntriesUpdate(int count)
        {
            var contexts = new List<NavigationContext>();
            NavigationContext? ctx = null;
            var invokedCount = 0;

            foreach (var mode in NavigationMode.GetAll().Where(mode => mode.IsRefresh || mode.IsNew))
            foreach (var navigationType in NavigationType.GetAll())
            {
                for (var i = 0; i < count; i++)
                {
                    ctx = new NavigationContext(this, TestNavigationProvider.Instance, Guid.NewGuid().ToString(), navigationType, mode);
                    ctx.Metadata.Set(Key, i);
                    NavigationDispatcher.OnNavigated(ctx);
                    contexts.Add(ctx);
                }
            }

            for (var i = 0; i < count; i++)
            {
                NavigationDispatcher.AddComponent(new TestNavigationEntryListener
                {
                    OnNavigationEntryAdded = (_, _, _) => throw new NotSupportedException(),
                    OnNavigationEntryRemoved = (_, _, _) => throw new NotSupportedException(),
                    OnNavigationEntryUpdated = (_, entry, arg3) =>
                    {
                        ++invokedCount;
                        arg3!.ShouldEqual(ctx);
                        entry.IsPending.ShouldBeFalse();
                        entry.NavigationProvider.ShouldEqual(((INavigationContext)arg3!).NavigationProvider);
                        entry.NavigationType.ShouldEqual(arg3!.NavigationType);
                        entry.NavigationId.ShouldEqual(arg3!.NavigationId);
                        entry.Metadata.Get(Key).ShouldEqual(entry.Metadata.Get(Key));
                    }
                });
            }

            foreach (var navigationContext in contexts)
            {
                ctx = navigationContext;
                NavigationDispatcher.OnNavigated(navigationContext);
            }

            ValidateEntries(NavigationDispatcher.GetNavigationEntries(), contexts);
            invokedCount.ShouldEqual(contexts.Count * count);
        }

        [Fact]
        public void OnNavigatedShouldUpdatePendingEntry()
        {
            Presenter.AddComponent(new TestPresenterComponent
            {
                TryShow = (_, o, _, _) => ItemOrIReadOnlyList.FromItem((IPresenterResult)o)
            });

            var navigationContext = new NavigationContext(this, TestNavigationProvider.Instance, "id1", NavigationType.Background, NavigationMode.New);
            var presenterResult = new PresenterResult(this, "id1", TestNavigationProvider.Instance, NavigationType.Background);

            Presenter.TryShow(presenterResult);
            var entries = NavigationDispatcher.GetNavigationEntries(DefaultMetadata);
            entries.Single(entry => entry.NavigationId == navigationContext.NavigationId).IsPending.ShouldBeTrue();

            NavigationDispatcher.OnNavigated(navigationContext);
            entries = NavigationDispatcher.GetNavigationEntries(DefaultMetadata);
            entries.Single(entry => entry.NavigationId == navigationContext.NavigationId).IsPending.ShouldBeFalse();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void OnNavigatingShouldTrackEntriesAdd(int count)
        {
            var contexts = new List<NavigationContext>();
            NavigationContext? ctx = null;
            var invokedCount = 0;

            for (var i = 0; i < count; i++)
            {
                NavigationDispatcher.AddComponent(new TestNavigationEntryListener
                {
                    OnNavigationEntryAdded = (m, entry, arg3) =>
                    {
                        ++invokedCount;
                        m.ShouldEqual(NavigationDispatcher);
                        arg3.ShouldEqual(ctx);
                        entry.IsPending.ShouldBeTrue();
                        entry.NavigationProvider.ShouldEqual(((INavigationContext)arg3!).NavigationProvider);
                        entry.NavigationType.ShouldEqual(arg3!.NavigationType);
                        entry.NavigationId.ShouldEqual(arg3!.NavigationId);
                        entry.Metadata.Get(Key).ShouldEqual(entry.Metadata.Get(Key));
                    },
                    OnNavigationEntryRemoved = (_, _, _) => throw new NotSupportedException(),
                    OnNavigationEntryUpdated = (_, _, _) => throw new NotSupportedException()
                });
            }

            foreach (var mode in NavigationMode.GetAll().Where(mode => mode.IsRefresh || mode.IsNew))
            foreach (var navigationType in NavigationType.GetAll())
            {
                for (var i = 0; i < count; i++)
                {
                    ctx = new NavigationContext(this, TestNavigationProvider.Instance, Guid.NewGuid().ToString(), navigationType, mode);
                    ctx.Metadata.Set(Key, i);
                    NavigationDispatcher.OnNavigating(ctx);
                    contexts.Add(ctx);
                }
            }

            ValidateEntries(NavigationDispatcher.GetNavigationEntries(), contexts, true);
            invokedCount.ShouldEqual(contexts.Count * count);
        }

        [Theory]
        [InlineData(nameof(NavigationMode.New), true)]
        [InlineData(nameof(NavigationMode.Close), false)]
        public void OnNavigationCanceledShouldRemoveOnlyPendingEntries(string modeName, bool shouldRemove)
        {
            var id1 = "id1";
            var id2 = "id2";
            var mode = NavigationMode.Get(modeName);
            Presenter.AddComponent(new TestPresenterComponent
            {
                TryShow = (_, o, _, _) => ItemOrIReadOnlyList.FromItem((IPresenterResult)o)
            });

            var navigationContext = new NavigationContext(this, TestNavigationProvider.Instance, id1, NavigationType.Background, NavigationMode.New);
            var presenterResult = new PresenterResult(this, id2, TestNavigationProvider.Instance, NavigationType.Background);
            NavigationDispatcher.OnNavigated(navigationContext);
            Presenter.TryShow(presenterResult);

            var entries = NavigationDispatcher.GetNavigationEntries(DefaultMetadata);
            entries.Single(entry => entry.NavigationId == id1).ShouldNotBeNull();
            entries.Single(entry => entry.NavigationId == id2).ShouldNotBeNull();

            NavigationDispatcher.OnNavigationCanceled(new NavigationContext(this, TestNavigationProvider.Instance, id1, NavigationType.Background, mode));
            NavigationDispatcher.OnNavigationCanceled(new NavigationContext(this, TestNavigationProvider.Instance, id2, NavigationType.Background, mode));

            entries = NavigationDispatcher.GetNavigationEntries(DefaultMetadata);
            var navigationEntry = entries.SingleOrDefault(entry => entry.NavigationId == id1);
            if (shouldRemove)
                navigationEntry.ShouldBeNull();
            else
                navigationEntry.ShouldNotBeNull();
            entries.SingleOrDefault(entry => entry.NavigationId == id2).ShouldBeNull();
        }

        [Theory]
        [InlineData(nameof(NavigationMode.New), true)]
        [InlineData(nameof(NavigationMode.Close), false)]
        public void OnNavigationFailedShouldRemoveOnlyPendingEntries(string modeName, bool shouldRemove)
        {
            var id1 = "id1";
            var id2 = "id2";
            var mode = NavigationMode.Get(modeName);
            Presenter.AddComponent(new TestPresenterComponent
            {
                TryShow = (_, o, _, _) => ItemOrIReadOnlyList.FromItem((IPresenterResult)o)
            });

            var navigationContext = new NavigationContext(this, TestNavigationProvider.Instance, id1, NavigationType.Background, NavigationMode.New);
            var presenterResult = new PresenterResult(this, id2, TestNavigationProvider.Instance, NavigationType.Background);
            NavigationDispatcher.OnNavigated(navigationContext);
            Presenter.TryShow(presenterResult);

            var entries = NavigationDispatcher.GetNavigationEntries(DefaultMetadata);
            entries.Single(entry => entry.NavigationId == id1).ShouldNotBeNull();
            entries.Single(entry => entry.NavigationId == id2).ShouldNotBeNull();

            NavigationDispatcher.OnNavigationFailed(new NavigationContext(this, TestNavigationProvider.Instance, id1, NavigationType.Background, mode), new Exception());
            NavigationDispatcher.OnNavigationFailed(new NavigationContext(this, TestNavigationProvider.Instance, id2, NavigationType.Background, mode), new Exception());

            entries = NavigationDispatcher.GetNavigationEntries(DefaultMetadata);
            var navigationEntry = entries.SingleOrDefault(entry => entry.NavigationId == id1);
            if (shouldRemove)
                navigationEntry.ShouldBeNull();
            else
                navigationEntry.ShouldNotBeNull();
            entries.SingleOrDefault(entry => entry.NavigationId == id2).ShouldBeNull();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryShowShouldTrackEntriesAdd(int count)
        {
            Presenter.AddComponent(new TestPresenterComponent
            {
                TryShow = (_, o, _, _) => ItemOrIReadOnlyList.FromItem((IPresenterResult)o)
            });

            var contexts = new List<IHasNavigationInfo>();
            IPresenterResult? ctx = null;
            var invokedCount = 0;

            for (var i = 0; i < count; i++)
            {
                var listener = new TestNavigationEntryListener
                {
                    OnNavigationEntryAdded = (_, entry, arg3) =>
                    {
                        ++invokedCount;
                        arg3.ShouldEqual(ctx);
                        entry.IsPending.ShouldBeTrue();
                        entry.NavigationProvider.ShouldEqual(((IHasNavigationProvider)arg3!).NavigationProvider);
                        entry.NavigationType.ShouldEqual(arg3!.NavigationType);
                        entry.NavigationId.ShouldEqual(arg3!.NavigationId);
                        entry.Metadata.Get(Key).ShouldEqual(entry.Metadata.Get(Key));
                    },
                    OnNavigationEntryRemoved = (_, _, _) => throw new NotSupportedException(),
                    OnNavigationEntryUpdated = (_, _, _) => throw new NotSupportedException()
                };
                NavigationDispatcher.AddComponent(listener);
            }

            foreach (var navigationType in NavigationType.GetAll())
            {
                for (var i = 0; i < count; i++)
                {
                    ctx = GetPresenterResult(this, navigationType, null, TestNavigationProvider.Instance);
                    ctx.Metadata.Set(Key, i);
                    Presenter.TryShow(ctx);
                    contexts.Add(ctx);
                }
            }

            ValidateEntries(NavigationDispatcher.GetNavigationEntries(), contexts, true);
            invokedCount.ShouldEqual(contexts.Count * count);
        }

        protected override IPresenter GetPresenter() => new Presenter(ComponentCollectionManager);

        protected override INavigationDispatcher GetNavigationDispatcher() => new NavigationDispatcher(ComponentCollectionManager);

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
                ((IHasNavigationProvider)navigationContext).NavigationProvider.ShouldEqual(entry.NavigationProvider);
                ((IMetadataOwner<IReadOnlyMetadataContext>)navigationContext).Metadata.Get(Key).ShouldEqual(entry.Metadata.Get(Key));
            }
        }
    }
}