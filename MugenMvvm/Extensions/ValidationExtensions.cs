using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Collections;
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
        #region Methods

        public static IValidator GetValidator(this IValidationManager validatorProvider, object request, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(validatorProvider, nameof(validatorProvider));
            var result = validatorProvider.TryGetValidator(request, metadata);
            if (result == null)
                ExceptionManager.ThrowRequestNotSupported<IValidatorProviderComponent>(validatorProvider, request, metadata);
            return result;
        }

        public static void SetErrors(this IValidator validator, object target, string memberName, ItemOrIReadOnlyList<object> errors, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(validator, nameof(validator));
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(memberName, nameof(memberName));
            InlineValidatorComponent? component = null;
            foreach (var c in validator.GetComponents<InlineValidatorComponent>(metadata))
            {
                if (c.Target == target)
                {
                    component = c;
                    break;
                }
            }

            if (component == null)
            {
                component = new InlineValidatorComponent(target);
                validator.AddComponent(component);
            }

            component.SetErrors(memberName, errors, metadata);
        }

        public static ValidationRuleMemberBuilder<T, TMember> For<T, TMember>(this ValidationRuleBuilder<T> builder, Expression<Func<T, TMember>> member) where T : class
        {
            var memberInfo = member.GetMemberInfo();
            return new ValidationRuleMemberBuilder<T, TMember>(memberInfo.Name, memberInfo.GetMemberGetter<T, TMember>(), builder);
        }

        public static ValidationRuleMemberBuilder<T, TMember> For<T, TMember>(this ValidationRuleBuilder<T> builder, string memberName, Func<T, TMember> memberAccessor) where T : class
            => new(memberName, memberAccessor, builder);

        public static ValidationRuleMemberBuilder<T, TMember> For<T, TOldMember, TMember>(this ValidationRuleMemberBuilder<T, TOldMember> builder, Expression<Func<T, TMember>> member) where T : class
        {
            var memberInfo = member.GetMemberInfo();
            return new ValidationRuleMemberBuilder<T, TMember>(memberInfo.Name, memberInfo.GetMemberGetter<T, TMember>(), builder.Builder);
        }

        public static ValidationRuleMemberBuilder<T, TMember> For<T, TOldMember, TMember>(this ValidationRuleMemberBuilder<T, TOldMember> builder, string memberName, Func<T, TMember> memberAccessor) where T : class
            => new(memberName, memberAccessor, builder.Builder);

        public static void AddRulesTo<T>(this ValidationRuleBuilder<T> builder, RuleValidatorProviderComponent ruleValidatorProvider) where T : class
        {
            Should.NotBeNull(ruleValidatorProvider, nameof(ruleValidatorProvider));
            ruleValidatorProvider.AddRules(builder.Build(), (validator, o, arg3) => o is T);
        }

        public static void AddRulesTo<T, TMember>(this ValidationRuleMemberBuilder<T, TMember> builder, RuleValidatorProviderComponent ruleValidatorProvider) where T : class
            => builder.Builder.AddRulesTo(ruleValidatorProvider);

        public static ValidationRuleMemberBuilder<T, TMember> NotNull<T, TMember>(this ValidationRuleMemberBuilder<T, TMember> builder, object error,
            Func<T, IReadOnlyMetadataContext?, bool>? condition = null, ICollection<string>? dependencyMembers = null)
            where T : class =>
            builder.Must((t, v, m) => v != null, error, condition, dependencyMembers);

        public static ValidationRuleMemberBuilder<T, TMember> Null<T, TMember>(this ValidationRuleMemberBuilder<T, TMember> builder, object error,
            Func<T, IReadOnlyMetadataContext?, bool>? condition = null, ICollection<string>? dependencyMembers = null)
            where T : class =>
            builder.Must((t, v, m) => v == null, error, condition, dependencyMembers);

        public static ValidationRuleMemberBuilder<T, TMember> NotEmpty<T, TMember>(this ValidationRuleMemberBuilder<T, TMember> builder, object error,
            Func<T, IReadOnlyMetadataContext?, bool>? condition = null, ICollection<string>? dependencyMembers = null)
            where T : class =>
            builder.Must((t, v, m) => !IsEmpty(v), error, condition, dependencyMembers);

        public static ValidationRuleMemberBuilder<T, TMember> Empty<T, TMember>(this ValidationRuleMemberBuilder<T, TMember> builder, object error,
            Func<T, IReadOnlyMetadataContext?, bool>? condition = null, ICollection<string>? dependencyMembers = null)
            where T : class =>
            builder.Must((t, v, m) => IsEmpty(v), error, condition, dependencyMembers);

        public static ValidationRuleMemberBuilder<T, string?> Length<T>(this ValidationRuleMemberBuilder<T, string?> builder, int min, int max,
            object error, Func<T, IReadOnlyMetadataContext?, bool>? condition = null, ICollection<string>? dependencyMembers = null)
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

        #endregion

        #region Nested types

        [StructLayout(LayoutKind.Auto)]
        public ref struct ValidationRuleMemberBuilder<T, TMember> where T : class
        {
            #region Fields

            public readonly Func<T, TMember> Accessor;
            public readonly string MemberName;
            public ValidationRuleBuilder<T> Builder;

            #endregion

            #region Constructors

            public ValidationRuleMemberBuilder(string memberName, Func<T, TMember> accessor, ValidationRuleBuilder<T> builder)
            {
                Should.NotBeNull(memberName, nameof(memberName));
                Should.NotBeNull(accessor, nameof(accessor));
                Accessor = accessor;
                MemberName = memberName;
                Builder = builder;
            }

            #endregion

            #region Methods

            public ItemOrIReadOnlyList<IValidationRule> Build() => Builder.Build();

            public ValidationRuleMemberBuilder<T, TMember> Must(Func<T, TMember, IReadOnlyMetadataContext?, bool> validator, object error,
                Func<T, IReadOnlyMetadataContext?, bool>? condition = null, ICollection<string>? dependencyMembers = null)
            {
                Should.NotBeNull(validator, nameof(validator));
                Should.NotBeNull(error, nameof(error));
                Builder.AddValidator(MemberName, Accessor, (validator, error, condition), (t, v, s, m) =>
                    {
                        if (s.validator(t, v, m))
                            return null;
                        if (s.error is Func<object> func)
                            return func();
                        return s.error;
                    },
                    condition == null
                        ? (Func<T, (Func<T, TMember, IReadOnlyMetadataContext?, bool> validator, object error, Func<T, IReadOnlyMetadataContext?, bool>? condition), IReadOnlyMetadataContext?, bool>?) null
                        : (t, s, m) => s.condition!(t, m), dependencyMembers);
                return this;
            }

            public ValidationRuleMemberBuilder<T, TMember> MustAsync(Func<T, TMember, CancellationToken, IReadOnlyMetadataContext?, Task<bool>> validator, object error,
                Func<T, IReadOnlyMetadataContext?, bool>? condition = null, ICollection<string>? dependencyMembers = null)
            {
                Should.NotBeNull(validator, nameof(validator));
                Should.NotBeNull(error, nameof(error));
                Builder.AddAsyncValidator(MemberName, Accessor, (validator, error, condition), (t, v, s, c, m) => s
                        .validator(t, v, c, m)
                        .ContinueWith((task, o) =>
                        {
                            if (task.Result)
                                return null;
                            if (o is Func<object> func)
                                return func();
                            return o;
                        }, s.error, c, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current),
                    condition == null
                        ? (Func<T, (Func<T, TMember, CancellationToken, IReadOnlyMetadataContext?, Task<bool>> validator, object error, Func<T, IReadOnlyMetadataContext?, bool>? condition), IReadOnlyMetadataContext?,
                            bool>?) null
                        : (t, s, m) => s.condition!(t, m), dependencyMembers);
                return this;
            }

            #endregion
        }

        #endregion
    }
}