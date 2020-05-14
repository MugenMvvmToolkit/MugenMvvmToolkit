using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Metadata;
using MugenMvvm.Navigation;
using MugenMvvm.Navigation.Components;
using MugenMvvm.UnitTest.Navigation.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Navigation.Components
{
    public class NavigationEntryFinderTest : UnitTestBase
    {
        #region Fields

        private NavigationEntryFinder _component = null!;
        private List<INavigationEntry> _entries = null!;
        private NavigationDispatcher _navigationDispatcher = null!;
        private List<INavigationProvider> _providers = null!;
        private bool _returnNullEntries;

        #endregion

        #region Methods

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryGetPreviousNavigationEntryShouldReturnNullUndefined(int count)
        {
            GenerateData(count);
            var entry = new NavigationEntry(_providers[0], "0", NavigationType.Undefined);
            _component.TryGetPreviousNavigationEntry(entry, null).ShouldBeNull();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryGetPreviousNavigationEntryShouldReturnLastRootEntrySystem(int count)
        {
            GenerateData(count);
            var navigationEntry = _entries
                .Where(entry => entry.NavigationType.IsRootNavigation)
                .OrderByDescending(entry => entry.Metadata.Get(NavigationMetadata.NavigationDate))
                .First();
            foreach (var navigationType in NavigationType.GetAll().Where(type => type.IsSystemNavigation))
            {
                var entry = new NavigationEntry(_providers[0], "0", navigationType);
                _component.TryGetPreviousNavigationEntry(entry, null).ShouldEqual(navigationEntry);
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryGetPreviousNavigationEntryShouldReturnLastFromProviderNotUndefinedOrSystem(int count)
        {
            GenerateData(count);

            foreach (var navigationType in NavigationType.GetAll().Where(type => !type.IsSystemNavigation && !type.IsUndefined))
            {
                foreach (var navigationProvider in _providers)
                {
                    var navigationEntry = _entries
                        .Where(entry => entry.NavigationProvider.Id == navigationProvider.Id && entry.NavigationType == navigationType)
                        .OrderByDescending(entry => entry.Metadata.Get(NavigationMetadata.NavigationDate))
                        .First();

                    var entry = new NavigationEntry(navigationProvider, "0", navigationType);
                    _component.TryGetPreviousNavigationEntry(entry, null).ShouldEqual(navigationEntry);
                }
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryGetPreviousNavigationEntryShouldReturnLastFromProviderIfNullFromRoot(int count)
        {
            GenerateData(count);
            _returnNullEntries = true;
            foreach (var navigationType in NavigationType.GetAll().Where(type => type.IsRootNavigation))
            {
                foreach (var navigationProvider in _providers)
                {
                    var navigationEntry = _entries
                        .Where(entry => entry.NavigationType.IsRootNavigation)
                        .OrderByDescending(entry => entry.Metadata.Get(NavigationMetadata.NavigationDate))
                        .First();

                    var entry = new NavigationEntry(navigationProvider, "0", navigationType);
                    _component.TryGetPreviousNavigationEntry(entry, null).ShouldEqual(navigationEntry);
                }
            }
        }

        private void GenerateData(int count)
        {
            _returnNullEntries = false;
            _entries = new List<INavigationEntry>();
            _providers = new List<INavigationProvider>();
            for (var i = 0; i < count; i++)
                _providers.Add(new TestNavigationProvider { Id = i.ToString() });

            foreach (var navigationType in NavigationType.GetAll())
            {
                for (var i = 0; i < count; i++)
                    for (var j = 0; j < count; j++)
                    {
                        var navigationEntry = new NavigationEntry(_providers[i], i + "-" + j, navigationType);
                        _entries.Add(navigationEntry);
                        navigationEntry.Metadata.Set(NavigationMetadata.NavigationDate, DateTime.UtcNow.AddSeconds(j + i));
                    }
            }

            _navigationDispatcher = new NavigationDispatcher();
            _component = new NavigationEntryFinder();
            var provider = new TestNavigationEntryProviderComponent
            {
                TryGetNavigationEntries = (type, context) =>
                {
                    if (type == null)
                        return _entries;
                    if (_returnNullEntries)
                        return null;
                    return _entries.Where(entry => entry.NavigationType == type).ToArray();
                }
            };

            _navigationDispatcher.AddComponent(_component);
            _navigationDispatcher.AddComponent(provider);
        }

        #endregion
    }
}