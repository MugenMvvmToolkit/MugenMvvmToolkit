using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Validation.Internal;
using MugenMvvm.Validation;
using MugenMvvm.Validation.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Validation.Components
{
    public class RuleValidatorComponentTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ConstructorShouldInitializeHasAsyncValidation()
        {
            new RuleValidatorComponent(this, new[]
            {
                new TestValidationRule {IsAsync = true},
                new TestValidationRule {IsAsync = false}
            }).HasAsyncValidation.ShouldBeTrue();

            new RuleValidatorComponent(this, new[]
            {
                new TestValidationRule {IsAsync = false},
                new TestValidationRule {IsAsync = false}
            }).HasAsyncValidation.ShouldBeFalse();
        }

        [Fact]
        public void ValidateShouldUseRules()
        {
            var memberName1 = "Test1";
            var memberName2 = "Test2";
            var tcs = new TaskCompletionSource<object?>();
            var rules = new[]
            {
                new TestValidationRule
                {
                    IsAsync = true,
                    ValidateAsync = (t, v, errors, ct, m) =>
                    {
                        t.ShouldEqual(this);
                        ct.CanBeCanceled.ShouldBeTrue();
                        if (v == memberName1)
                        {
                            errors[memberName1] = memberName1;
                            return tcs.Task;
                        }
                        return Task.CompletedTask;
                    }
                },
                new TestValidationRule
                {
                    IsAsync = false,
                    ValidateAsync = (t, v, errors, ct, m) =>
                    {
                        t.ShouldEqual(this);
                        ct.CanBeCanceled.ShouldBeTrue();
                        if (v == memberName2)
                            errors[memberName2] = memberName2;
                        return Task.CompletedTask;
                    }
                }
            };

            var validator = new Validator();
            var component = new RuleValidatorComponent(this, rules);
            validator.AddComponent(component);

            validator.ValidateAsync(memberName2).IsCompleted.ShouldBeTrue();
            validator.GetErrors(memberName2).Iterator().AsList().Single().ShouldEqual(memberName2);

            var task = validator.ValidateAsync(memberName1);
            task.IsCompleted.ShouldBeFalse();

            tcs.TrySetResult(null);
            Thread.Sleep(10);
            task.IsCompleted.ShouldBeTrue();
            validator.GetErrors(memberName1).Iterator().AsList().Single().ShouldEqual(memberName1);
        }

        #endregion
    }
}