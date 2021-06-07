using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Extensions;
using MugenMvvm.Validation;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Validation
{
    public class ValidationRuleBuilderTest : UnitTestBase
    {
        [Fact]
        public async Task AddAsyncValidatorShouldUseDelegate()
        {
            var memberName = "Test";
            var target = new object();
            var value = "";
            var error = "error";
            var errorTcs = new TaskCompletionSource<object?>();
            var invokeCount = 0;
            var rule = new ValidationRuleBuilder<object>().AddAsyncValidator(memberName, o => value, this, (o, s, state, ct, m) =>
            {
                ++invokeCount;
                o.ShouldEqual(target);
                s.ShouldEqual(value);
                ct.ShouldEqual(DefaultCancellationToken);
                state.ShouldEqual(this);
                m.ShouldEqual(DefaultMetadata);
                return errorTcs.Task;
            }).Build().Item!;
            rule.IsAsync.ShouldBeTrue();


            var task = rule.ValidateAsync(target, memberName, DefaultCancellationToken, DefaultMetadata);
            invokeCount.ShouldEqual(1);
            task.IsCompleted.ShouldBeFalse();
            errorTcs.TrySetResult(error);
            var errors = await task;
            task.IsCompleted.ShouldBeTrue();
            errors.Count.ShouldEqual(1);
            errors.Contains(new ValidationErrorInfo(target, memberName, error)).ShouldBeTrue();

            errorTcs = new TaskCompletionSource<object?>();
            error = null;
            task = rule.ValidateAsync(target, memberName, DefaultCancellationToken, DefaultMetadata);
            invokeCount.ShouldEqual(2);
            task.IsCompleted.ShouldBeFalse();
            errorTcs.TrySetResult(error);
            errors = await task;
            task.IsCompleted.ShouldBeTrue();
            errors.Count.ShouldEqual(1);
            errors.Contains(new ValidationErrorInfo(target, memberName, null)).ShouldBeTrue();
        }

        [Fact]
        public async Task AddValidatorShouldUseDelegate()
        {
            var memberName = "Test";
            var target = new object();
            var value = "";
            var error = "error";
            var invokeCount = 0;
            var rule = new ValidationRuleBuilder<object>().AddValidator(memberName, o => value, this, (o, s, state, m) =>
            {
                ++invokeCount;
                o.ShouldEqual(target);
                s.ShouldEqual(value);
                state.ShouldEqual(this);
                m.ShouldEqual(DefaultMetadata);
                return error;
            }).Build().Item!;
            rule.IsAsync.ShouldBeFalse();

            var errors = await rule.ValidateAsync(target, memberName, default, DefaultMetadata);
            invokeCount.ShouldEqual(1);
            errors.Count.ShouldEqual(1);
            errors.Contains(new ValidationErrorInfo(target, memberName, error));

            error = null;
            errors = await rule.ValidateAsync(target, memberName, default, DefaultMetadata);
            invokeCount.ShouldEqual(2);
            errors.Count.ShouldEqual(1);
            errors.Contains(new ValidationErrorInfo(target, memberName, null)).ShouldBeTrue();
        }

        [Fact]
        public void ForExtensionShouldBeCorrect1()
        {
            var validationModel = new ValidationModel {Property = "G"};
            var builder = new ValidationRuleBuilder<ValidationModel>();
            var memberBuilder = builder.For(model => model.Property, ReflectionManager);
            memberBuilder.MemberName.ShouldEqual(nameof(ValidationModel.Property));
            memberBuilder.Accessor(validationModel).ShouldEqual(validationModel.Property);
        }

        [Fact]
        public void ForExtensionShouldBeCorrect2()
        {
            var validationModel = new ValidationModel {Property = "G"};
            var builder = new ValidationRuleBuilder<ValidationModel>();
            var memberBuilder = builder.For(nameof(validationModel.Property), model => model.Property);
            memberBuilder.MemberName.ShouldEqual(nameof(ValidationModel.Property));
            memberBuilder.Accessor(validationModel).ShouldEqual(validationModel.Property);
        }

        [Fact]
        public async Task MustAsyncExtensionShouldBeCorrect()
        {
            var dpMember = "d";
            var error = "error";
            var canValidate = true;
            var propertyName = nameof(ValidationModel.Property);
            var validationModel = new ValidationModel();
            var tcs = new TaskCompletionSource<bool>();
            var rule = new ValidationRuleBuilder<ValidationModel>()
                       .For(propertyName, model => model.Property).MustAsync((model, s, c, m) =>
                       {
                           model.ShouldEqual(validationModel);
                           s.ShouldEqual(validationModel.Property);
                           m.ShouldEqual(DefaultMetadata);
                           c.ShouldEqual(DefaultCancellationToken);
                           return tcs.Task;
                       }, () => error, (model, context) =>
                       {
                           model.ShouldEqual(validationModel);
                           context.ShouldEqual(DefaultMetadata);
                           return canValidate;
                       }, new[] {dpMember}).Build().Item!;

            var task = rule.ValidateAsync(validationModel, propertyName, DefaultCancellationToken, DefaultMetadata);
            task.IsCompleted.ShouldBeFalse();
            tcs.TrySetResult(false);
            var errors = await task;
            task.IsCompleted.ShouldBeTrue();
            errors.Count.ShouldEqual(1);
            errors.Contains(new ValidationErrorInfo(validationModel, propertyName, error)).ShouldBeTrue();

            tcs = new TaskCompletionSource<bool>();
            task = rule.ValidateAsync(validationModel, dpMember, DefaultCancellationToken, DefaultMetadata);
            task.IsCompleted.ShouldBeFalse();
            tcs.TrySetResult(false);
            await task;
            task.IsCompleted.ShouldBeTrue();
            errors.Count.ShouldEqual(1);
            errors.Contains(new ValidationErrorInfo(validationModel, propertyName, error)).ShouldBeTrue();

            canValidate = false;
            tcs = new TaskCompletionSource<bool>();
            errors = await rule.ValidateAsync(validationModel, propertyName, DefaultCancellationToken, DefaultMetadata);
            errors.Count.ShouldEqual(1);
            errors.Contains(new ValidationErrorInfo(validationModel, propertyName, null)).ShouldBeTrue();

            canValidate = true;
            validationModel.Property = "test";
            task = rule.ValidateAsync(validationModel, propertyName, DefaultCancellationToken, DefaultMetadata);
            task.IsCompleted.ShouldBeFalse();
            tcs.TrySetResult(true);
            errors = await task;
            task.IsCompleted.ShouldBeTrue();
            errors.Count.ShouldEqual(1);
            errors.Contains(new ValidationErrorInfo(validationModel, propertyName, null)).ShouldBeTrue();
        }

        [Fact]
        public async Task MustExtensionShouldBeCorrect()
        {
            var dpMember = "d";
            var error = "error";
            var canValidate = true;
            var propertyName = nameof(ValidationModel.Property);
            var validationModel = new ValidationModel();
            var rule = new ValidationRuleBuilder<ValidationModel>()
                       .For(propertyName, model => model.Property).Must((model, s, m) =>
                       {
                           m.ShouldEqual(DefaultMetadata);
                           return !string.IsNullOrEmpty(model.Property) && !string.IsNullOrEmpty(s);
                       }, () => error, (model, context) =>
                       {
                           model.ShouldEqual(validationModel);
                           context.ShouldEqual(DefaultMetadata);
                           return canValidate;
                       }, new[] {dpMember}).Build().Item!;


            var errors = await rule.ValidateAsync(validationModel, propertyName, default, DefaultMetadata);
            errors.Count.ShouldEqual(1);
            errors.Contains(new ValidationErrorInfo(validationModel, propertyName, error)).ShouldBeTrue();

            errors = await rule.ValidateAsync(validationModel, dpMember, default, DefaultMetadata);
            errors.Count.ShouldEqual(1);
            errors.Contains(new ValidationErrorInfo(validationModel, propertyName, error)).ShouldBeTrue();

            canValidate = false;
            errors = await rule.ValidateAsync(validationModel, propertyName, default, DefaultMetadata);
            errors.Count.ShouldEqual(1);
            errors.Contains(new ValidationErrorInfo(validationModel, propertyName, null)).ShouldBeTrue();

            canValidate = true;
            validationModel.Property = "test";
            errors = await rule.ValidateAsync(validationModel, propertyName, default, DefaultMetadata);
            errors.Count.ShouldEqual(1);
            errors.Contains(new ValidationErrorInfo(validationModel, propertyName, null)).ShouldBeTrue();
        }

        [Fact]
        public async Task ValidatorShouldUseCondition()
        {
            var memberName = "Test";
            var target = new object();
            var value = "";
            var error = "error";
            var invokeCount = 0;
            var canValidate = true;
            var rule = new ValidationRuleBuilder<object>().AddValidator(memberName, o => value, this, (o, s, state, m) =>
            {
                ++invokeCount;
                o.ShouldEqual(target);
                s.ShouldEqual(value);
                state.ShouldEqual(this);
                m.ShouldEqual(DefaultMetadata);
                return error;
            }, (o, state, m) =>
            {
                o.ShouldEqual(target);
                state.ShouldEqual(this);
                m.ShouldEqual(DefaultMetadata);
                return canValidate;
            }).Build().Item!;


            var errors = await rule.ValidateAsync(target, memberName, default, DefaultMetadata);
            invokeCount.ShouldEqual(1);
            errors.Count.ShouldEqual(1);
            errors.Contains(new ValidationErrorInfo(target, memberName, error)).ShouldBeTrue();

            canValidate = false;
            errors = await rule.ValidateAsync(target, memberName, default, DefaultMetadata);
            errors.Count.ShouldEqual(1);
            errors.Contains(new ValidationErrorInfo(target, memberName, null)).ShouldBeTrue();
        }

        [Fact]
        public async Task ValidatorShouldUseDependencyMembers()
        {
            var dMember = "DM";
            var memberName = "Test";
            var target = new object();
            var value = "";
            var error = "error";
            var invokeCount = 0;
            var rule = new ValidationRuleBuilder<object>().AddValidator(memberName, o => value, this, (o, s, state, m) =>
            {
                ++invokeCount;
                o.ShouldEqual(target);
                s.ShouldEqual(value);
                state.ShouldEqual(this);
                m.ShouldEqual(DefaultMetadata);
                return error;
            }, null, new[] {dMember}).Build().Item!;


            var errors = await rule.ValidateAsync(target, memberName, default, DefaultMetadata);
            invokeCount.ShouldEqual(1);
            errors.Count.ShouldEqual(1);
            errors.Contains(new ValidationErrorInfo(target, memberName, error)).ShouldBeTrue();

            errors = await rule.ValidateAsync(target, dMember, default, DefaultMetadata);
            invokeCount.ShouldEqual(2);
            errors.Count.ShouldEqual(1);
            errors.Contains(new ValidationErrorInfo(target, memberName, error)).ShouldBeTrue();

            errors = await rule.ValidateAsync(target, "", default, DefaultMetadata);
            invokeCount.ShouldEqual(3);
            errors.Count.ShouldEqual(1);
            errors.Contains(new ValidationErrorInfo(target, memberName, error)).ShouldBeTrue();

            errors = await rule.ValidateAsync(target, "e", default, DefaultMetadata);
            invokeCount.ShouldEqual(3);
            errors.Count.ShouldEqual(0);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task EmptyExtensionShouldBeCorrect1(bool delegateError)
        {
            var error = "error";
            var propertyName = nameof(ValidationModel.Property);
            var validationModel = new ValidationModel();
            var rule = delegateError
                ? new ValidationRuleBuilder<ValidationModel>().For(propertyName, model => model.Property).Empty(() => error).Build().Item!
                : new ValidationRuleBuilder<ValidationModel>().For(propertyName, model => model.Property).Empty(error).Build().Item!;

            validationModel.Property = "test";
            var errors = await rule.ValidateAsync(validationModel, propertyName, default, null);
            errors.Count.ShouldEqual(1);
            errors.Contains(new ValidationErrorInfo(validationModel, propertyName, error)).ShouldBeTrue();

            validationModel.Property = "";
            errors = await rule.ValidateAsync(validationModel, propertyName, default, null);
            errors.Count.ShouldEqual(1);
            errors.Contains(new ValidationErrorInfo(validationModel, propertyName, null)).ShouldBeTrue();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task EmptyExtensionShouldBeCorrect2(bool delegateError)
        {
            var error = "error";
            var propertyName = nameof(ValidationModel.Property);
            var value = 1;
            var validationModel = new ValidationModel();
            var rule = delegateError
                ? new ValidationRuleBuilder<ValidationModel>().For(propertyName, model => value).Empty(() => error).Build().Item!
                : new ValidationRuleBuilder<ValidationModel>().For(propertyName, model => value).Empty(error).Build().Item!;

            var errors = await rule.ValidateAsync(validationModel, propertyName, default, null);
            errors.Count.ShouldEqual(1);
            errors.Contains(new ValidationErrorInfo(validationModel, propertyName, error)).ShouldBeTrue();

            value = 0;
            errors = await rule.ValidateAsync(validationModel, propertyName, default, null);
            errors.Count.ShouldEqual(1);
            errors.Contains(new ValidationErrorInfo(validationModel, propertyName, null)).ShouldBeTrue();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task EmptyExtensionShouldBeCorrect3(bool delegateError)
        {
            var error = "error";
            var propertyName = nameof(ValidationModel.Property);
            var value = new[] {""};
            var validationModel = new ValidationModel();
            var rule = delegateError
                ? new ValidationRuleBuilder<ValidationModel>().For(propertyName, model => value).Empty(() => error).Build().Item!
                : new ValidationRuleBuilder<ValidationModel>().For(propertyName, model => value).Empty(error).Build().Item!;

            var errors = await rule.ValidateAsync(validationModel, propertyName, default, null);
            errors.Count.ShouldEqual(1);
            errors.Contains(new ValidationErrorInfo(validationModel, propertyName, error)).ShouldBeTrue();

            value = new string[0];
            errors = await rule.ValidateAsync(validationModel, propertyName, default, null);
            errors.Count.ShouldEqual(1);
            errors.Contains(new ValidationErrorInfo(validationModel, propertyName, null)).ShouldBeTrue();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task LengthExtensionShouldBeCorrect(bool delegateError)
        {
            var error = "error";
            var propertyName = nameof(ValidationModel.Property);
            var validationModel = new ValidationModel();
            var rule = delegateError
                ? new ValidationRuleBuilder<ValidationModel>().For(propertyName, model => model.Property).Length(1, 5, () => error).Build().Item!
                : new ValidationRuleBuilder<ValidationModel>().For(propertyName, model => model.Property).Length(1, 5, error).Build().Item!;

            var errors = await rule.ValidateAsync(validationModel, propertyName, default, null);
            errors.Count.ShouldEqual(1);
            errors.Contains(new ValidationErrorInfo(validationModel, propertyName, error)).ShouldBeTrue();

            validationModel.Property = "testtest";
            errors = await rule.ValidateAsync(validationModel, propertyName, default, null);
            errors.Count.ShouldEqual(1);
            errors.Contains(new ValidationErrorInfo(validationModel, propertyName, error)).ShouldBeTrue();

            validationModel.Property = "test";
            errors = await rule.ValidateAsync(validationModel, propertyName, default, null);
            errors.Count.ShouldEqual(1);
            errors.Contains(new ValidationErrorInfo(validationModel, propertyName, null)).ShouldBeTrue();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task NotEmptyExtensionShouldBeCorrect1(bool delegateError)
        {
            var error = "error";
            var propertyName = nameof(ValidationModel.Property);
            var validationModel = new ValidationModel();
            var rule = delegateError
                ? new ValidationRuleBuilder<ValidationModel>().For(propertyName, model => model.Property).NotEmpty(() => error).Build().Item!
                : new ValidationRuleBuilder<ValidationModel>().For(propertyName, model => model.Property).NotEmpty(error).Build().Item!;

            validationModel.Property = "";
            var errors = await rule.ValidateAsync(validationModel, propertyName, default, null);
            errors.Count.ShouldEqual(1);
            errors.Contains(new ValidationErrorInfo(validationModel, propertyName, error)).ShouldBeTrue();

            validationModel.Property = "test";
            errors = await rule.ValidateAsync(validationModel, propertyName, default, null);
            errors.Count.ShouldEqual(1);
            errors.Contains(new ValidationErrorInfo(validationModel, propertyName, null)).ShouldBeTrue();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task NotEmptyExtensionShouldBeCorrect2(bool delegateError)
        {
            var error = "error";
            var propertyName = nameof(ValidationModel.Property);
            var value = 0;
            var validationModel = new ValidationModel();
            var rule = delegateError
                ? new ValidationRuleBuilder<ValidationModel>().For(propertyName, model => value).NotEmpty(() => error).Build().Item!
                : new ValidationRuleBuilder<ValidationModel>().For(propertyName, model => value).NotEmpty(error).Build().Item!;

            var errors = await rule.ValidateAsync(validationModel, propertyName, default, null);
            errors.Count.ShouldEqual(1);
            errors.Contains(new ValidationErrorInfo(validationModel, propertyName, error)).ShouldBeTrue();

            value = 1;
            errors = await rule.ValidateAsync(validationModel, propertyName, default, null);
            errors.Count.ShouldEqual(1);
            errors.Contains(new ValidationErrorInfo(validationModel, propertyName, null)).ShouldBeTrue();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task NotEmptyExtensionShouldBeCorrect3(bool delegateError)
        {
            var error = "error";
            var propertyName = nameof(ValidationModel.Property);
            var value = new string[0];
            var validationModel = new ValidationModel();
            var rule = delegateError
                ? new ValidationRuleBuilder<ValidationModel>().For(propertyName, model => value).NotEmpty(() => error).Build().Item!
                : new ValidationRuleBuilder<ValidationModel>().For(propertyName, model => value).NotEmpty(error).Build().Item!;

            var errors = await rule.ValidateAsync(validationModel, propertyName, default, null);
            errors.Count.ShouldEqual(1);
            errors.Contains(new ValidationErrorInfo(validationModel, propertyName, error)).ShouldBeTrue();

            value = new[] {""};
            errors = await rule.ValidateAsync(validationModel, propertyName, default, null);
            errors.Count.ShouldEqual(1);
            errors.Contains(new ValidationErrorInfo(validationModel, propertyName, null)).ShouldBeTrue();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task NotNullExtensionShouldBeCorrect(bool delegateError)
        {
            var error = "error";
            var propertyName = nameof(ValidationModel.Property);
            var validationModel = new ValidationModel();
            var rule = delegateError
                ? new ValidationRuleBuilder<ValidationModel>().For(propertyName, model => model.Property).NotNull(() => error).Build().Item!
                : new ValidationRuleBuilder<ValidationModel>().For(propertyName, model => model.Property).NotNull(error).Build().Item!;

            var errors = await rule.ValidateAsync(validationModel, propertyName, default, null);
            errors.Count.ShouldEqual(1);
            errors.Contains(new ValidationErrorInfo(validationModel, propertyName, error)).ShouldBeTrue();

            validationModel.Property = "test";
            errors = await rule.ValidateAsync(validationModel, propertyName, default, null);
            errors.Count.ShouldEqual(1);
            errors.Contains(new ValidationErrorInfo(validationModel, propertyName, null)).ShouldBeTrue();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task NullExtensionShouldBeCorrect(bool delegateError)
        {
            var error = "error";
            var propertyName = nameof(ValidationModel.Property);
            var validationModel = new ValidationModel();
            var rule = delegateError
                ? new ValidationRuleBuilder<ValidationModel>().For(propertyName, model => model.Property).Null(() => error).Build().Item!
                : new ValidationRuleBuilder<ValidationModel>().For(propertyName, model => model.Property).Null(error).Build().Item!;

            validationModel.Property = "test";
            var errors = await rule.ValidateAsync(validationModel, propertyName, default, null);
            errors.Count.ShouldEqual(1);
            errors.Contains(new ValidationErrorInfo(validationModel, propertyName, error)).ShouldBeTrue();

            validationModel.Property = null;
            errors = await rule.ValidateAsync(validationModel, propertyName, default, null);
            errors.Count.ShouldEqual(1);
            errors.Contains(new ValidationErrorInfo(validationModel, propertyName, null)).ShouldBeTrue();
        }

        private sealed class ValidationModel
        {
            public string? Property { get; set; }
        }
    }
}