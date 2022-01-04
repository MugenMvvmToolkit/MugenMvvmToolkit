using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Validation;

namespace MugenMvvm.Validation
{
    public static class ValidationRuleBuilder
    {
        public static Builder<T> Get<T>() where T : class => new(2);

        [StructLayout(LayoutKind.Auto)]
        public ref struct Builder<T> where T : class
        {
            private ItemOrListEditor<IValidationRule> _rules;

            public Builder(int capacity)
            {
                _rules = new ItemOrListEditor<IValidationRule>(capacity);
            }

            public Builder<T> AddRule(IValidationRule rule)
            {
                Should.NotBeNull(rule, nameof(rule));
                _rules.Add(rule);
                return this;
            }

            public Builder<T> AddValidator<TValue, TState>(string memberName, Func<T, TValue> memberAccessor, TState state,
                Func<IValidator, T, TValue, TState, IReadOnlyMetadataContext?, object?> validator, Func<T, TState, IReadOnlyMetadataContext?, bool>? condition = null,
                ItemOrIReadOnlyList<string> dependencyMembers = default) =>
                AddRule(new Rule<T, TValue, TState>(memberName, memberAccessor, validator, condition, dependencyMembers, state));

            public Builder<T> AddAsyncValidator<TValue, TState>(string memberName, Func<T, TValue> memberAccessor, TState state,
                Func<IValidator, T, TValue, TState, CancellationToken, IReadOnlyMetadataContext?, ValueTask<object?>> validator,
                Func<T, TState, IReadOnlyMetadataContext?, bool>? condition = null, ItemOrIReadOnlyList<string> dependencyMembers = default) =>
                AddRule(new Rule<T, TValue, TState>(memberName, memberAccessor, validator, condition, dependencyMembers, state));

            public ItemOrIReadOnlyList<IValidationRule> Build() => _rules.ToItemOrList();
        }

        private sealed class Rule<T, TValue, TState> : IValidationRule where T : class
        {
            private readonly Func<T, TState, IReadOnlyMetadataContext?, bool>? _condition;
            private readonly object? _dependencyMembers;
            private readonly Func<T, TValue> _memberAccessor;
            private readonly string _memberName;
            private readonly TState _state;
            private readonly Delegate _validator;

            public Rule(string memberName, Func<T, TValue> memberAccessor,
                Delegate validator, Func<T, TState, IReadOnlyMetadataContext?, bool>? condition, ItemOrIReadOnlyList<string> dependencyMembers, TState state)
            {
                Should.NotBeNull(memberName, nameof(memberName));
                Should.NotBeNull(memberAccessor, nameof(memberAccessor));
                Should.NotBeNull(validator, nameof(validator));
                _memberName = memberName;
                _memberAccessor = memberAccessor;
                _validator = validator;
                _condition = condition;
                _dependencyMembers = dependencyMembers.GetRawValue();
                _state = state;
            }

            public bool IsAsync => _validator is Func<IValidator, T, TValue, TState, CancellationToken, IReadOnlyMetadataContext?, ValueTask<object?>>;

            public void Dispose()
            {
            }

            public async ValueTask<ItemOrIReadOnlyList<ValidationErrorInfo>> ValidateAsync(IValidator validator, object t, string? member, CancellationToken cancellationToken,
                IReadOnlyMetadataContext? metadata)
            {
                if (t is not T target)
                    return default;

                if (!string.IsNullOrEmpty(member) && !string.Equals(_memberName, member) && !ContainsDependencyMember(member!))
                    return default;

                if (_condition != null && !_condition(target, _state, metadata))
                    return ToError(target, null);

                object? error;
                if (_validator is Func<IValidator, T, TValue, TState, IReadOnlyMetadataContext?, object?> validatorFunc)
                    error = validatorFunc(validator, target, _memberAccessor(target), _state, metadata);
                else
                    error = await ((Func<IValidator, T, TValue, TState, CancellationToken, IReadOnlyMetadataContext?, ValueTask<object?>>) _validator)
                                  .Invoke(validator, target, _memberAccessor(target), _state, cancellationToken, metadata)
                                  .ConfigureAwait(false);
                return ToError(target, error);
            }

            private ItemOrIReadOnlyList<ValidationErrorInfo> ToError(object target, object? error) => new ValidationErrorInfo(target, _memberName, error);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private bool ContainsDependencyMember(string memberName) => ItemOrIReadOnlyList.FromRawValue<string>(_dependencyMembers).Contains(memberName, StringComparer.Ordinal);
        }
    }
}