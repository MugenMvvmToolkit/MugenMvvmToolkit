using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Models.Components;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;

namespace MugenMvvm.Validation.Components
{
    public sealed class RuleValidationHandler : IValidationHandlerComponent, IHasPriority, IHasTarget<object>, IDisposableComponent<IValidator>
    {
        public readonly ItemOrIReadOnlyList<IValidationRule> Rules;
        private readonly List<ValidationErrorInfo>? _errors;
        private readonly CancellationTokenSource? _disposeToken;

        public RuleValidationHandler(object target, ItemOrIReadOnlyList<IValidationRule> rules, int priority = ValidationComponentPriority.RuleValidationHandler)
        {
            Should.NotBeNull(target, nameof(target));
            Target = target;
            Rules = rules;
            Priority = priority;
            foreach (var rule in rules)
            {
                if (rule.IsAsync)
                {
                    _disposeToken = new CancellationTokenSource();
                    break;
                }
            }

            if (_disposeToken == null && rules.Count > 1)
                _errors = new List<ValidationErrorInfo>(rules.Count);
        }

        public int Priority { get; init; }

        public object Target { get; }

        public async Task TryValidateAsync(IValidator validator, string? member, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            if (_disposeToken == null)
            {
                Validate(validator, member, metadata);
                return;
            }

            var token = cancellationToken.CanBeCanceled ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _disposeToken.Token) : _disposeToken;
            try
            {
                await ValidateAsync(validator, member, token.Token, metadata).ConfigureAwait(false);
            }
            finally
            {
                if (cancellationToken.CanBeCanceled)
                    token.Dispose();
            }
        }

        private async Task ValidateAsync(IValidator validator, string? member, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            var editor = new ItemOrListEditor<ValidationErrorInfo>(Rules.Count);
            var tasks = new ItemOrListEditor<ValueTask<ItemOrIReadOnlyList<ValidationErrorInfo>>>(2);
            foreach (var rule in Rules)
            {
                var task = rule.ValidateAsync(Target, member, cancellationToken, metadata);
                if (task.IsCompletedSuccessfully)
                    editor.AddRange(task.Result);
                else
                    tasks.Add(task);
            }

            foreach (var task in tasks)
                editor.AddRange(await task.ConfigureAwait(false));

            validator.SetErrors(this, editor, metadata);
        }

        private void Validate(IValidator validator, string? member, IReadOnlyMetadataContext? metadata)
        {
            _errors?.Clear();
            var editor = new ItemOrListEditor<ValidationErrorInfo>(_errors);
            foreach (var rule in Rules)
            {
                var task = rule.ValidateAsync(Target, member, default, metadata);
                if (!task.IsCompletedSuccessfully)
                    ExceptionManager.ThrowObjectNotInitialized(task);

                editor.AddRange(task.Result);
            }

            validator.SetErrors(this, editor, metadata);
        }

        void IDisposableComponent<IValidator>.Dispose(IValidator owner, IReadOnlyMetadataContext? metadata) => _disposeToken?.Cancel();
    }
}