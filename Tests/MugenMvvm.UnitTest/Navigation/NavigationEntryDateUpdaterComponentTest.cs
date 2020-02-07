using System;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Metadata;
using MugenMvvm.Navigation;
using MugenMvvm.Navigation.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Navigation
{
    public class NavigationEntryDateUpdaterComponentTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void OnNavigationEntryAddedShouldSetDate()
        {
            var dispatcher = new NavigationDispatcher();
            var entry = new NavigationEntry(new TestNavigationProvider(), "et", NavigationType.Alert);
            var context = new NavigationContext(entry.NavigationProvider, entry.NavigationOperationId, entry.NavigationType, NavigationMode.Remove);
            var component = new NavigationEntryDateUpdaterComponent();

            var utcNow = DateTime.UtcNow;
            component.OnNavigationEntryAdded(dispatcher, entry, context);

            var dateTime = entry.Metadata.Get(NavigationMetadata.NavigationDate);
            utcNow.ShouldBeLessThanOrEqualTo(dateTime);
        }

        [Fact]
        public void OnNavigationEntryUpdatedShouldSetDate()
        {
            var dispatcher = new NavigationDispatcher();
            var entry = new NavigationEntry(new TestNavigationProvider(), "et", NavigationType.Alert);
            var context = new NavigationContext(entry.NavigationProvider, entry.NavigationOperationId, entry.NavigationType, NavigationMode.Remove);
            var component = new NavigationEntryDateUpdaterComponent();

            var utcNow = DateTime.UtcNow;
            component.OnNavigationEntryUpdated(dispatcher, entry, context);

            var dateTime = entry.Metadata.Get(NavigationMetadata.NavigationDate);
            utcNow.ShouldBeLessThanOrEqualTo(dateTime);
        }

        [Fact]
        public void OnNavigationEntryRemovedShouldNotSetDate()
        {
            var dispatcher = new NavigationDispatcher();
            var entry = new NavigationEntry(new TestNavigationProvider(), "et", NavigationType.Alert);
            var context = new NavigationContext(entry.NavigationProvider, entry.NavigationOperationId, entry.NavigationType, NavigationMode.Remove);
            var component = new NavigationEntryDateUpdaterComponent();

            component.OnNavigationEntryRemoved(dispatcher, entry, context);
            entry.Metadata.Contains(NavigationMetadata.NavigationDate).ShouldBeFalse();
        }

        #endregion
    }
}