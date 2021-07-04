using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Entities;
using MugenMvvm.Entities.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Entities;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Entities.Components
{
    public class EntityTrackingCollectionProviderTest : UnitTestBase
    {
        [Fact]
        public void ShouldReturnCollection()
        {
            EntityManager.AddComponent(new EntityTrackingCollectionProvider(ComponentCollectionManager));

            var collection = (EntityTrackingCollection)EntityManager.TryGetTrackingCollection(this, DefaultMetadata)!;
            collection.Components.Get<object>().Single().ShouldEqual(EntityStateTransitionManager.Instance);
            collection.Comparer.ShouldEqual(EqualityComparer<object>.Default);

            collection = (EntityTrackingCollection)EntityManager.TryGetTrackingCollection(ReferenceEqualityComparer.Instance, DefaultMetadata)!;
            collection.Components.Get<object>().Single().ShouldEqual(EntityStateTransitionManager.Instance);
            collection.Comparer.ShouldEqual(ReferenceEqualityComparer.Instance);
        }

        protected override IEntityManager GetEntityManager() => new EntityManager(ComponentCollectionManager);
    }
}