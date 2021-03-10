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
using MugenMvvm.UnitTests.Navigation.Internal;
using MugenMvvm.UnitTests.Presentation.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Navigation.Components
{
    public class NavigationEntryManagerTest : UnitTestBase
    {
        private static readonly IMetadataContextKey<int> Key = MetadataContextKey.FromKey<int>("i");
        private readonly NavigationDispatcher _navigationDispatcher;
        private readonly NavigationEntryManager _entryManager;
        private readonly Presenter _presenter;

        public NavigationEntryManagerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _presenter = new Presenter(ComponentCollectionManager);
            _navigationDispatcher = new NavigationDispatcher(ComponentCollectionManager);
            _entryManager = new NavigationEntryManager();
            _navigationDispatcher.AddComponent(_entryManager);
            _presenter.AddComponent(_entryManager);
        }

        [Fact]
        public void OnNavigatedShouldUpdatePendingEntry()
        {
            var provider = new TestNavigationProvider {Id = "t"};
            _presenter.AddComponent(new TestPresenterComponent
            {
                TryShow = (o, context, arg3) => ItemOrIReadOnlyList.FromItem((IPresenterResult) o)
            });

            var navigationContext = new NavigationContext(this, provider, "id1", NavigationType.Background, NavigationMode.New);
            var presenterResult = new PresenterResult(this, "id1", provider, NavigationType.Background);

            _presenter.TryShow(presenterResult);
            var entries = _navigationDispatcher.GetNavigationEntries(DefaultMetadata);
            entries.AsList().Single(entry => entry.NavigationId == navigationContext.NavigationId).IsPending.ShouldBeTrue();

            _navigationDispatcher.OnNavigated(navigationContext);
            entries = _navigationDispatcher.GetNavigationEntries(DefaultMetadata);
            entries.AsList().Single(entry => entry.NavigationId == navigationContext.NavigationId).IsPending.ShouldBeFalse();
        }

        [Theory]
        [InlineData(nameof(NavigationMode.New), true)]
        [InlineData(nameof(NavigationMode.Close), false)]
        public void OnNavigationCanceledShouldRemoveOnlyPendingEntries(string modeName, bool shouldRemove)
        {
            var id1 = "id1";
            var id2 = "id2";
            var mode = NavigationMode.Get(modeName);
            var provider = new TestNavigationProvider {Id = "t"};
            _presenter.AddComponent(new TestPresenterComponent
            {
                TryShow = (o, context, arg3) => ItemOrIReadOnlyList.FromItem((IPresenterResult) o)
            });

            var navigationContext = new NavigationContext(this, provider, id1, NavigationType.Background, NavigationMode.New);
            var presenterResult = new PresenterResult(this, id2, provider, NavigationType.Background);
            _navigationDispatcher.OnNavigated(navigationContext);
            _presenter.TryShow(presenterResult);

            var entries = _navigationDispatcher.GetNavigationEntries(DefaultMetadata);
            entries.AsList().Single(entry => entry.NavigationId == id1).ShouldNotBeNull();
            entries.AsList().Single(entry => entry.NavigationId == id2).ShouldNotBeNull();

            _navigationDispatcher.OnNavigationCanceled(new NavigationContext(this, provider, id1, NavigationType.Background, mode));
            _navigationDispatcher.OnNavigationCanceled(new NavigationContext(this, provider, id2, NavigationType.Background, mode));

            entries = _navigationDispatcher.GetNavigationEntries(DefaultMetadata);
            var navigationEntry = entries.AsList().SingleOrDefault(entry => entry.NavigationId == id1);
            if (shouldRemove)
                navigationEntry.ShouldBeNull();
            else
                navigationEntry.ShouldNotBeNull();
            entries.AsList().SingleOrDefault(entry => entry.NavigationId == id2).ShouldBeNull();
        }

        [Theory]
        [InlineData(nameof(NavigationMode.New), true)]
        [InlineData(nameof(NavigationMode.Close), false)]
        public void OnNavigationFailedShouldRemoveOnlyPendingEntries(string modeName, bool shouldRemove)
        {
            var id1 = "id1";
            var id2 = "id2";
            var mode = NavigationMode.Get(modeName);
            var provider = new TestNavigationProvider {Id = "t"};
            _presenter.AddComponent(new TestPresenterComponent
            {
                TryShow = (o, context, arg3) => ItemOrIReadOnlyList.FromItem((IPresenterResult) o)
            });

            var navigationContext = new NavigationContext(this, provider, id1, NavigationType.Background, NavigationMode.New);
            var presenterResult = new PresenterResult(this, id2, provider, NavigationType.Background);
            _navigationDispatcher.OnNavigated(navigationContext);
            _presenter.TryShow(presenterResult);

            var entries = _navigationDispatcher.GetNavigationEntries(DefaultMetadata);
            entries.AsList().Single(entry => entry.NavigationId == id1).ShouldNotBeNull();
            entries.AsList().Single(entry => entry.NavigationId == id2).ShouldNotBeNull();

            _navigationDispatcher.OnNavigationFailed(new NavigationContext(this, provider, id1, NavigationType.Background, mode), new Exception());
            _navigationDispatcher.OnNavigationFailed(new NavigationContext(this, provider, id2, NavigationType.Background, mode), new Exception());

            entries = _navigationDispatcher.GetNavigationEntries(DefaultMetadata);
            var navigationEntry = entries.AsList().SingleOrDefault(entry => entry.NavigationId == id1);
            if (shouldRemove)
                navigationEntry.ShouldBeNull();
            else
                navigationEntry.ShouldNotBeNull();
            entries.AsList().SingleOrDefault(entry => entry.NavigationId == id2).ShouldBeNull();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void OnNavigatingShouldTrackEntriesAdd(int count)
        {
            var provider = new TestNavigationProvider {Id = "t"};
            var contexts = new List<NavigationContext>();
            NavigationContext? ctx = null;
            var invokedCount = 0;

            for (var i = 0; i < count; i++)
            {
                var listener = new TestNavigationEntryListener(_navigationDispatcher)
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
                _navigationDispatcher.AddComponent(listener);
            }

            foreach (var mode in NavigationMode.GetAll().Where(mode => mode.IsRefresh || mode.IsNew))
            foreach (var navigationType in NavigationType.GetAll())
            {
                for (var i = 0; i < count; i++)
                {
                    ctx = new NavigationContext(this, provider, Guid.NewGuid().ToString(), navigationType, mode);
                    ctx.Metadata.Set(Key, i);
                    _navigationDispatcher.OnNavigating(ctx);
                    contexts.Add(ctx);
                }
            }

            ValidateEntries(_navigationDispatcher.GetNavigationEntries().AsList(), contexts, true);
            invokedCount.ShouldEqual(contexts.Count * count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void OnNavigatedShouldTrackEntriesAdd(int count)
        {
            var provider = new TestNavigationProvider {Id = "t"};
            var contexts = new List<NavigationContext>();
            NavigationContext? ctx = null;
            var invokedCount = 0;

            for (var i = 0; i < count; i++)
            {
                var listener = new TestNavigationEntryListener(_navigationDispatcher)
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
                _navigationDispatcher.AddComponent(listener);
            }

            foreach (var mode in NavigationMode.GetAll().Where(mode => mode.IsRefresh || mode.IsNew))
            foreach (var navigationType in NavigationType.GetAll())
            {
                for (var i = 0; i < count; i++)
                {
                    ctx = new NavigationContext(this, provider, Guid.NewGuid().ToString(), navigationType, mode);
                    ctx.Metadata.Set(Key, i);
                    _navigationDispatcher.OnNavigated(ctx);
                    contexts.Add(ctx);
                }
            }

            ValidateEntries(_navigationDispatcher.GetNavigationEntries().AsList(), contexts);
            invokedCount.ShouldEqual(contexts.Count * count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void OnNavigatedShouldTrackEntriesUpdate(int count)
        {
            var provider = new TestNavigationProvider {Id = "t"};
            var contexts = new List<NavigationContext>();
            NavigationContext? ctx = null;
            var invokedCount = 0;

            foreach (var mode in NavigationMode.GetAll().Where(mode => mode.IsRefresh || mode.IsNew))
            foreach (var navigationType in NavigationType.GetAll())
            {
                for (var i = 0; i < count; i++)
                {
                    ctx = new NavigationContext(this, provider, Guid.NewGuid().ToString(), navigationType, mode);
                    ctx.Metadata.Set(Key, i);
                    _navigationDispatcher.OnNavigated(ctx);
                    contexts.Add(ctx);
                }
            }

            for (var i = 0; i < count; i++)
            {
                var listener = new TestNavigationEntryListener(_navigationDispatcher)
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
                _navigationDispatcher.AddComponent(listener);
            }

            foreach (var navigationContext in contexts)
            {
                ctx = navigationContext;
                _navigationDispatcher.OnNavigated(navigationContext);
            }

            ValidateEntries(_navigationDispatcher.GetNavigationEntries().AsList(), contexts);
            invokedCount.ShouldEqual(contexts.Count * count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void OnNavigatedShouldTrackEntriesRemove(int count)
        {
            var provider = new TestNavigationProvider {Id = "t"};
            var contexts = new List<NavigationContext>();
            NavigationContext? ctx = null;

            foreach (var closeMode in NavigationMode.GetAll().Where(mode => mode.IsClose))
            {
                _navigationDispatcher.Components.Clear();
                _navigationDispatcher.AddComponent(_entryManager);
                var invokedCount = 0;
                foreach (var mode in NavigationMode.GetAll().Where(mode => mode.IsRefresh || mode.IsNew))
                foreach (var navigationType in NavigationType.GetAll())
                {
                    for (var i = 0; i < count; i++)
                    {
                        ctx = new NavigationContext(this, provider, Guid.NewGuid().ToString(), navigationType, mode);
                        ctx.Metadata.Set(Key, i);
                        _navigationDispatcher.OnNavigated(ctx);
                        contexts.Add(ctx);
                    }
                }

                for (var i = 0; i < count; i++)
                {
                    var listener = new TestNavigationEntryListener(_navigationDispatcher)
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
                    _navigationDispatcher.AddComponent(listener);
                }

                foreach (var navigationContext in contexts)
                {
                    ctx = new NavigationContext(this, navigationContext.NavigationProvider, navigationContext.NavigationId, navigationContext.NavigationType, closeMode,
                        navigationContext.Metadata);
                    _navigationDispatcher.OnNavigated(ctx);
                }

                invokedCount.ShouldEqual(contexts.Count * count);
                contexts.Clear();
                ValidateEntries(_navigationDispatcher.GetNavigationEntries().AsList(), contexts);
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryShowShouldTrackEntriesAdd(int count)
        {
            var provider = new TestNavigationProvider {Id = "t"};
            _presenter.AddComponent(new TestPresenterComponent
            {
                TryShow = (o, context, arg3) => ItemOrIReadOnlyList.FromItem((IPresenterResult) o)
            });

            var contexts = new List<IHasNavigationInfo>();
            IPresenterResult? ctx = null;
            var invokedCount = 0;

            for (var i = 0; i < count; i++)
            {
                var listener = new TestNavigationEntryListener(_navigationDispatcher)
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
                _navigationDispatcher.AddComponent(listener);
            }

            foreach (var navigationType in NavigationType.GetAll())
            {
                for (var i = 0; i < count; i++)
                {
                    ctx = new PresenterResult(this, Guid.NewGuid().ToString(), provider, navigationType);
                    ctx.Metadata.Set(Key, i);
                    _presenter.TryShow(ctx);
                    contexts.Add(ctx);
                }
            }

            ValidateEntries(_navigationDispatcher.GetNavigationEntries().AsList(), contexts, true);
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