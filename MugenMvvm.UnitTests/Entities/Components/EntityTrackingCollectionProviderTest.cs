using System.Collections.Generic;
using MugenMvvm.Constants;
using MugenMvvm.Entities;
using MugenMvvm.Entities.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Entities.Components
{
    public class EntityTrackingCollectionProviderTest : UnitTestBase
    {
        [Fact]
        public void ShouldReturnCollection()
        {
            var provider = new EntityTrackingCollectionProvider(ComponentCollectionManager);
            provider.Priority.ShouldEqual(EntityComponentPriority.TrackingCollectionProvider);
            var collection = (EntityTrackingCollection) provider.TryGetTrackingCollection(null!, this, DefaultMetadata)!;
            collection.Components.Get<object>().Single().ShouldEqual(EntityStateTransitionManager.Instance);
            collection.Comparer.ShouldEqual(EqualityComparer<object>.Default);

            collection = (EntityTrackingCollection) provider.TryGetTrackingCollection(null!, ReferenceEqualityComparer.Instance, DefaultMetadata)!;
            collection.Components.Get<object>().Single().ShouldEqual(EntityStateTransitionManager.Instance);
            collection.Comparer.ShouldEqual(ReferenceEqualityComparer.Instance);
        }
    }
}