using MugenMvvm.Binding.Core;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Core
{
    public class BindingTargetSourceStateTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void DefaultShouldBeEmpty()
        {
            default(BindingTargetSourceState).IsEmpty.ShouldBeTrue();
        }

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            var target = typeof(object);
            var source = "";
            var memberManagerRequest = new BindingTargetSourceState(target, source);
            memberManagerRequest.Target.ShouldEqual(target);
            memberManagerRequest.Source.ShouldEqual(source);
            memberManagerRequest.IsEmpty.ShouldBeFalse();
        }

        #endregion
    }
}