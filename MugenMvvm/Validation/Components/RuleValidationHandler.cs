using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Validation;

namespace MugenMvvm.Validation.Components
{
    public sealed class RuleValidationHandler : ValidationHandlerBase, IDisposable
    {
        public readonly ItemOrIReadOnlyList<IValidationRule> Rules;
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
        }

        public object Target { get; }

        protected override ValueTask<ItemOrIReadOnlyList<ValidationErrorInfo>> ValidateAsync(IValidator validator, string? member, CancellationToken cancellationToken,
            IReadOnlyMetadataContext? metadata)
        {
            if (_disposeToken == null)
                return new ValueTask<ItemOrIReadOnlyList<ValidationErrorInfo>>(Validate(member, metadata));
            return ValidateInternalAsync(member,
                cancellationToken.CanBeCanceled ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _disposeToken.Token).Token : _disposeToken.Token, metadata);
        }

        private async ValueTask<ItemOrIReadOnlyList<ValidationErrorInfo>> ValidateInternalAsync(string? member, CancellationToken cancellationToken,
            IReadOnlyMetadataContext? metadata)
        {
            ItemOrListEditor<ValidationErrorInfo> editor = default;
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
            return editor;
        }

        private ItemOrIReadOnlyList<ValidationErrorInfo> Validate(string? member, IReadOnlyMetadataContext? metadata)
        {
            ItemOrListEditor<ValidationErrorInfo> editor = default;
            foreach (var rule in Rules)
            {
                var task = rule.ValidateAsync(Target, member, default, metadata);
                if (!task.IsCompletedSuccessfully)
                    ExceptionManager.ThrowObjectNotInitialized(task);

                editor.AddRange(task.Result);
            }

            return editor;
        }

        void IDisposable.Dispose() => _disposeToken?.Cancel();
    }
}