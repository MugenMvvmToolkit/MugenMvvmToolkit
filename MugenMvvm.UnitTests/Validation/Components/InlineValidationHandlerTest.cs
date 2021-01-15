using MugenMvvm.Extensions;
using MugenMvvm.Validation;
using MugenMvvm.Validation.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Validation.Components
{
    public class InlineValidationHandlerTest : UnitTestBase
    {
        [Fact]
        public void SetErrorsShouldAddInlineValidator()
        {
            var validator = new Validator();
            validator.GetComponents<InlineValidationHandler>().ShouldBeEmpty();

            var memberName = "test";
            var errors = new object[] {"test"};
            validator.HasErrors().ShouldBeFalse();

            validator.SetErrors(this, memberName, errors);
            validator.HasErrors(memberName).ShouldBeTrue();
            validator.HasErrors().ShouldBeTrue();
            validator.GetErrors(memberName).AsList().ShouldContain(errors);

            validator.SetErrors(this, memberName, default, DefaultMetadata);
            validator.HasErrors(memberName).ShouldBeFalse();
            validator.HasErrors().ShouldBeFalse();
            validator.GetErrors().ShouldBeEmpty();
            validator.GetComponents<InlineValidationHandler>().Count.ShouldEqual(1);

            validator.SetErrors(validator, memberName, errors);
            validator.HasErrors(memberName).ShouldBeTrue();
            validator.HasErrors().ShouldBeTrue();
            validator.GetErrors(memberName).AsList().ShouldContain(errors);
            validator.GetComponents<InlineValidationHandler>().Count.ShouldEqual(2);

            validator.SetErrors(validator, memberName, default, DefaultMetadata);
            validator.HasErrors(memberName).ShouldBeFalse();
            validator.HasErrors().ShouldBeFalse();
            validator.GetErrors().ShouldBeEmpty();
            validator.GetComponents<InlineValidationHandler>().Count.ShouldEqual(2);
        }

        [Fact]
        public void SetErrorsShouldUpdateErrors()
        {
            var memberName = "test";
            var errors = new object[] {"test"};

            var validator = new InlineValidationHandler(this);
            validator.HasErrors(null!, null, null).ShouldBeFalse();

            validator.SetErrors(memberName, errors);
            validator.HasErrors(null!, memberName, null).ShouldBeTrue();
            validator.HasErrors(null!, null, null).ShouldBeTrue();

            validator.TryGetErrors(null!, memberName).AsList().ShouldContain(errors);

            validator.SetErrors(memberName, default, DefaultMetadata);
            validator.HasErrors(null!, memberName, null).ShouldBeFalse();
            validator.HasErrors(null!, null, null).ShouldBeFalse();
            validator.TryGetErrors(null!).ShouldBeEmpty();
        }
    }
}