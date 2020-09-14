using System;
using MugenMvvm.Internal.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Internal.Components
{
    public class WeakReferenceProviderComponentTest : UnitTestBase
    {
        #region Methods

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldReturnWeakReference(bool trackResurrection)
        {
            var component = new WeakReferenceProviderComponent {TrackResurrection = trackResurrection};
            var weakReference = (WeakReference) component.TryGetWeakReference(null!, this, DefaultMetadata)!;
            weakReference.TrackResurrection.ShouldEqual(trackResurrection);
            weakReference.Target.ShouldEqual(this);
        }

        #endregion
    }
}