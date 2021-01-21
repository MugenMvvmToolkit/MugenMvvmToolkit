using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Validation.Internal;
using MugenMvvm.Validation;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Validation.Components
{
    public class ValidationHandlerBaseTest : UnitTestBase
    {
        [Fact]
        public async Task ShouldClearErrorsResultWithoutValidationMember()
        {
            var clearInvokeCount = 0;
            var setInvokeCount = 0;
            const string member = "test";
            ItemOrIReadOnlyList<ValidationErrorInfo> errors = default;
            var validator = new Validator();
            var handler = new TestValidationHandlerBase
            {
                Validate = (_, _, _, _) => new ValueTask<ItemOrIReadOnlyList<ValidationErrorInfo>>(errors)
            };
            validator.AddComponent(handler);
            validator.AddComponent(new TestValidatorErrorManagerComponent(validator)
            {
                ClearErrors = (list, o, m) =>
                {
                    ++clearInvokeCount;
                    o.ShouldEqual(handler);
                    list.ShouldEqual(member);
                    m.ShouldEqual(DefaultMetadata);
                },
                SetErrors = (o, list, m) =>
                {
                    ++setInvokeCount;
                    o.ShouldEqual(handler);
                    list.ShouldEqual(errors);
                    m.ShouldEqual(DefaultMetadata);
                },
                ResetErrors = (o, list, m) => throw new NotSupportedException()
            });

            await validator.ValidateAsync(member, CancellationToken.None, DefaultMetadata);
            setInvokeCount.ShouldEqual(1);
            clearInvokeCount.ShouldEqual(1);
        }

        [Fact]
        public async Task ShouldResetErrorsEmptyMember()
        {
            var invokeCount = 0;
            ItemOrIReadOnlyList<ValidationErrorInfo> errors = new ValidationErrorInfo(this, "test", "test");
            var validator = new Validator();
            var handler = new TestValidationHandlerBase
            {
                Validate = (_, _, _, _) => new ValueTask<ItemOrIReadOnlyList<ValidationErrorInfo>>(errors)
            };
            validator.AddComponent(handler);
            validator.AddComponent(new TestValidatorErrorManagerComponent(validator)
            {
                ClearErrors = (list, o, m) => throw new NotSupportedException(),
                SetErrors = (list, o, m) => throw new NotSupportedException(),
                ResetErrors = (o, list, m) =>
                {
                    ++invokeCount;
                    o.ShouldEqual(handler);
                    list.ShouldEqual(errors);
                    m.ShouldEqual(DefaultMetadata);
                }
            });

            await validator.ValidateAsync("", CancellationToken.None, DefaultMetadata);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public async Task ShouldSetErrorsResult()
        {
            var invokeCount = 0;
            const string member = "test";
            ItemOrIReadOnlyList<ValidationErrorInfo> errors = new ValidationErrorInfo(this, member, member);
            var validator = new Validator();
            var handler = new TestValidationHandlerBase
            {
                Validate = (_, _, _, _) => new ValueTask<ItemOrIReadOnlyList<ValidationErrorInfo>>(errors)
            };
            validator.AddComponent(handler);
            validator.AddComponent(new TestValidatorErrorManagerComponent(validator)
            {
                ClearErrors = (list, o, m) => throw new NotSupportedException(),
                ResetErrors = (list, o, m) => throw new NotSupportedException(),
                SetErrors = (o, list, m) =>
                {
                    ++invokeCount;
                    o.ShouldEqual(handler);
                    list.ShouldEqual(errors);
                    m.ShouldEqual(DefaultMetadata);
                }
            });

            await validator.ValidateAsync(member, CancellationToken.None, DefaultMetadata);
            invokeCount.ShouldEqual(1);
        }
    }
}