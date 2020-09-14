using MugenMvvm.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Internal
{
    public class WeakReferenceImplTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ReleaseShouldClearReference()
        {
            var weakReferenceImpl = new WeakReferenceImpl(this, true);
            weakReferenceImpl.Target.ShouldNotBeNull();

            weakReferenceImpl.Release();
            weakReferenceImpl.Target.ShouldBeNull();
        }

        #endregion
    }
}