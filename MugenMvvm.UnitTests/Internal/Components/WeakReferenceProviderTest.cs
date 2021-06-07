using System;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Internal;
using MugenMvvm.Internal.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Internal.Components
{
    public class WeakReferenceProviderTest : UnitTestBase
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldReturnWeakReference(bool trackResurrection)
        {
            WeakReferenceManager.AddComponent(new WeakReferenceProvider { TrackResurrection = trackResurrection });
            var weakReference = (WeakReference)WeakReferenceManager.TryGetWeakReference(this, DefaultMetadata)!;
            weakReference.TrackResurrection.ShouldEqual(trackResurrection);
            weakReference.Target.ShouldEqual(this);
        }

        protected override IWeakReferenceManager GetWeakReferenceManager() => new WeakReferenceManager(ComponentCollectionManager);
    }
}