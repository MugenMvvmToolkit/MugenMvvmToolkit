using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Internal;
using MugenMvvm.UnitTest.Components;
using MugenMvvm.UnitTest.Internal.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Internal
{
    public class AttachedValueManagerTest : ComponentOwnerTestBase<AttachedValueManager>
    {
        #region Methods

        [Fact]
        public void TryGetAttachedValuesShouldReturnEmptyNoComponents() => new AttachedValueManager().TryGetAttachedValues(this, DefaultMetadata).IsEmpty.ShouldBeTrue();

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryGetAttachedValuesShouldUseComponents(int componentCount)
        {
            var manager = new AttachedValueManager();
            var request = this;
            var storageManager = new TestAttachedValueStorageManager();
            var state = "";
            var invokeCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var isLast = i == componentCount - 1;
                var component = new TestAttachedValueStorageProviderComponent(manager)
                {
                    Priority = -i,
                    TryGetAttachedValues = (o, arg4) =>
                    {
                        ++invokeCount;
                        o.ShouldEqual(request);
                        arg4.ShouldEqual(DefaultMetadata);
                        if (isLast)
                            return new AttachedValueStorage(this, storageManager, state);
                        return default;
                    }
                };
                manager.AddComponent(component);
            }

            var storage = manager.TryGetAttachedValues(request, DefaultMetadata);
            storageManager.GetCount = (o, o1) =>
            {
                o.ShouldEqual(request);
                o1.ShouldEqual(state);
                return int.MaxValue;
            };
            storage.GetCount().ShouldEqual(int.MaxValue);
            invokeCount.ShouldEqual(componentCount);
        }

        protected override AttachedValueManager GetComponentOwner(IComponentCollectionManager? collectionProvider = null) => new AttachedValueManager(collectionProvider);

        #endregion
    }
}