using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Internal;
using MugenMvvm.Internal.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Internal
{
    public class ValueHolderCacheWeakReferenceProviderDecoratorTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ShouldCacheWeakReference()
        {
            var invokeCount = 0;
            var weak = new WeakReferenceImpl(this, true);
            var cacheComponent = new ValueHolderCacheWeakReferenceProviderDecorator();
            var providerComponent = new TestWeakReferenceProviderComponent
            {
                TryGetWeakReference = (o, context) =>
                {
                    ++invokeCount;
                    return weak;
                }
            };
            ((IComponentCollectionDecorator<IWeakReferenceProviderComponent>)cacheComponent).Decorate(new List<IWeakReferenceProviderComponent> { cacheComponent, providerComponent }, DefaultMetadata);

            var target = new TestValueHolder<IWeakReference>();
            cacheComponent.TryGetWeakReference(target, DefaultMetadata).ShouldEqual(weak);
            cacheComponent.TryGetWeakReference(target, DefaultMetadata).ShouldEqual(weak);
            target.Value.ShouldEqual(weak);
            invokeCount.ShouldEqual(1);
        }

        #endregion
    }
}