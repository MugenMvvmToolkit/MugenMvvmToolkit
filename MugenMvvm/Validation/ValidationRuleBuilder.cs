using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Internal;

namespace MugenMvvm.Validation
{
    public ref struct ValidationRuleBuilder<T> where T : class
    {
        #region Fields

        private ItemOrListEditor<IValidationRule, List<IValidationRule>> _rules;

        #endregion

        #region Constructors

        private ValidationRuleBuilder(bool _)
        {
            _rules = ItemOrListEditor.Get<IValidationRule>();
        }

        #endregion

        #region Methods

        public static ValidationRuleBuilder<T> Get() => new ValidationRuleBuilder<T>(true);

        public ValidationRuleBuilder<T> AddValidator<TValue, TState>(string memberName, Func<T, TValue> memberAccessor, TState state,
            Func<T, TValue, TState, IReadOnlyMetadataContext?, object?> validator, Func<T, TState, IReadOnlyMetadataContext?, bool>? condition = null, ICollection<string>? dependencyMembers = null)
        {
            _rules.Add(new Rule<TValue, TState>(memberName, memberAccessor, validator, condition, dependencyMembers, state));
            return this;
        }

        public ValidationRuleBuilder<T> AddAsyncValidator<TValue, TState>(string memberName, Func<T, TValue> memberAccessor, TState state,
            Func<T, TValue, TState, CancellationToken, IReadOnlyMetadataContext?, Task<object?>> validator, Func<T, TState, IReadOnlyMetadataContext?, bool>? condition = null, ICollection<string>? dependencyMembers = null)
        {
            _rules.Add(new Rule<TValue, TState>(memberName, memberAccessor, validator, condition, dependencyMembers, state));
            return this;
        }

        public ItemOrList<IValidationRule, IReadOnlyList<IValidationRule>> Build() => _rules.ToItemOrList<IReadOnlyList<IValidationRule>>();

        #endregion

        #region Nested types

        private sealed class Rule<TValue, TState> : IValidationRule
        {
            #region Fields

            private readonly Action<Task<object?>, object?>? _addErrorDelegate;
            private readonly Func<T, TState, IReadOnlyMetadataContext?, bool>? _condition;
            private readonly ICollection<string>? _dependencyMembers;
            private readonly Func<T, TValue> _memberAccessor;
            private readonly string _memberName;
            private readonly TState _state;
            private readonly Delegate _validator;

            #endregion

            #region Constructors

            public Rule(string memberName, Func<T, TValue> memberAccessor,
                Delegate validator, Func<T, TState, IReadOnlyMetadataContext?, bool>? condition, ICollection<string>? dependencyMembers, TState state)
            {
                Should.NotBeNull(memberName, nameof(memberName));
                Should.NotBeNull(memberAccessor, nameof(memberAccessor));
                Should.NotBeNull(validator, nameof(validator));
                _memberName = memberName;
                _memberAccessor = memberAccessor;
                _validator = validator;
                _condition = condition;
                _dependencyMembers = dependencyMembers;
                _state = state;
                if (IsAsync)
                    _addErrorDelegate = AddError;
            }

            #endregion

            #region Properties

            public bool IsAsync => _validator is Func<T, TValue, TState, CancellationToken, IReadOnlyMetadataContext?, Task<object?>>;

            #endregion

            #region Implementation of interfaces

            public Task? ValidateAsync(object t, string memberName, IDictionary<string, object?> errors, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
            {
                if (!(t is T target) || _condition != null && !_condition(target, _state, metadata))
                    return null;

                if (!string.IsNullOrEmpty(memberName) && !string.Equals(_memberName, memberName) && (_dependencyMembers == null || !_dependencyMembers.Contains(memberName)))
                    return null;

                if (_validator is Func<T, TValue, TState, IReadOnlyMetadataContext?, object?> validator)
                {
                    AddError(errors, validator(target, _memberAccessor(target), _state, metadata));
                    return null;
                }

                return ((Func<T, TValue, TState, CancellationToken, IReadOnlyMetadataContext?, Task<object?>>)_validator)
                    .Invoke(target, _memberAccessor(target), _state, cancellationToken, metadata)
                    .ContinueWith(_addErrorDelegate!, errors, cancellationToken, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
            }

            #endregion

            #region Methods

            private void AddError(Task<object?> task, object? state) => AddError((IDictionary<string, object?>)state!, task.Result);

            private void AddError(IDictionary<string, object?> errors, object? error)
            {
                if (error == null)
                    return;
                lock (errors)
                {
                    errors.TryGetValue(_memberName, out var errorOrList);
                    if (errorOrList == null)
                        errors[_memberName] = error;
                    else
                    {
                        if (!(errorOrList is List<object> list))
                        {
                            list = new List<object>(2) { errorOrList };
                            errors[_memberName] = list;
                        }

                        list.Add(error);
                    }
                }
            }

            #endregion
        }

        #endregion
    }
}