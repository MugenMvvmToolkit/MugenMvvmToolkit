using MugenMvvm.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Internal
{
    public class TypeCheckerTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ShouldReturnCorrectValues()
        {
            TypeChecker.IsNullable<object>().ShouldBeTrue();
            TypeChecker.IsNullable<bool?>().ShouldBeTrue();
            TypeChecker.IsNullable<bool>().ShouldBeFalse();
            TypeChecker.IsValueType<bool>().ShouldBeTrue();
            TypeChecker.IsValueType<object>().ShouldBeFalse();
        }

        #endregion
    }
}