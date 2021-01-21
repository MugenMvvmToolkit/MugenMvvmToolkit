using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Validation.Internal;
using MugenMvvm.Validation;
using MugenMvvm.Validation.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Validation.Components
{
    public class RuleValidationHandlerTest : UnitTestBase
    {
        [Fact]
        public async Task ValidateShouldUseRules1()
        {
            var member = "Test2";
            var rules = new[]
            {
                new TestValidationRule
                {
                    IsAsync = false,
                    ValidateAsync = (t, v, ct, m) =>
                    {
                        t.ShouldEqual(this);
                        ct.CanBeCanceled.ShouldBeFalse();
                        if (v == member)
                            return new ValueTask<ItemOrIReadOnlyList<ValidationErrorInfo>>(new ValidationErrorInfo(this, member, member));
                        return default;
                    }
                }
            };

            var validator = new Validator();
            var component = new RuleValidationHandler(this, rules);
            validator.AddComponent(component);
            validator.AddComponent(new ValidatorErrorManager());

            ItemOrListEditor<ValidationErrorInfo> errors = default;
            var validateAsync = validator.ValidateAsync(member);
            validateAsync.IsCompleted.ShouldBeTrue();
            await validateAsync;
            validator.GetErrors(member, ref errors);
            errors.Count.ShouldEqual(1);
            errors[0].ShouldEqual(new ValidationErrorInfo(this, member, member));
        }

        [Fact]
        public async Task ValidateShouldUseRules2()
        {
            var memberName1 = "Test1";
            var memberName2 = "Test2";
            var tcs = new TaskCompletionSource<object?>();
            var rules = new[]
            {
                new TestValidationRule
                {
                    IsAsync = true,
                    ValidateAsync = (t, v, ct, m) =>
                    {
                        t.ShouldEqual(this);
                        ct.CanBeCanceled.ShouldBeTrue();
                        if (v == memberName1)
                            return tcs.Task.ContinueWith(t => (ItemOrIReadOnlyList<ValidationErrorInfo>) new ValidationErrorInfo(this, memberName1, memberName1), ct).AsValueTask();

                        return default;
                    }
                },
                new TestValidationRule
                {
                    IsAsync = false,
                    ValidateAsync = (t, v, ct, m) =>
                    {
                        t.ShouldEqual(this);
                        ct.CanBeCanceled.ShouldBeTrue();
                        if (v == memberName2)
                            return new ValueTask<ItemOrIReadOnlyList<ValidationErrorInfo>>(new ValidationErrorInfo(this, memberName2, memberName2));
                        return default;
                    }
                }
            };

            var validator = new Validator();
            var component = new RuleValidationHandler(this, rules);
            validator.AddComponent(component);
            validator.AddComponent(new ValidatorErrorManager());

            ItemOrListEditor<ValidationErrorInfo> errors = default;
            var validateAsync = validator.ValidateAsync(memberName2);
            validateAsync.IsCompleted.ShouldBeTrue();
            await validateAsync;
            validator.GetErrors(memberName2, ref errors);
            errors.Count.ShouldEqual(1);
            errors[0].ShouldEqual(new ValidationErrorInfo(this, memberName2, memberName2));

            var task = validator.ValidateAsync(memberName1);
            task.IsCompleted.ShouldBeFalse();

            tcs.TrySetResult(null);
            await task;
            task.IsCompleted.ShouldBeTrue();
            errors.Clear();
            validator.GetErrors(memberName1, ref errors);
            errors[0].ShouldEqual(new ValidationErrorInfo(this, memberName1, memberName1));
        }
    }
}