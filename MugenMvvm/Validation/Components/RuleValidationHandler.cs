﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Models.Components;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;

namespace MugenMvvm.Validation.Components
{
    public sealed class RuleValidationHandler : IValidationHandlerComponent, IHasTarget<object>, IDisposableComponent<IValidator>
    {
        public readonly ItemOrIReadOnlyList<IValidationRule> Rules;
        private readonly List<ValidationErrorInfo>? _errors;
        private readonly CancellationTokenSource? _disposeToken;

        public RuleValidationHandler(object target, ItemOrIReadOnlyList<IValidationRule> rules)
        {
            Should.NotBeNull(target, nameof(target));
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

            if (_disposeToken == null && rules.Count > 1)
                _errors = new List<ValidationErrorInfo>(rules.Count);
        }

        public object Target { get; }

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
            var editor = new ItemOrListEditor<ValidationErrorInfo>();
            ItemOrListEditor<ValueTask<ItemOrIReadOnlyList<ValidationErrorInfo>>> tasks = default;
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