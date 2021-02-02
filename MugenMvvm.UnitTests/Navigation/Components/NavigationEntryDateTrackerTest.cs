using System;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Metadata;
using MugenMvvm.Navigation;
using MugenMvvm.Navigation.Components;
using MugenMvvm.UnitTests.Navigation.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Navigation.Components
{
    public class NavigationEntryDateTrackerTest : UnitTestBase
    {
        private readonly NavigationDispatcher _navigationDispatcher;
        private readonly NavigationEntryDateTracker _component;

        public NavigationEntryDateTrackerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _navigationDispatcher = new NavigationDispatcher(ComponentCollectionManager);
            _component = new NavigationEntryDateTracker();
            _navigationDispatcher.AddComponent(_component);
        }

        [Fact]
        public void OnNavigationEntryAddedShouldSetDate()
        {
            var entry = new NavigationEntry(this, new TestNavigationProvider(), "et", NavigationType.Alert);
            var context = new NavigationContext(this, entry.NavigationProvider, entry.NavigationId, entry.NavigationType, NavigationMode.Close);

            var utcNow = DateTime.UtcNow;
            _component.OnNavigationEntryAdded(_navigationDispatcher, entry, context);

            var dateTime = entry.Metadata.Get(NavigationMetadata.NavigationDate);
            utcNow.ShouldBeLessThanOrEqualTo(dateTime);
        }

        [Fact]
        public void OnNavigationEntryRemovedShouldNotSetDate()
        {
            var entry = new NavigationEntry(this, new TestNavigationProvider(), "et", NavigationType.Alert);
            var context = new NavigationContext(this, entry.NavigationProvider, entry.NavigationId, entry.NavigationType, NavigationMode.Close);

            _component.OnNavigationEntryRemoved(_navigationDispatcher, entry, context);
            entry.Metadata.Contains(NavigationMetadata.NavigationDate).ShouldBeFalse();
        }

        [Fact]
        public void OnNavigationEntryUpdatedShouldSetDate()
        {
            var entry = new NavigationEntry(this, new TestNavigationProvider(), "et", NavigationType.Alert);
            var context = new NavigationContext(this, entry.NavigationProvider, entry.NavigationId, entry.NavigationType, NavigationMode.Close);

            var utcNow = DateTime.UtcNow;
            _component.OnNavigationEntryUpdated(_navigationDispatcher, entry, context);

            var dateTime = entry.Metadata.Get(NavigationMetadata.NavigationDate);
            utcNow.ShouldBeLessThanOrEqualTo(dateTime);
        }
    }
}