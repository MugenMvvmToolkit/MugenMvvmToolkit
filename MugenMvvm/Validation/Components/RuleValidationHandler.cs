using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;

namespace MugenMvvm.Validation.Components
{
    public sealed class RuleValidationHandler : IValidationHandlerComponent, IDisposable
    {
        [ThreadStatic]
        private static List<ValidationErrorInfo>? _errorsCache;

        public readonly ItemOrIReadOnlyList<IValidationRule> Rules;
        private readonly CancellationTokenSource? _disposeToken;

        public RuleValidationHandler(object target, ItemOrIReadOnlyList<IValidationRule> rules, bool useCache)
        {
            Should.NotBeNull(target, nameof(target));
            UseCache = useCache;
            Target = target;
            Rules = rules;
            foreach (var rule in rules)
            {
                if (rule.IsAsync)
                {
                    _disposeToken = new CancellationTokenSource();
                    break;
                }
            }
        }

        public bool UseCache { get; }

        public object Target { get; }

        private static ItemOrListEditor<ValidationErrorInfo> GetItemOrListEditor(int count, bool useCache)
        {
            if (count < 2)
                return default;
            if (useCache)
            {
                var errors = _errorsCache;
                if (errors == null)
                {
                    errors = new List<ValidationErrorInfo>(count);
                    _errorsCache = errors;
                }
                else
                    errors.Clear();

                return new ItemOrListEditor<ValidationErrorInfo>(errors);
            }

            return new ItemOrListEditor<ValidationErrorInfo>(new List<ValidationErrorInfo>(count));
        }

        public Task TryValidateAsync(IValidator validator, string? member, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            if (_disposeToken == null)
            {
                Validate(validator, member, metadata);
                return Task.CompletedTask;
            }

            return ValidateAsync(validator, member,
                cancellationToken.CanBeCanceled ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _disposeToken.Token).Token : _disposeToken.Token, metadata);
        }

        private async Task ValidateAsync(IValidator validator, string? member, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            var editor = GetItemOrListEditor(Rules.Count, false);
            ItemOrListEditor<ValueTask<ItemOrIReadOnlyList<ValidationErrorInfo>>> tasks = default;
            foreach (var rule in Rules)
            {
                var task = rule.ValidateAsync(Target, member, cancellationToken, metadata);
                if (task.IsCompletedSuccessfully)
                    editor.AddRange(task.Result);
                else
                    tasks.Add(task);
            }

            foreach (var task in tasks.ToItemOrList())
                editor.AddRange(await task.ConfigureAwait(false));

            validator.SetErrors(this, editor, metadata);
        }

        private void Validate(IValidator validator, string? member, IReadOnlyMetadataContext? metadata)
        {
            var editor = GetItemOrListEditor(Rules.Count, true);
            foreach (var rule in Rules)
            {
                var task = rule.ValidateAsync(Target, member, default, metadata);
                if (!task.IsCompletedSuccessfully)
                    ExceptionManager.ThrowObjectNotInitialized(task);

                editor.AddRange(task.Result);
            }

            validator.SetErrors(this, editor, metadata);
        }

        void IDisposable.Dispose() => _disposeToken?.Cancel();
    }
}