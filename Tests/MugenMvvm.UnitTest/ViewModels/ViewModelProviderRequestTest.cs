using System;
using MugenMvvm.ViewModels;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.ViewModels
{
    public class ViewModelProviderRequestTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void IsEmptyShouldBeTrueDefault()
        {
            ViewModelProviderRequest request = default;
            request.IsEmpty.ShouldBeTrue();
        }

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            var id = Guid.NewGuid();
            var type = GetType();
            var request = new ViewModelProviderRequest(type, id);
            request.Id.ShouldEqual(id);
            request.Type.ShouldEqual(type);
        }

        #endregion
    }
}