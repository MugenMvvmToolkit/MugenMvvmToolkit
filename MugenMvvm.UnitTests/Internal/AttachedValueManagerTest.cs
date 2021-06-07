using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Internal;
using MugenMvvm.Tests.Internal;
using MugenMvvm.UnitTests.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Internal
{
    public class AttachedValueManagerTest : ComponentOwnerTestBase<AttachedValueManager>
    {
        [Fact]
        public void TryGetAttachedValuesShouldReturnEmptyNoComponents() => AttachedValueManager.TryGetAttachedValues(this, DefaultMetadata).IsEmpty.ShouldBeTrue();

        protected override IAttachedValueManager GetAttachedValueManager() => GetComponentOwner(ComponentCollectionManager);

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryGetAttachedValuesShouldUseComponents(int componentCount)
        {
            var request = this;
            var storageManager = new TestAttachedValueStorageManager();
            var state = "";
            var invokeCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var isLast = i == componentCount - 1;
                var component = new TestAttachedValueStorageProviderComponent
                {
                    Priority = -i,
                    TryGetAttachedValues = (owner, o, m) =>
                    {
                        owner.ShouldEqual(AttachedValueManager);
                        o.ShouldEqual(request);
                        m.ShouldEqual(DefaultMetadata);
                        ++invokeCount;
                        if (isLast)
                            return new AttachedValueStorage(this, storageManager, state);
                        return default;
                    }
                };
                AttachedValueManager.AddComponent(component);
            }

            var storage = AttachedValueManager.TryGetAttachedValues(request, DefaultMetadata);
            storageManager.GetCount = (o, o1) =>
            {
                o.ShouldEqual(request);
                o1.ShouldEqual(state);
                return int.MaxValue;
            };
            storage.GetCount().ShouldEqual(int.MaxValue);
            invokeCount.ShouldEqual(componentCount);
        }

        protected override AttachedValueManager GetComponentOwner(IComponentCollectionManager? componentCollectionManager = null) => new(componentCollectionManager);
    }
}