using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Models.Components;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;

namespace MugenMvvm.Validation.Components
{
    public sealed class RuleValidationHandler : IValidationHandlerComponent, IHasPriority, IHasTarget<object>, IDisposableComponent<IValidator>, IAttachableComponent,
        IDetachableComponent //todo review pooled, change rules, set error as rule, opt error manager, cancel for sync
    {
        public readonly ItemOrIReadOnlyList<IValidationRule> Rules;
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
        }

        public int Priority { get; init; }

        public object Target { get; }

        public async Task TryValidateAsync(IValidator validator, string? member, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            if (_disposeToken == null)
            {
                ValidateSync(validator, member, metadata);
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

        public Task WaitAsync(IValidator validator, string? member, IReadOnlyMetadataContext? metadata) => Task.CompletedTask;

        private async Task ValidateAsync(IValidator validator, string? member, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            var tasks = new ItemOrListEditor<(IValidationRule, Task<ItemOrIReadOnlyList<ValidationErrorInfo>>)>(2);
            foreach (var rule in Rules)
            {
                var task = rule.ValidateAsync(validator, Target, member, cancellationToken, metadata);
                if (task.IsCompleted)
                    validator.SetErrors(rule, task.Result, metadata);
                else
                    tasks.Add((rule, task.AsTask()));
            }

            foreach (var task in tasks)
                validator.SetErrors(task.Item1, await task.Item2.ConfigureAwait(false), metadata);
        }

        private void ValidateSync(IValidator validator, string? member, IReadOnlyMetadataContext? metadata)
        {
            lock (this)
            {
                foreach (var rule in Rules)
                {
                    var task = rule.ValidateAsync(validator, Target, member, default, metadata);
                    if (!task.IsCompleted)
                        ExceptionManager.ThrowObjectNotInitialized(rule);

                    validator.SetErrors(rule, task.Result, metadata);
                }
            }
        }

        void IAttachableComponent.OnAttaching(object owner, IReadOnlyMetadataContext? metadata)
        {
        }

        void IAttachableComponent.OnAttached(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (owner is IValidator validator)
                validator.ValidateAsync(metadata: metadata);
        }

        void IDetachableComponent.OnDetaching(object owner, IReadOnlyMetadataContext? metadata)
        {
        }

        void IDetachableComponent.OnDetached(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (owner is IValidator validator)
            {
                foreach (var rule in Rules)
                    validator.ClearErrors(default, rule, metadata);
            }
        }

        void IDisposableComponent<IValidator>.OnDisposing(IValidator owner, IReadOnlyMetadataContext? metadata)
        {
        }

        void IDisposableComponent<IValidator>.OnDisposed(IValidator owner, IReadOnlyMetadataContext? metadata)
        {
            _disposeToken?.Cancel();
            foreach (var rule in Rules)
                rule.Dispose();
        }
    }
}