using MugenMvvm.Binding.Attributes;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Parsing.Attributes
{
    [BindingMacros(Name, true)]
    public class BindingSyntaxExtensionAttributeTest : UnitTestBase
    {
        #region Fields

        private const string Name = "Test";

        #endregion

        #region Methods

        [Fact]
        public void TryGetShouldReturnCorrectAttribute()
        {
            var attribute = (BindingMacrosAttribute) BindingSyntaxExtensionAttributeBase.TryGet(typeof(BindingSyntaxExtensionAttributeTest))!;
            attribute.ResourceName.ShouldEqual(Name);
            attribute.IsStatic.ShouldBeTrue();

            BindingSyntaxExtensionAttributeBase.TryGet(typeof(object)).ShouldBeNull();
        }

        #endregion
    }
}