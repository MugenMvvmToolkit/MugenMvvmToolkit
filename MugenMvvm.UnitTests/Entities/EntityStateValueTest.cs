using MugenMvvm.Entities;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Entities
{
    public class EntityStateValueTest : UnitTestBase
    {
        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            var member = new object();
            var oldValue = new object();
            var newValue = new object();
            var value = new EntityStateValue(member, oldValue, newValue);
            value.IsEmpty.ShouldBeFalse();
            value.Member.ShouldEqual(member);
            value.OldValue.ShouldEqual(oldValue);
            value.NewValue.ShouldEqual(newValue);
        }

        [Fact]
        public void IsEmptyShouldBeTrueDefault()
        {
            EntityStateValue v = default;
            v.IsEmpty.ShouldBeTrue();
        }
    }
}