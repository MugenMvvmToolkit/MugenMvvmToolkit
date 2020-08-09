using System;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Internal;
using MugenMvvm.UnitTest.Components;
using MugenMvvm.UnitTest.Internal.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Internal
{
    public class WeakReferenceManagerTest : ComponentOwnerTestBase<WeakReferenceManager>
    {
        #region Methods

        [Fact]
        public void GetWeakReferenceShouldReturnDefaultWeakReferenceNull()
        {
            var weakReferenceManager = new WeakReferenceManager();
            weakReferenceManager.GetWeakReference(null, DefaultMetadata).ShouldEqual(Default.WeakReference);
        }

        [Fact]
        public void GetWeakReferenceShouldThrowNoComponents()
        {
            var weakReferenceManager = new WeakReferenceManager();
            ShouldThrow<InvalidOperationException>(() => weakReferenceManager.GetWeakReference(this, DefaultMetadata));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetWeakReferenceShouldBeHandledByComponents(int count)
        {
            var result = new WeakReferenceImpl(this, true);
            var weakReferenceManager = new WeakReferenceManager();
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

        protected override WeakReferenceManager GetComponentOwner(IComponentCollectionManager? collectionProvider = null) => new WeakReferenceManager(collectionProvider);

        #endregion
    }
}