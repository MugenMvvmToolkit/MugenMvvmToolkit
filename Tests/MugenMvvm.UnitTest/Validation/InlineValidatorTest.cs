using MugenMvvm.Validation;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Validation
{
    public class InlineValidatorTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void SetErrorsShouldUpdateErrors()
        {
            var memberName = "test";
            var errors = new object[] {"test"};
            var validator = new InlineValidator();
            validator.HasErrors.ShouldBeFalse();

            validator.SetErrors(memberName, errors);
            validator.HasErrors.ShouldBeTrue();

            validator.GetErrors(memberName).ShouldContain(errors);

            validator.SetErrors(memberName, null, DefaultMetadata);
            validator.HasErrors.ShouldBeFalse();
            validator.GetErrors().ShouldBeEmpty();
        }

        #endregion
    }
}