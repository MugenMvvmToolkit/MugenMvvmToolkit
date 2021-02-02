using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Validation.Internal;
using MugenMvvm.Validation;
using MugenMvvm.Validation.Components;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Validation.Components
{
    public class RuleValidationHandlerTest : UnitTestBase
    {
        private readonly Validator _validator;

        public RuleValidationHandlerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _validator = new Validator(null, ComponentCollectionManager);
            _validator.AddComponent(new ValidatorErrorManager());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ValidateShouldUseRules1(bool useCache)
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


            var component = new RuleValidationHandler(this, rules, useCache);
            component.UseCache.ShouldEqual(useCache);
            _validator.AddComponent(component);

            ItemOrListEditor<ValidationErrorInfo> errors = default;
            var validateAsync = _validator.ValidateAsync(member);
            validateAsync.IsCompleted.ShouldBeTrue();
            await validateAsync;
            _validator.GetErrors(member, ref errors);
            errors.Count.ShouldEqual(1);
            errors[0].ShouldEqual(new ValidationErrorInfo(this, member, member));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ValidateShouldUseRules2(bool useCache)
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

            var component = new RuleValidationHandler(this, rules, useCache);
            component.UseCache.ShouldEqual(useCache);
            _validator.AddComponent(component);

            ItemOrListEditor<ValidationErrorInfo> errors = default;
            var validateAsync = _validator.ValidateAsync(memberName2);
            validateAsync.IsCompleted.ShouldBeTrue();
            await validateAsync;
            _validator.GetErrors(memberName2, ref errors);
            errors.Count.ShouldEqual(1);
            errors[0].ShouldEqual(new ValidationErrorInfo(this, memberName2, memberName2));

            var task = _validator.ValidateAsync(memberName1);
            task.IsCompleted.ShouldBeFalse();

            tcs.TrySetResult(null);
            await task;
            task.IsCompleted.ShouldBeTrue();
            errors.Clear();
            _validator.GetErrors(memberName1, ref errors);
            errors[0].ShouldEqual(new ValidationErrorInfo(this, memberName1, memberName1));
        }
    }
}