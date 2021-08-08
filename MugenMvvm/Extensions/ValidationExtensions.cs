using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;
using MugenMvvm.Internal;
using MugenMvvm.Validation;
using MugenMvvm.Validation.Components;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        public static IValidator GetValidator(this IValidationManager validatorProvider, ItemOrIReadOnlyList<object> targets, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(validatorProvider, nameof(validatorProvider));
            var result = validatorProvider.TryGetValidator(targets, metadata);
            if (result == null)
                ExceptionManager.ThrowRequestNotSupported<IValidatorProviderComponent>(validatorProvider, targets.GetRawValue(), metadata);
            return result;
        }

        public static void SetErrors(this IValidator validator, ItemOrIReadOnlyList<ValidationErrorInfo> errors, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(validator, nameof(validator));
            validator.SetErrors(validator, errors, metadata);
        }

        public static bool AddChildValidator(this IValidator owner, IValidator validator)
        {
            Should.NotBeNull(owner, nameof(owner));
            return owner.GetOrAddComponent<ChildValidatorAdapter>().Add(validator);
        }

        public static bool RemoveChildValidator(this IValidator owner, IValidator validator)
        {
            Should.NotBeNull(owner, nameof(owner));
            return owner.GetOrAddComponent<ChildValidatorAdapter>().Remove(validator);
        }

        public static bool Contains(this ItemOrIReadOnlyList<ValidationErrorInfo> errors, string? member)
        {
            foreach (var error in errors)
            {
                if (member == error.Member)
                    return true;
            }

            return false;
        }

        public static ValidationRuleMemberBuilder<T, TMember> For<T, TMember>(this ValidationRuleBuilder.Builder<T> builder, Expression<Func<T, TMember>> member,
            IReflectionManager? reflectionManager = null) where T : class
        {
            var memberInfo = member.GetMemberInfo();
            return new ValidationRuleMemberBuilder<T, TMember>(memberInfo.Name, memberInfo.GetMemberGetter<T, TMember>(reflectionManager), builder);
        }

        public static ValidationRuleMemberBuilder<T, TMember> For<T, TMember>(this ValidationRuleBuilder.Builder<T> builder, string memberName, Func<T, TMember> memberAccessor)
            where T : class
            => new(memberName, memberAccessor, builder);

        public static ValidationRuleMemberBuilder<T, TMember> For<T, TOldMember, TMember>(this ValidationRuleMemberBuilder<T, TOldMember> builder,
            Expression<Func<T, TMember>> member) where T : class
        {
            var memberInfo = member.GetMemberInfo();
            return new ValidationRuleMemberBuilder<T, TMember>(memberInfo.Name, memberInfo.GetMemberGetter<T, TMember>(), builder.Builder);
        }

        public static ValidationRuleMemberBuilder<T, TMember> For<T, TOldMember, TMember>(this ValidationRuleMemberBuilder<T, TOldMember> builder, string memberName,
            Func<T, TMember> memberAccessor) where T : class
            => new(memberName, memberAccessor, builder.Builder);

        public static void AddRulesTo<T>(this ValidationRuleBuilder.Builder<T> builder, RuleValidationManager ruleValidationManager) where T : class
        {
            Should.NotBeNull(ruleValidationManager, nameof(ruleValidationManager));
            ruleValidationManager.AddRules(builder.Build(), (_, o, _) => o is T);
        }

        public static void AddRulesTo<T, TMember>(this ValidationRuleMemberBuilder<T, TMember> builder, RuleValidationManager ruleValidationManager) where T : class
            => builder.Builder.AddRulesTo(ruleValidationManager);

        public static ActionToken AddRulesTo<T>(this ValidationRuleBuilder.Builder<T> builder, IValidator validator, T target) where T : class
        {
            Should.NotBeNull(validator, nameof(validator));
            Should.NotBeNull(target, nameof(target));
            return validator.AddComponent(new RuleValidationHandler(target, builder.Build()));
        }

        public static ActionToken AddRulesTo<T, TMember>(this ValidationRuleMemberBuilder<T, TMember> builder, IValidator validator, T target) where T : class
            => builder.Builder.AddRulesTo(validator, target);

        public static ValidationRuleMemberBuilder<T, TMember> NotNull<T, TMember>(this ValidationRuleMemberBuilder<T, TMember> builder, ValidationError error,
            Func<T, IReadOnlyMetadataContext?, bool>? condition = null, ItemOrIReadOnlyList<string> dependencyMembers = default)
            where T : class =>
            builder.Must((t, v, m) => v != null, error, condition, dependencyMembers);

        public static ValidationRuleMemberBuilder<T, TMember> Null<T, TMember>(this ValidationRuleMemberBuilder<T, TMember> builder, ValidationError error,
            Func<T, IReadOnlyMetadataContext?, bool>? condition = null, ItemOrIReadOnlyList<string> dependencyMembers = default)
            where T : class =>
            builder.Must((t, v, m) => v == null, error, condition, dependencyMembers);

        public static ValidationRuleMemberBuilder<T, TMember> NotEmpty<T, TMember>(this ValidationRuleMemberBuilder<T, TMember> builder, ValidationError error,
            Func<T, IReadOnlyMetadataContext?, bool>? condition = null, ItemOrIReadOnlyList<string> dependencyMembers = default)
            where T : class =>
            builder.Must((t, v, m) => !IsEmpty(v), error, condition, dependencyMembers);

        public static ValidationRuleMemberBuilder<T, TMember> Empty<T, TMember>(this ValidationRuleMemberBuilder<T, TMember> builder, ValidationError error,
            Func<T, IReadOnlyMetadataContext?, bool>? condition = null, ItemOrIReadOnlyList<string> dependencyMembers = default)
            where T : class =>
            builder.Must((t, v, m) => IsEmpty(v), error, condition, dependencyMembers);

        public static ValidationRuleMemberBuilder<T, string?> Length<T>(this ValidationRuleMemberBuilder<T, string?> builder, int min, int max,
            ValidationError error, Func<T, IReadOnlyMetadataContext?, bool>? condition = null, ItemOrIReadOnlyList<string> dependencyMembers = default)
            where T : class =>
            builder.Must((t, v, m) => Length(v, min, max), error, condition, dependencyMembers);

        private static bool Length(string? value, int min, int max)
        {
            var l = value?.Length ?? 0;
            return l >= min && l <= max;
        }

        private static bool IsEmpty<T>(T value)
        {
            if (TypeChecker.IsValueType<T>())
                return EqualityComparer<T>.Default.Equals(value, default!);

            switch (value)
            {
                case null:
                case string s when string.IsNullOrWhiteSpace(s):
                case ICollection c when c.Count == 0:
                case IEnumerable e when !e.Cast<object>().Any():
                    return true;
                default:
                    return false;
            }
        }
    }
}