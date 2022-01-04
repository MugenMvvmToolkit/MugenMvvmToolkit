using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Validation;

namespace MugenMvvm.Validation
{
    [StructLayout(LayoutKind.Auto)]
    public ref struct ValidationRuleMemberBuilder<T, TMember> where T : class
    {
        public readonly Func<T, TMember> Accessor;
        public readonly string MemberName;
        public ValidationRuleBuilder.Builder<T> Builder;

        public ValidationRuleMemberBuilder(string memberName, Func<T, TMember> accessor, ValidationRuleBuilder.Builder<T> builder)
        {
            Should.NotBeNull(memberName, nameof(memberName));
            Should.NotBeNull(accessor, nameof(accessor));
            Accessor = accessor;
            MemberName = memberName;
            Builder = builder;
        }

        public static implicit operator ValidationRuleBuilder.Builder<T>(ValidationRuleMemberBuilder<T, TMember> memberBuilder) => memberBuilder.Builder;

        public ItemOrIReadOnlyList<IValidationRule> Build() => Builder.Build();

        public ValidationRuleMemberBuilder<T, TMember> AddValidator<TState>(TState state,
            Func<IValidator, T, TMember, TState, IReadOnlyMetadataContext?, object?> validator, Func<T, TState, IReadOnlyMetadataContext?, bool>? condition = null,
            ItemOrIReadOnlyList<string> dependencyMembers = default)
        {
            Builder.AddValidator(MemberName, Accessor, state, validator, condition, dependencyMembers);
            return this;
        }

        public ValidationRuleMemberBuilder<T, TMember> AddAsyncValidator<TState>(TState state,
            Func<IValidator, T, TMember, TState, CancellationToken, IReadOnlyMetadataContext?, ValueTask<object?>> validator,
            Func<T, TState, IReadOnlyMetadataContext?, bool>? condition = null,
            ItemOrIReadOnlyList<string> dependencyMembers = default)
        {
            Builder.AddAsyncValidator(MemberName, Accessor, state, validator, condition, dependencyMembers);
            return this;
        }

        public ValidationRuleMemberBuilder<T, TMember> Must(Func<TMember, IReadOnlyMetadataContext?, bool> validator, ValidationRuleError error,
            Func<T, IReadOnlyMetadataContext?, bool>? condition = null, ItemOrIReadOnlyList<string> dependencyMembers = default)
        {
            Should.NotBeNull(validator, nameof(validator));
            Builder.AddValidator(MemberName, Accessor, (validator, error, condition), (_, _, v, s, m) => s.validator(v, m) ? null : s.error.Error,
                condition == null ? null : (t, s, m) => s.condition!(t, m), dependencyMembers);
            return this;
        }

        public ValidationRuleMemberBuilder<T, TMember> MustAsync(Func<TMember, CancellationToken, IReadOnlyMetadataContext?, Task<bool>> validator, ValidationRuleError error,
            Func<T, IReadOnlyMetadataContext?, bool>? condition = null, ItemOrIReadOnlyList<string> dependencyMembers = default)
        {
            Should.NotBeNull(validator, nameof(validator));
            Builder.AddAsyncValidator(MemberName, Accessor, (validator, error, condition), static async (_, _, v, s, c, m) =>
            {
                if (await s.validator(v, c, m).ConfigureAwait(false))
                    return null;
                return s.error.Error;
            }, condition == null ? null : (t, s, m) => s.condition!(t, m), dependencyMembers);
            return this;
        }
    }
}