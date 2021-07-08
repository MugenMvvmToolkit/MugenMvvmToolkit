using MugenMvvm.Bindings.Attributes;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Attributes
{
    [BindingMacros(Name, true)]
    public class BindingSyntaxExtensionAttributeTest : UnitTestBase
    {
        private const string Name = "Test";

        [Fact]
        public void TryGetShouldReturnCorrectAttribute()
        {
            var attribute = (BindingMacrosAttribute)BindingSyntaxExtensionAttributeBase.TryGet(typeof(BindingSyntaxExtensionAttributeTest))!;
            attribute.ResourceName.ShouldEqual(Name);
            attribute.IsStatic.ShouldBeTrue();

            BindingSyntaxExtensionAttributeBase.TryGet(typeof(object)).ShouldBeNull();
        }
    }
}