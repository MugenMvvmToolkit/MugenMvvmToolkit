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
    [StructLayout(LayoutKind.Auto)]
    public ref struct ValidationRuleBuilder<T> where T : class
    {
        private ItemOrListEditor<IValidationRule> _rules;

        public ValidationRuleBuilder<T> AddValidator<TValue, TState>(string memberName, Func<T, TValue> memberAccessor, TState state,
            Func<T, TValue, TState, IReadOnlyMetadataContext?, object?> validator, Func<T, TState, IReadOnlyMetadataContext?, bool>? condition = null,
            ItemOrIReadOnlyList<string> dependencyMembers = default)
        {
            _rules.Add(new Rule<TValue, TState>(memberName, memberAccessor, validator, condition, dependencyMembers, state));
            return this;
        }

        public ValidationRuleBuilder<T> AddAsyncValidator<TValue, TState>(string memberName, Func<T, TValue> memberAccessor, TState state,
            Func<T, TValue, TState, CancellationToken, IReadOnlyMetadataContext?, Task<object?>> validator, Func<T, TState, IReadOnlyMetadataContext?, bool>? condition = null,
            ItemOrIReadOnlyList<string> dependencyMembers = default)
        {
            _rules.Add(new Rule<TValue, TState>(memberName, memberAccessor, validator, condition, dependencyMembers, state));
            return this;
        }

        public ItemOrIReadOnlyList<IValidationRule> Build() => _rules.ToItemOrList();

        private sealed class Rule<TValue, TState> : IValidationRule
        {
            private readonly Func<T, TState, IReadOnlyMetadataContext?, bool>? _condition;
            private readonly object? _dependencyMembers;
            private readonly Func<T, TValue> _memberAccessor;
            private readonly string _memberName;
            private readonly TState _state;
            private readonly Delegate _validator;
            private readonly Func<Task<object?>, object?, ItemOrIReadOnlyList<ValidationErrorInfo>>? _converterDelegate;

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
                if (IsAsync)
                    _converterDelegate = ConvertResult;
            }

            private ItemOrIReadOnlyList<ValidationErrorInfo> ConvertResult(Task<object?> task, object? target) => ToError(target!, task.Result);

            public bool IsAsync => _validator is Func<T, TValue, TState, CancellationToken, IReadOnlyMetadataContext?, Task<object?>>;

            public ValueTask<ItemOrIReadOnlyList<ValidationErrorInfo>> ValidateAsync(object t, string? member, CancellationToken cancellationToken,
                IReadOnlyMetadataContext? metadata)
            {
                if (t is not T target)
                    return default;

                if (!string.IsNullOrEmpty(member) && !string.Equals(_memberName, member) && !ContainsDependencyMember(member!))
                    return default;

                if (_condition != null && !_condition(target, _state, metadata))
                    return new ValueTask<ItemOrIReadOnlyList<ValidationErrorInfo>>(ToError(target, null));

                if (_validator is Func<T, TValue, TState, IReadOnlyMetadataContext?, object?> validator)
                    return new ValueTask<ItemOrIReadOnlyList<ValidationErrorInfo>>(ToError(target, validator(target, _memberAccessor(target), _state, metadata)));
                return ((Func<T, TValue, TState, CancellationToken, IReadOnlyMetadataContext?, Task<object?>>) _validator)
                       .Invoke(target, _memberAccessor(target), _state, cancellationToken, metadata)
                       .ContinueWith(_converterDelegate!, target, cancellationToken, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current)
                       .AsValueTask();
            }

            private ItemOrIReadOnlyList<ValidationErrorInfo> ToError(object target, object? error) => new ValidationErrorInfo(target, _memberName, error);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private bool ContainsDependencyMember(string memberName) => ItemOrIReadOnlyList.FromRawValue<string>(_dependencyMembers).Contains(memberName, StringComparer.Ordinal);
        }
    }
}