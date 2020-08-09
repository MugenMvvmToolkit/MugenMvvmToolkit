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
            validator.HasErrors(null!).ShouldBeFalse();

            validator.SetErrors(memberName, errors);
            validator.HasErrors(null!, memberName).ShouldBeTrue();
            validator.HasErrors(null!).ShouldBeTrue();

            validator.TryGetErrors(null!, memberName).AsList().ShouldContain(errors);

            validator.SetErrors(memberName, null, DefaultMetadata);
            validator.HasErrors(null!, memberName).ShouldBeFalse();
            validator.HasErrors(null!).ShouldBeFalse();
            validator.TryGetErrors(null!).ShouldBeEmpty();
        }


        [Fact]
        public void SetErrorsShouldAddInlineValidator()
        {
            var validator = new Validator();
            validator.GetComponents<InlineValidatorComponent>().ShouldBeEmpty();

            var memberName = "test";
            var errors = new object[] {"test"};
            validator.HasErrors().ShouldBeFalse();

            validator.SetErrors(this, memberName, errors);
            validator.HasErrors(memberName).ShouldBeTrue();
            validator.HasErrors().ShouldBeTrue();
            validator.GetErrors(memberName).AsList().ShouldContain(errors);

            validator.SetErrors(this, memberName, null, DefaultMetadata);
            validator.HasErrors(memberName).ShouldBeFalse();
            validator.HasErrors().ShouldBeFalse();
            validator.GetErrors().ShouldBeEmpty();
            validator.GetComponents<InlineValidatorComponent>().Length.ShouldEqual(1);

            validator.SetErrors(validator, memberName, errors);
            validator.HasErrors(memberName).ShouldBeTrue();
            validator.HasErrors().ShouldBeTrue();
            validator.GetErrors(memberName).AsList().ShouldContain(errors);
            validator.GetComponents<InlineValidatorComponent>().Length.ShouldEqual(2);

            validator.SetErrors(validator, memberName, null, DefaultMetadata);
            validator.HasErrors(memberName).ShouldBeFalse();
            validator.HasErrors().ShouldBeFalse();
            validator.GetErrors().ShouldBeEmpty();
            validator.GetComponents<InlineValidatorComponent>().Length.ShouldEqual(2);
        }

        #endregion
    }
}