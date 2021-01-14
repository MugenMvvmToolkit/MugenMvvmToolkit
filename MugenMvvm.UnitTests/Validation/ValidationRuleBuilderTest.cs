using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
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
            var cts = new CancellationTokenSource();
            var rule = new ValidationRuleBuilder<object>().AddAsyncValidator(memberName, o => value, this, (o, s, state, ct, m) =>
            {
                ++invokeCount;
                o.ShouldEqual(target);
                s.ShouldEqual(value);
                ct.ShouldEqual(cts.Token);
                state.ShouldEqual(this);
                m.ShouldEqual(DefaultMetadata);
                return errorTcs.Task;
            }).Build().Item!;
            rule.IsAsync.ShouldBeTrue();

            var errors = new Dictionary<string, object?>();
            var task = rule.ValidateAsync(target, memberName, errors, cts.Token, DefaultMetadata)!;
            invokeCount.ShouldEqual(1);
            task.IsCompleted.ShouldBeFalse();
            errorTcs.TrySetResult(error);
            await task;
            task.IsCompleted.ShouldBeTrue();
            errors.Count.ShouldEqual(1);
            errors[memberName].ShouldEqual(error);

            errorTcs = new TaskCompletionSource<object?>();
            task = rule.ValidateAsync(target, memberName, errors, cts.Token, DefaultMetadata)!;
            invokeCount.ShouldEqual(2);
            task.IsCompleted.ShouldBeFalse();
            errorTcs.TrySetResult(error);
            await task;
            task.IsCompleted.ShouldBeTrue();
            errors.Count.ShouldEqual(1);
            ((IEnumerable<object>) errors[memberName]!).ShouldEqual(new[] {error, error});

            errorTcs = new TaskCompletionSource<object?>();
            error = null;
            errors.Clear();
            task = rule.ValidateAsync(target, memberName, errors, cts.Token, DefaultMetadata)!;
            invokeCount.ShouldEqual(3);
            task.IsCompleted.ShouldBeFalse();
            errorTcs.TrySetResult(error);
            await task;
            task.IsCompleted.ShouldBeTrue();
            errors.Count.ShouldEqual(0);
        }

        [Fact]
        public void AddValidatorShouldUseDelegate()
        {
            var memberName = "Test";
            var target = new object();
            var value = "";
            var error = "error";
            var invokeCount = 0;
            var builder = new ValidationRuleBuilder<object>();
            var rule = builder.AddValidator(memberName, o => value, this, (o, s, state, m) =>
            {
                ++invokeCount;
                o.ShouldEqual(target);
                s.ShouldEqual(value);
                state.ShouldEqual(this);
                m.ShouldEqual(DefaultMetadata);
                return error;
            }).Build().Item!;
            rule.IsAsync.ShouldBeFalse();

            var errors = new Dictionary<string, object?>();
            rule.ValidateAsync(target, memberName, errors, default, DefaultMetadata).ShouldEqual(Default.CompletedTask);
            invokeCount.ShouldEqual(1);
            errors.Count.ShouldEqual(1);
            errors[memberName].ShouldEqual(error);

            rule.ValidateAsync(target, memberName, errors, default, DefaultMetadata).ShouldEqual(Default.CompletedTask);
            invokeCount.ShouldEqual(2);
            errors.Count.ShouldEqual(1);
            ((IEnumerable<object>) errors[memberName]!).ShouldEqual(new[] {error, error});

            error = null;
            errors.Clear();
            rule.ValidateAsync(target, memberName, errors, default, DefaultMetadata).ShouldEqual(Default.CompletedTask);
            invokeCount.ShouldEqual(3);
            errors.Count.ShouldEqual(0);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void EmptyExtensionShouldBeCorrect1(bool delegateError)
        {
            var error = "error";
            var propertyName = nameof(ValidationModel.Property);
            var validationModel = new ValidationModel();
            var rule = delegateError
                ? new ValidationRuleBuilder<ValidationModel>().For(propertyName, model => model.Property).Empty(() => error).Build().Item!
                : new ValidationRuleBuilder<ValidationModel>().For(propertyName, model => model.Property).Empty(error).Build().Item!;

            validationModel.Property = "test";
            var errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, null).ShouldEqual(Default.CompletedTask);
            errors.Count.ShouldEqual(1);
            errors[propertyName].ShouldEqual(error);

            validationModel.Property = "";
            errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, null).ShouldEqual(Default.CompletedTask);
            errors.Count.ShouldEqual(0);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void EmptyExtensionShouldBeCorrect2(bool delegateError)
        {
            var error = "error";
            var propertyName = nameof(ValidationModel.Property);
            var value = 1;
            var validationModel = new ValidationModel();
            var rule = delegateError
                ? new ValidationRuleBuilder<ValidationModel>().For(propertyName, model => value).Empty(() => error).Build().Item!
                : new ValidationRuleBuilder<ValidationModel>().For(propertyName, model => value).Empty(error).Build().Item!;

            var errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, null).ShouldEqual(Default.CompletedTask);
            errors.Count.ShouldEqual(1);
            errors[propertyName].ShouldEqual(error);

            value = 0;
            errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, null).ShouldEqual(Default.CompletedTask);
            errors.Count.ShouldEqual(0);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void EmptyExtensionShouldBeCorrect3(bool delegateError)
        {
            var error = "error";
            var propertyName = nameof(ValidationModel.Property);
            var value = new[] {""};
            var validationModel = new ValidationModel();
            var rule = delegateError
                ? new ValidationRuleBuilder<ValidationModel>().For(propertyName, model => value).Empty(() => error).Build().Item!
                : new ValidationRuleBuilder<ValidationModel>().For(propertyName, model => value).Empty(error).Build().Item!;

            var errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, null).ShouldEqual(Default.CompletedTask);
            errors.Count.ShouldEqual(1);
            errors[propertyName].ShouldEqual(error);

            value = new string[0];
            errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, null).ShouldEqual(Default.CompletedTask);
            errors.Count.ShouldEqual(0);
        }

        [Fact]
        public void ForExtensionShouldBeCorrect1()
        {
            var validationModel = new ValidationModel {Property = "G"};
            var builder = new ValidationRuleBuilder<ValidationModel>();
            var memberBuilder = builder.For(model => model.Property);
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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void LengthExtensionShouldBeCorrect(bool delegateError)
        {
            var error = "error";
            var propertyName = nameof(ValidationModel.Property);
            var validationModel = new ValidationModel();
            var rule = delegateError
                ? new ValidationRuleBuilder<ValidationModel>().For(propertyName, model => model.Property).Length(1, 5, () => error).Build().Item!
                : new ValidationRuleBuilder<ValidationModel>().For(propertyName, model => model.Property).Length(1, 5, error).Build().Item!;

            var errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, null).ShouldEqual(Default.CompletedTask);
            errors.Count.ShouldEqual(1);
            errors[propertyName].ShouldEqual(error);

            validationModel.Property = "testtest";
            errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, null).ShouldEqual(Default.CompletedTask);
            errors.Count.ShouldEqual(1);
            errors[propertyName].ShouldEqual(error);

            validationModel.Property = "test";
            errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, null).ShouldEqual(Default.CompletedTask);
            errors.Count.ShouldEqual(0);
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
            var cts = new CancellationTokenSource();
            var rule = new ValidationRuleBuilder<ValidationModel>()
                       .For(propertyName, model => model.Property).MustAsync((model, s, c, m) =>
                       {
                           model.ShouldEqual(validationModel);
                           s.ShouldEqual(validationModel.Property);
                           m.ShouldEqual(DefaultMetadata);
                           c.ShouldEqual(cts.Token);
                           return tcs.Task;
                       }, () => error, (model, context) =>
                       {
                           model.ShouldEqual(validationModel);
                           context.ShouldEqual(DefaultMetadata);
                           return canValidate;
                       }, new[] {dpMember}).Build().Item!;

            var errors = new Dictionary<string, object?>();
            var task = rule.ValidateAsync(validationModel, propertyName, errors, cts.Token, DefaultMetadata)!;
            task.IsCompleted.ShouldBeFalse();
            tcs.TrySetResult(false);
            await task;
            task.IsCompleted.ShouldBeTrue();
            errors.Count.ShouldEqual(1);
            errors[propertyName].ShouldEqual(error);

            tcs = new TaskCompletionSource<bool>();
            errors = new Dictionary<string, object?>();
            task = rule.ValidateAsync(validationModel, dpMember, errors, cts.Token, DefaultMetadata)!;
            task.IsCompleted.ShouldBeFalse();
            tcs.TrySetResult(false);
            await task;
            task.IsCompleted.ShouldBeTrue();
            errors.Count.ShouldEqual(1);
            errors[propertyName].ShouldEqual(error);

            canValidate = false;
            tcs = new TaskCompletionSource<bool>();
            errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, cts.Token, DefaultMetadata).ShouldEqual(Default.CompletedTask);
            errors.Count.ShouldEqual(0);

            canValidate = true;
            validationModel.Property = "test";
            errors = new Dictionary<string, object?>();
            task = rule.ValidateAsync(validationModel, propertyName, errors, cts.Token, DefaultMetadata)!;
            task.IsCompleted.ShouldBeFalse();
            tcs.TrySetResult(true);
            await task;
            task.IsCompleted.ShouldBeTrue();
            errors.Count.ShouldEqual(0);
        }

        [Fact]
        public void MustExtensionShouldBeCorrect()
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

            var errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, DefaultMetadata).ShouldEqual(Default.CompletedTask);
            errors.Count.ShouldEqual(1);
            errors[propertyName].ShouldEqual(error);

            errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, dpMember, errors, default, DefaultMetadata).ShouldEqual(Default.CompletedTask);
            errors.Count.ShouldEqual(1);
            errors[propertyName].ShouldEqual(error);

            canValidate = false;
            errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, DefaultMetadata).ShouldEqual(Default.CompletedTask);
            errors.Count.ShouldEqual(0);

            canValidate = true;
            validationModel.Property = "test";
            errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, DefaultMetadata).ShouldEqual(Default.CompletedTask);
            errors.Count.ShouldEqual(0);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void NotEmptyExtensionShouldBeCorrect1(bool delegateError)
        {
            var error = "error";
            var propertyName = nameof(ValidationModel.Property);
            var validationModel = new ValidationModel();
            var rule = delegateError
                ? new ValidationRuleBuilder<ValidationModel>().For(propertyName, model => model.Property).NotEmpty(() => error).Build().Item!
                : new ValidationRuleBuilder<ValidationModel>().For(propertyName, model => model.Property).NotEmpty(error).Build().Item!;

            validationModel.Property = "";
            var errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, null).ShouldEqual(Default.CompletedTask);
            errors.Count.ShouldEqual(1);
            errors[propertyName].ShouldEqual(error);

            validationModel.Property = "test";
            errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, null).ShouldEqual(Default.CompletedTask);
            errors.Count.ShouldEqual(0);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void NotEmptyExtensionShouldBeCorrect2(bool delegateError)
        {
            var error = "error";
            var propertyName = nameof(ValidationModel.Property);
            var value = 0;
            var validationModel = new ValidationModel();
            var rule = delegateError
                ? new ValidationRuleBuilder<ValidationModel>().For(propertyName, model => value).NotEmpty(() => error).Build().Item!
                : new ValidationRuleBuilder<ValidationModel>().For(propertyName, model => value).NotEmpty(error).Build().Item!;

            var errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, null).ShouldEqual(Default.CompletedTask);
            errors.Count.ShouldEqual(1);
            errors[propertyName].ShouldEqual(error);

            value = 1;
            errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, null).ShouldEqual(Default.CompletedTask);
            errors.Count.ShouldEqual(0);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void NotEmptyExtensionShouldBeCorrect3(bool delegateError)
        {
            var error = "error";
            var propertyName = nameof(ValidationModel.Property);
            var value = new string[0];
            var validationModel = new ValidationModel();
            var rule = delegateError
                ? new ValidationRuleBuilder<ValidationModel>().For(propertyName, model => value).NotEmpty(() => error).Build().Item!
                : new ValidationRuleBuilder<ValidationModel>().For(propertyName, model => value).NotEmpty(error).Build().Item!;

            var errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, null).ShouldEqual(Default.CompletedTask);
            errors.Count.ShouldEqual(1);
            errors[propertyName].ShouldEqual(error);

            value = new[] {""};
            errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, null).ShouldEqual(Default.CompletedTask);
            errors.Count.ShouldEqual(0);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void NotNullExtensionShouldBeCorrect(bool delegateError)
        {
            var error = "error";
            var propertyName = nameof(ValidationModel.Property);
            var validationModel = new ValidationModel();
            var rule = delegateError
                ? new ValidationRuleBuilder<ValidationModel>().For(propertyName, model => model.Property).NotNull(() => error).Build().Item!
                : new ValidationRuleBuilder<ValidationModel>().For(propertyName, model => model.Property).NotNull(error).Build().Item!;

            var errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, null).ShouldEqual(Default.CompletedTask);
            errors.Count.ShouldEqual(1);
            errors[propertyName].ShouldEqual(error);

            validationModel.Property = "test";
            errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, null).ShouldEqual(Default.CompletedTask);
            errors.Count.ShouldEqual(0);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void NullExtensionShouldBeCorrect(bool delegateError)
        {
            var error = "error";
            var propertyName = nameof(ValidationModel.Property);
            var validationModel = new ValidationModel();
            var rule = delegateError
                ? new ValidationRuleBuilder<ValidationModel>().For(propertyName, model => model.Property).Null(() => error).Build().Item!
                : new ValidationRuleBuilder<ValidationModel>().For(propertyName, model => model.Property).Null(error).Build().Item!;

            validationModel.Property = "test";
            var errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, null).ShouldEqual(Default.CompletedTask);
            errors.Count.ShouldEqual(1);
            errors[propertyName].ShouldEqual(error);

            validationModel.Property = null;
            errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, null).ShouldEqual(Default.CompletedTask);
            errors.Count.ShouldEqual(0);
        }

        [Fact]
        public void ValidatorShouldUseCondition()
        {
            var memberName = "Test";
            var target = new object();
            var value = "";
            var error = "error";
            var invokeCount = 0;
            var canValidate = true;
            var builder = new ValidationRuleBuilder<object>();
            var rule = builder.AddValidator(memberName, o => value, this, (o, s, state, m) =>
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

            var errors = new Dictionary<string, object?>();
            rule.ValidateAsync(target, memberName, errors, default, DefaultMetadata).ShouldEqual(Default.CompletedTask);
            invokeCount.ShouldEqual(1);
            errors.Count.ShouldEqual(1);
            errors[memberName].ShouldEqual(error);

            canValidate = false;
            errors = new Dictionary<string, object?>();
            rule.ValidateAsync(target, memberName, errors, default, DefaultMetadata).ShouldEqual(Default.CompletedTask);
            invokeCount.ShouldEqual(1);
            errors.Count.ShouldEqual(0);
        }

        [Fact]
        public void ValidatorShouldUseDependencyMembers()
        {
            var dMember = "DM";
            var memberName = "Test";
            var target = new object();
            var value = "";
            var error = "error";
            var invokeCount = 0;
            var builder = new ValidationRuleBuilder<object>();
            var rule = builder.AddValidator(memberName, o => value, this, (o, s, state, m) =>
            {
                ++invokeCount;
                o.ShouldEqual(target);
                s.ShouldEqual(value);
                state.ShouldEqual(this);
                m.ShouldEqual(DefaultMetadata);
                return error;
            }, null, new[] {dMember}).Build().Item!;

            var errors = new Dictionary<string, object?>();
            rule.ValidateAsync(target, memberName, errors, default, DefaultMetadata).ShouldEqual(Default.CompletedTask);
            invokeCount.ShouldEqual(1);
            errors.Count.ShouldEqual(1);
            errors[memberName].ShouldEqual(error);

            errors = new Dictionary<string, object?>();
            rule.ValidateAsync(target, dMember, errors, default, DefaultMetadata).ShouldEqual(Default.CompletedTask);
            invokeCount.ShouldEqual(2);
            errors.Count.ShouldEqual(1);
            errors[memberName].ShouldEqual(error);

            errors = new Dictionary<string, object?>();
            rule.ValidateAsync(target, "", errors, default, DefaultMetadata).ShouldEqual(Default.CompletedTask);
            invokeCount.ShouldEqual(3);
            errors.Count.ShouldEqual(1);
            errors[memberName].ShouldEqual(error);

            errors = new Dictionary<string, object?>();
            rule.ValidateAsync(target, "e", errors, default, DefaultMetadata).ShouldEqual(Default.CompletedTask);
            invokeCount.ShouldEqual(3);
            errors.Count.ShouldEqual(0);
        }

        private sealed class ValidationModel
        {
            public string? Property { get; set; }
        }
    }
}