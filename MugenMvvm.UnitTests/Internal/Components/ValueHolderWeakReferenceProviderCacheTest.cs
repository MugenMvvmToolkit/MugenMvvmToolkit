using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Internal;
using MugenMvvm.Internal.Components;
using MugenMvvm.Tests.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Internal.Components
{
    public class ValueHolderWeakReferenceProviderCacheTest : UnitTestBase
    {
        [Fact]
        public void ShouldCacheWeakReference()
        {
            var invokeCount = 0;
            var weak = new WeakReferenceImpl(this, true);
            WeakReferenceManager.AddComponent(new TestWeakReferenceProviderComponent
            {
                TryGetWeakReference = (m, o, context) =>
                {
                    m.ShouldEqual(WeakReferenceManager);
                    ++invokeCount;
                    return weak;
                }
            });

            var target = new TestValueHolder<IWeakReference>();
            WeakReferenceManager.TryGetWeakReference(target, Metadata).ShouldEqual(weak);
            WeakReferenceManager.TryGetWeakReference(target, Metadata).ShouldEqual(weak);
            target.Value.ShouldEqual(weak);
            invokeCount.ShouldEqual(1);
        }

        protected override IWeakReferenceManager GetWeakReferenceManager()
        {
            var weakReferenceManager = new WeakReferenceManager(ComponentCollectionManager);
            weakReferenceManager.AddComponent(new ValueHolderWeakReferenceProviderCache());
            return weakReferenceManager;
        }
    }
}