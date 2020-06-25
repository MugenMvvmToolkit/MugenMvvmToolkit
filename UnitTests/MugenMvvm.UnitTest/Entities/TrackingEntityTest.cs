using MugenMvvm.Entities;
using MugenMvvm.Enums;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Entities
{
    public class TrackingEntityTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void IsEmptyShouldBeTrueDefault()
        {
            TrackingEntity v = default;
            v.IsEmpty.ShouldBeTrue();
        }

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            var entity = new object();
            var state = EntityState.Added;
            var value = new TrackingEntity(entity, state);
            value.IsEmpty.ShouldBeFalse();
            value.Entity.ShouldEqual(entity);
            value.State.ShouldEqual(state);
        }

        #endregion
    }
}