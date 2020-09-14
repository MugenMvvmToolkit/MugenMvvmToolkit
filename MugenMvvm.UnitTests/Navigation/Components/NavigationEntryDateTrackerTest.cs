using System;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Metadata;
using MugenMvvm.Navigation;
using MugenMvvm.Navigation.Components;
using MugenMvvm.UnitTests.Navigation.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Navigation.Components
{
    public class NavigationEntryDateTrackerTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void OnNavigationEntryAddedShouldSetDate()
        {
            var dispatcher = new NavigationDispatcher();
            var entry = new NavigationEntry(this, new TestNavigationProvider(), "et", NavigationType.Alert);
            var context = new NavigationContext(this, entry.NavigationProvider, entry.NavigationId, entry.NavigationType, NavigationMode.Close);
            var component = new NavigationEntryDateTracker();

            var utcNow = DateTime.UtcNow;
            component.OnNavigationEntryAdded(dispatcher, entry, context);

            var dateTime = entry.Metadata.Get(NavigationMetadata.NavigationDate);
            utcNow.ShouldBeLessThanOrEqualTo(dateTime);
        }

        [Fact]
        public void OnNavigationEntryUpdatedShouldSetDate()
        {
            var dispatcher = new NavigationDispatcher();
            var entry = new NavigationEntry(this, new TestNavigationProvider(), "et", NavigationType.Alert);
            var context = new NavigationContext(this, entry.NavigationProvider, entry.NavigationId, entry.NavigationType, NavigationMode.Close);
            var component = new NavigationEntryDateTracker();

            var utcNow = DateTime.UtcNow;
            component.OnNavigationEntryUpdated(dispatcher, entry, context);

            var dateTime = entry.Metadata.Get(NavigationMetadata.NavigationDate);
            utcNow.ShouldBeLessThanOrEqualTo(dateTime);
        }

        [Fact]
        public void OnNavigationEntryRemovedShouldNotSetDate()
        {
            var dispatcher = new NavigationDispatcher();
            var entry = new NavigationEntry(this, new TestNavigationProvider(), "et", NavigationType.Alert);
            var context = new NavigationContext(this, entry.NavigationProvider, entry.NavigationId, entry.NavigationType, NavigationMode.Close);
            var component = new NavigationEntryDateTracker();

            component.OnNavigationEntryRemoved(dispatcher, entry, context);
            entry.Metadata.Contains(NavigationMetadata.NavigationDate).ShouldBeFalse();
        }

        #endregion
    }
}