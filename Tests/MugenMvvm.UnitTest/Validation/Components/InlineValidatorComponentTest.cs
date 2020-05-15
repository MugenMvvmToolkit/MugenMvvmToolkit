using MugenMvvm.Extensions;
using MugenMvvm.Validation;
using MugenMvvm.Validation.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Validation.Components
{
    public class InlineValidatorComponentTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void SetErrorsShouldUpdateErrors()
        {
            var memberName = "test";
            var errors = new object[] {"test"};
            var validator = new InlineValidatorComponent(this);
            validator.HasErrors.ShouldBeFalse();

            validator.SetErrors(memberName, errors);
            validator.HasErrors.ShouldBeTrue();

            validator.GetErrors(memberName).ShouldContain(errors);

            validator.SetErrors(memberName, null, DefaultMetadata);
            validator.HasErrors.ShouldBeFalse();
            validator.GetErrors().ShouldBeEmpty();
        }


        [Fact]
        public void SetErrorsShouldAddInlineValidator()
        {
            var validator = new Validator();
            validator.GetComponents<InlineValidatorComponent>().ShouldBeEmpty();

            var memberName = "test";
            var errors = new object[] {"test"};
            validator.HasErrors.ShouldBeFalse();

            validator.SetErrors(this, memberName, errors);
            validator.HasErrors.ShouldBeTrue();
            validator.GetErrors(memberName).ShouldContain(errors);

            validator.SetErrors(this, memberName, null, DefaultMetadata);
            validator.HasErrors.ShouldBeFalse();
            validator.GetErrors().ShouldBeEmpty();
            validator.GetComponents<InlineValidatorComponent>().Length.ShouldEqual(1);

            validator.SetErrors(validator, memberName, errors);
            validator.HasErrors.ShouldBeTrue();
            validator.GetErrors(memberName).ShouldContain(errors);
            validator.GetComponents<InlineValidatorComponent>().Length.ShouldEqual(2);

            validator.SetErrors(validator, memberName, null, DefaultMetadata);
            validator.HasErrors.ShouldBeFalse();
            validator.GetErrors().ShouldBeEmpty();
            validator.GetComponents<InlineValidatorComponent>().Length.ShouldEqual(2);
        }

        #endregion
    }
}