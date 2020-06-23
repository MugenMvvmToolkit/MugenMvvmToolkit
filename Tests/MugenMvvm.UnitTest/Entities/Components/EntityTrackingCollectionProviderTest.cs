using System.Collections.Generic;
using MugenMvvm.Constants;
using MugenMvvm.Entities;
using MugenMvvm.Entities.Components;
using MugenMvvm.UnitTest.Internal.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Entities.Components
{
    public class EntityTrackingCollectionProviderTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ShouldReturnCollection()
        {
            var provider = new EntityTrackingCollectionProvider();
            provider.Priority.ShouldEqual(EntityComponentPriority.TrackingCollectionProvider);
            var collection = (EntityTrackingCollection) provider.TryGetTrackingCollection(this, DefaultMetadata)!;
            collection.Comparer.ShouldEqual(EqualityComparer<object>.Default);

            collection = (EntityTrackingCollection) provider.TryGetTrackingCollection(ReferenceEqualityComparer.Instance, DefaultMetadata)!;
            collection.Comparer.ShouldEqual(ReferenceEqualityComparer.Instance);
        }

        #endregion
    }
}