using System;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Metadata;
using MugenMvvm.Navigation;
using MugenMvvm.Navigation.Components;
using MugenMvvm.Tests.Navigation;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Navigation.Components
{
    public class NavigationEntryDateTrackerTest : UnitTestBase
    {
        private readonly NavigationEntryDateTracker _component;

        public NavigationEntryDateTrackerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _component = new NavigationEntryDateTracker();
            NavigationDispatcher.AddComponent(_component);
        }

        [Fact]
        public void OnNavigationEntryAddedShouldSetDate()
        {
            var entry = new NavigationEntry(this, TestNavigationProvider.Instance, "et", NavigationType.Alert);
            var context = GetNavigationContext(this, NavigationMode.Close, entry.NavigationType, entry.NavigationId, entry.NavigationProvider);

            var utcNow = DateTime.UtcNow;
            _component.OnNavigationEntryAdded(NavigationDispatcher, entry, context);

            var dateTime = entry.Metadata.Get(NavigationMetadata.NavigationDate);
            utcNow.ShouldBeLessThanOrEqualTo(dateTime);
        }

        [Fact]
        public void OnNavigationEntryRemovedShouldNotSetDate()
        {
            var entry = new NavigationEntry(this, TestNavigationProvider.Instance, "et", NavigationType.Alert);
            var context = GetNavigationContext(this, NavigationMode.Close, entry.NavigationType, entry.NavigationId, entry.NavigationProvider);

            _component.OnNavigationEntryRemoved(NavigationDispatcher, entry, context);
            entry.Metadata.Contains(NavigationMetadata.NavigationDate).ShouldBeFalse();
        }

        [Fact]
        public void OnNavigationEntryUpdatedShouldSetDate()
        {
            var entry = new NavigationEntry(this, TestNavigationProvider.Instance, "et", NavigationType.Alert);
            var context = GetNavigationContext(this, NavigationMode.Close, entry.NavigationType, entry.NavigationId, entry.NavigationProvider);

            var utcNow = DateTime.UtcNow;
            _component.OnNavigationEntryUpdated(NavigationDispatcher, entry, context);

            var dateTime = entry.Metadata.Get(NavigationMetadata.NavigationDate);
            utcNow.ShouldBeLessThanOrEqualTo(dateTime);
        }

        protected override INavigationDispatcher GetNavigationDispatcher() => new NavigationDispatcher(ComponentCollectionManager);
    }
}