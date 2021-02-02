using System;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Components;
using MugenMvvm.UnitTests.Internal.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Internal
{
    public class WeakReferenceManagerTest : ComponentOwnerTestBase<WeakReferenceManager>
    {
        [Fact]
        public void GetWeakReferenceShouldReturnDefaultWeakReferenceNull()
        {
            var weakReferenceManager = GetComponentOwner(ComponentCollectionManager);
            weakReferenceManager.GetWeakReference(null, DefaultMetadata).ShouldEqual(WeakReferenceImpl.Empty);
        }

        [Fact]
        public void GetWeakReferenceShouldThrowNoComponents()
        {
            ShouldThrow<InvalidOperationException>(() => GetComponentOwner(ComponentCollectionManager).GetWeakReference(this, DefaultMetadata));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetWeakReferenceShouldBeHandledByComponents(int count)
        {
            var result = new WeakReferenceImpl(this, true);
            var weakReferenceManager = GetComponentOwner(ComponentCollectionManager);
            var invokeCount = 0;
            for (var i = 0; i < count; i++)
            {
                var canReturn = i == count - 1;
                var component = new TestWeakReferenceProviderComponent(weakReferenceManager)
                {
                    Priority = -i,
                    TryGetWeakReference = (o, context) =>
                    {
                        ++invokeCount;
                        context.ShouldEqual(DefaultMetadata);
                        if (canReturn)
                            return result;
                        return null;
                    }
                };
                weakReferenceManager.AddComponent(component);
            }

            weakReferenceManager.GetWeakReference(this, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(count);
        }

        protected override WeakReferenceManager GetComponentOwner(IComponentCollectionManager? componentCollectionManager = null) => new(componentCollectionManager);
    }
}