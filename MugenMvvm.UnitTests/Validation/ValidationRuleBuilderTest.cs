using System;
using System.Collections.Generic;
using System.Linq;
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
        #region Methods

        [Fact]
        public void AddValidatorShouldUseDelegate()
        {
            var memberName = "Test";
            var target = new object();
            var value = "";
            var error = "error";
            var invokeCount = 0;
            var builder = ValidationRuleBuilder<object>.Get();
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
            rule.ValidateAsync(target, memberName, errors, default, DefaultMetadata).ShouldBeNull();
            invokeCount.ShouldEqual(1);
            errors.Count.ShouldEqual(1);
            errors[memberName].ShouldEqual(error);

            rule.ValidateAsync(target, memberName, errors, default, DefaultMetadata).ShouldBeNull();
            invokeCount.ShouldEqual(2);
            errors.Count.ShouldEqual(1);
            ((IEnumerable<object>) errors[memberName]!).SequenceEqual(new[] {error, error}).ShouldBeTrue();

            error = null;
            errors.Clear();
            rule.ValidateAsync(target, memberName, errors, default, DefaultMetadata).ShouldBeNull();
            invokeCount.ShouldEqual(3);
            errors.Count.ShouldEqual(0);
        }

        [Fact]
        public void AddAsyncValidatorShouldUseDelegate()
        {
            var memberName = "Test";
            var target = new object();
            var value = "";
            var error = "error";
            var errorTcs = new TaskCompletionSource<object?>();
            var invokeCount = 0;
            var builder = ValidationRuleBuilder<object>.Get();
            var cts = new CancellationTokenSource();
            var rule = builder.AddAsyncValidator(memberName, o => value, this, (o, s, state, ct, m) =>
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
            WaitCompletion();
            task.IsCompleted.ShouldBeTrue();
            errors.Count.ShouldEqual(1);
            errors[memberName].ShouldEqual(error);

            errorTcs = new TaskCompletionSource<object?>();
            task = rule.ValidateAsync(target, memberName, errors, cts.Token, DefaultMetadata)!;
            invokeCount.ShouldEqual(2);
            task.IsCompleted.ShouldBeFalse();
            errorTcs.TrySetResult(error);
            WaitCompletion();
            task.IsCompleted.ShouldBeTrue();
            errors.Count.ShouldEqual(1);
            ((IEnumerable<object>) errors[memberName]!).SequenceEqual(new[] {error, error}).ShouldBeTrue();

            errorTcs = new TaskCompletionSource<object?>();
            error = null;
            errors.Clear();
            task = rule.ValidateAsync(target, memberName, errors, cts.Token, DefaultMetadata)!;
            invokeCount.ShouldEqual(3);
            task.IsCompleted.ShouldBeFalse();
            errorTcs.TrySetResult(error);
            WaitCompletion();
            task.IsCompleted.ShouldBeTrue();
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
            var builder = ValidationRuleBuilder<object>.Get();
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
            rule.ValidateAsync(target, memberName, errors, default, DefaultMetadata).ShouldBeNull();
            invokeCount.ShouldEqual(1);
            errors.Count.ShouldEqual(1);
            errors[memberName].ShouldEqual(error);

            errors = new Dictionary<string, object?>();
            rule.ValidateAsync(target, dMember, errors, default, DefaultMetadata).ShouldBeNull();
            invokeCount.ShouldEqual(2);
            errors.Count.ShouldEqual(1);
            errors[memberName].ShouldEqual(error);

            errors = new Dictionary<string, object?>();
            rule.ValidateAsync(target, "", errors, default, DefaultMetadata).ShouldBeNull();
            invokeCount.ShouldEqual(3);
            errors.Count.ShouldEqual(1);
            errors[memberName].ShouldEqual(error);

            errors = new Dictionary<string, object?>();
            rule.ValidateAsync(target, "e", errors, default, DefaultMetadata).ShouldBeNull();
            invokeCount.ShouldEqual(3);
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
            var builder = ValidationRuleBuilder<object>.Get();
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
            rule.ValidateAsync(target, memberName, errors, default, DefaultMetadata).ShouldBeNull();
            invokeCount.ShouldEqual(1);
            errors.Count.ShouldEqual(1);
            errors[memberName].ShouldEqual(error);

            canValidate = false;
            errors = new Dictionary<string, object?>();
            rule.ValidateAsync(target, memberName, errors, default, DefaultMetadata).ShouldBeNull();
            invokeCount.ShouldEqual(1);
            errors.Count.ShouldEqual(0);
        }

        [Fact]
        public void ForExtensionShouldBeCorrect1()
        {
            var validationModel = new ValidationModel {Property = "G"};
            var builder = ValidationRuleBuilder<ValidationModel>.Get();
            var memberBuilder = builder.For(model => model.Property);
            memberBuilder.MemberName.ShouldEqual(nameof(ValidationModel.Property));
            memberBuilder.Accessor(validationModel).ShouldEqual(validationModel.Property);
        }

        [Fact]
        public void ForExtensionShouldBeCorrect2()
        {
            var validationModel = new ValidationModel {Property = "G"};
            var builder = ValidationRuleBuilder<ValidationModel>.Get();
            var memberBuilder = builder.For(nameof(validationModel.Property), model => model.Property);
            memberBuilder.MemberName.ShouldEqual(nameof(ValidationModel.Property));
            memberBuilder.Accessor(validationModel).ShouldEqual(validationModel.Property);
        }

        [Fact]
        public void NotNullExtensionShouldBeCorrect()
        {
            var error = "error";
            var propertyName = nameof(ValidationModel.Property);
            var validationModel = new ValidationModel();
            var rule = ValidationRuleBuilder<ValidationModel>.Get().For(propertyName, model => model.Property).NotNull(error).Build().Item!;

            var errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, null).ShouldBeNull();
            errors.Count.ShouldEqual(1);
            errors[propertyName].ShouldEqual(error);

            validationModel.Property = "test";
            errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, null).ShouldBeNull();
            errors.Count.ShouldEqual(0);
        }

        [Fact]
        public void NullExtensionShouldBeCorrect()
        {
            var error = "error";
            var propertyName = nameof(ValidationModel.Property);
            var validationModel = new ValidationModel();
            var rule = ValidationRuleBuilder<ValidationModel>.Get().For(propertyName, model => model.Property).Null(error).Build().Item!;

            validationModel.Property = "test";
            var errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, null).ShouldBeNull();
            errors.Count.ShouldEqual(1);
            errors[propertyName].ShouldEqual(error);

            validationModel.Property = null;
            errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, null).ShouldBeNull();
            errors.Count.ShouldEqual(0);
        }

        [Fact]
        public void NotEmptyExtensionShouldBeCorrect1()
        {
            var error = "error";
            var propertyName = nameof(ValidationModel.Property);
            var validationModel = new ValidationModel();
            var rule = ValidationRuleBuilder<ValidationModel>.Get().For(propertyName, model => model.Property).NotEmpty(error).Build().Item!;

            validationModel.Property = "";
            var errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, null).ShouldBeNull();
            errors.Count.ShouldEqual(1);
            errors[propertyName].ShouldEqual(error);

            validationModel.Property = "test";
            errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, null).ShouldBeNull();
            errors.Count.ShouldEqual(0);
        }

        [Fact]
        public void NotEmptyExtensionShouldBeCorrect2()
        {
            var error = "error";
            var propertyName = nameof(ValidationModel.Property);
            var value = 0;
            var validationModel = new ValidationModel();
            var rule = ValidationRuleBuilder<ValidationModel>.Get().For(propertyName, model => value).NotEmpty(error).Build().Item!;

            var errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, null).ShouldBeNull();
            errors.Count.ShouldEqual(1);
            errors[propertyName].ShouldEqual(error);

            value = 1;
            errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, null).ShouldBeNull();
            errors.Count.ShouldEqual(0);
        }

        [Fact]
        public void NotEmptyExtensionShouldBeCorrect3()
        {
            var error = "error";
            var propertyName = nameof(ValidationModel.Property);
            var value = new string[0];
            var validationModel = new ValidationModel();
            var rule = ValidationRuleBuilder<ValidationModel>.Get().For(propertyName, model => value).NotEmpty(error).Build().Item!;

            var errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, null).ShouldBeNull();
            errors.Count.ShouldEqual(1);
            errors[propertyName].ShouldEqual(error);

            value = new[] {""};
            errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, null).ShouldBeNull();
            errors.Count.ShouldEqual(0);
        }

        [Fact]
        public void EmptyExtensionShouldBeCorrect1()
        {
            var error = "error";
            var propertyName = nameof(ValidationModel.Property);
            var validationModel = new ValidationModel();
            var rule = ValidationRuleBuilder<ValidationModel>.Get().For(propertyName, model => model.Property).Empty(error).Build().Item!;

            validationModel.Property = "test";
            var errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, null).ShouldBeNull();
            errors.Count.ShouldEqual(1);
            errors[propertyName].ShouldEqual(error);

            validationModel.Property = "";
            errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, null).ShouldBeNull();
            errors.Count.ShouldEqual(0);
        }

        [Fact]
        public void EmptyExtensionShouldBeCorrect2()
        {
            var error = "error";
            var propertyName = nameof(ValidationModel.Property);
            var value = 1;
            var validationModel = new ValidationModel();
            var rule = ValidationRuleBuilder<ValidationModel>.Get().For(propertyName, model => value).Empty(error).Build().Item!;

            var errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, null).ShouldBeNull();
            errors.Count.ShouldEqual(1);
            errors[propertyName].ShouldEqual(error);

            value = 0;
            errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, null).ShouldBeNull();
            errors.Count.ShouldEqual(0);
        }

        [Fact]
        public void EmptyExtensionShouldBeCorrect3()
        {
            var error = "error";
            var propertyName = nameof(ValidationModel.Property);
            var value = new[] {""};
            var validationModel = new ValidationModel();
            var rule = ValidationRuleBuilder<ValidationModel>.Get().For(propertyName, model => value).Empty(error).Build().Item!;

            var errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, null).ShouldBeNull();
            errors.Count.ShouldEqual(1);
            errors[propertyName].ShouldEqual(error);

            value = new string[0];
            errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, null).ShouldBeNull();
            errors.Count.ShouldEqual(0);
        }

        [Fact]
        public void LengthExtensionShouldBeCorrect()
        {
            var error = "error";
            var propertyName = nameof(ValidationModel.Property);
            var validationModel = new ValidationModel();
            var rule = ValidationRuleBuilder<ValidationModel>.Get().For(propertyName, model => model.Property).Length(1, 5, error).Build().Item!;

            var errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, null).ShouldBeNull();
            errors.Count.ShouldEqual(1);
            errors[propertyName].ShouldEqual(error);

            validationModel.Property = "testtest";
            errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, null).ShouldBeNull();
            errors.Count.ShouldEqual(1);
            errors[propertyName].ShouldEqual(error);

            validationModel.Property = "test";
            errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, null).ShouldBeNull();
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
            var rule = ValidationRuleBuilder<ValidationModel>.Get()
                .For(propertyName, model => model.Property).Must((model, s, m) =>
                {
                    m.ShouldEqual(DefaultMetadata);
                    return !string.IsNullOrEmpty(model.Property) && !string.IsNullOrEmpty(s);
                }, new Func<string>(() => error), (model, context) =>
                {
                    model.ShouldEqual(validationModel);
                    context.ShouldEqual(DefaultMetadata);
                    return canValidate;
                }, new[] {dpMember}).Build().Item!;

            var errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, DefaultMetadata).ShouldBeNull();
            errors.Count.ShouldEqual(1);
            errors[propertyName].ShouldEqual(error);

            errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, dpMember, errors, default, DefaultMetadata).ShouldBeNull();
            errors.Count.ShouldEqual(1);
            errors[propertyName].ShouldEqual(error);

            canValidate = false;
            errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, DefaultMetadata).ShouldBeNull();
            errors.Count.ShouldEqual(0);

            canValidate = true;
            validationModel.Property = "test";
            errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, default, DefaultMetadata).ShouldBeNull();
            errors.Count.ShouldEqual(0);
        }

        [Fact]
        public void MustAsyncExtensionShouldBeCorrect()
        {
            var dpMember = "d";
            var error = "error";
            var canValidate = true;
            var propertyName = nameof(ValidationModel.Property);
            var validationModel = new ValidationModel();
            var tcs = new TaskCompletionSource<bool>();
            var cts = new CancellationTokenSource();
            var rule = ValidationRuleBuilder<ValidationModel>.Get()
                .For(propertyName, model => model.Property).MustAsync((model, s, c, m) =>
                {
                    model.ShouldEqual(validationModel);
                    s.ShouldEqual(validationModel.Property);
                    m.ShouldEqual(DefaultMetadata);
                    c.ShouldEqual(cts.Token);
                    return tcs.Task;
                }, new Func<string>(() => error), (model, context) =>
                {
                    model.ShouldEqual(validationModel);
                    context.ShouldEqual(DefaultMetadata);
                    return canValidate;
                }, new[] {dpMember}).Build().Item!;

            var errors = new Dictionary<string, object?>();
            var task = rule.ValidateAsync(validationModel, propertyName, errors, cts.Token, DefaultMetadata)!;
            task.IsCompleted.ShouldBeFalse();
            tcs.TrySetResult(false);
            WaitCompletion();
            task.IsCompleted.ShouldBeTrue();
            errors.Count.ShouldEqual(1);
            errors[propertyName].ShouldEqual(error);

            tcs = new TaskCompletionSource<bool>();
            errors = new Dictionary<string, object?>();
            task = rule.ValidateAsync(validationModel, dpMember, errors, cts.Token, DefaultMetadata)!;
            task.IsCompleted.ShouldBeFalse();
            tcs.TrySetResult(false);
            WaitCompletion();
            task.IsCompleted.ShouldBeTrue();
            errors.Count.ShouldEqual(1);
            errors[propertyName].ShouldEqual(error);

            canValidate = false;
            tcs = new TaskCompletionSource<bool>();
            errors = new Dictionary<string, object?>();
            rule.ValidateAsync(validationModel, propertyName, errors, cts.Token, DefaultMetadata).ShouldBeNull();
            errors.Count.ShouldEqual(0);

            canValidate = true;
            validationModel.Property = "test";
            errors = new Dictionary<string, object?>();
            task = rule.ValidateAsync(validationModel, propertyName, errors, cts.Token, DefaultMetadata)!;
            task.IsCompleted.ShouldBeFalse();
            tcs.TrySetResult(true);
            WaitCompletion();
            task.IsCompleted.ShouldBeTrue();
            errors.Count.ShouldEqual(0);
        }

        #endregion

        #region Nested types

        private sealed class ValidationModel
        {
            #region Properties

            public string? Property { get; set; }

            #endregion
        }

        #endregion
    }
}