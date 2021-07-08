using System;
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
    public class WeakReferenceManagerTest : ComponentOwnerTestBase<WeakReferenceManager>
    {
        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetWeakReferenceShouldBeHandledByComponents(int count)
        {
            var result = new WeakReferenceImpl(this, true);
            var invokeCount = 0;
            for (var i = 0; i < count; i++)
            {
                var canReturn = i == count - 1;
                var component = new TestWeakReferenceProviderComponent
                {
                    Priority = -i,
                    TryGetWeakReference = (w, o, context) =>
                    {
                        w.ShouldEqual(WeakReferenceManager);
                        o.ShouldEqual(this);
                        context.ShouldEqual(DefaultMetadata);
                        ++invokeCount;
                        if (canReturn)
                            return result;
                        return null;
                    }
                };
                WeakReferenceManager.AddComponent(component);
            }

            WeakReferenceManager.GetWeakReference(this, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(count);
        }

        [Fact]
        public void GetWeakReferenceShouldReturnDefaultWeakReferenceNull() => WeakReferenceManager.GetWeakReference(null, DefaultMetadata).ShouldEqual(WeakReferenceImpl.Empty);

        [Fact]
        public void GetWeakReferenceShouldThrowNoComponents() => ShouldThrow<InvalidOperationException>(() => WeakReferenceManager.GetWeakReference(this, DefaultMetadata));

        protected override IWeakReferenceManager GetWeakReferenceManager() => GetComponentOwner(ComponentCollectionManager);

        protected override WeakReferenceManager GetComponentOwner(IComponentCollectionManager? componentCollectionManager = null) => new(componentCollectionManager);
    }
}