using System.Linq;
using System.Threading.Tasks;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
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
        public async Task ValidateShouldUseRules()
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

                        return Default.CompletedTask;
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
                        return Default.CompletedTask;
                    }
                }
            };

            var validator = new Validator();
            var component = new RuleValidatorComponent(this, rules);
            validator.AddComponent(component);

            validator.ValidateAsync(memberName2).IsCompleted.ShouldBeTrue();
            validator.GetErrors(memberName2).AsList().Single().ShouldEqual(memberName2);

            var task = validator.ValidateAsync(memberName1);
            task.IsCompleted.ShouldBeFalse();

            tcs.TrySetResult(null);
            await task;
            task.IsCompleted.ShouldBeTrue();
            validator.GetErrors(memberName1).AsList().Single().ShouldEqual(memberName1);
        }

        #endregion
    }
}