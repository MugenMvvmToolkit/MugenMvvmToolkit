using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Validation;

namespace MugenMvvm.Validation.Components
{
    public sealed class RuleValidatorComponent : ValidatorComponentBase<object>
    {
        public readonly ItemOrIReadOnlyList<IValidationRule> Rules;

        private readonly bool _isAsync;

        public RuleValidatorComponent(object target, ItemOrIReadOnlyList<IValidationRule> rules)
            : base(target)
        {
            Rules = rules;
            foreach (var rule in rules)
                if (rule.IsAsync)
                {
                    _isAsync = true;
                    break;
                }
        }

        protected override CancellationToken GetCancellationToken(string memberName, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata) =>
            _isAsync ? base.GetCancellationToken(memberName, cancellationToken, metadata) : cancellationToken;

        protected override async ValueTask<ValidationResult> GetErrorsAsync(string memberName, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            var errors = new Dictionary<string, object?>(3);
            var tasks = new ItemOrListEditor<Task>();
            foreach (var rule in Rules)
            {
                var task = rule.ValidateAsync(Target, memberName, errors, cancellationToken, metadata);
                if (!task.IsCompleted)
                    tasks.Add(task);
            }

            if (tasks.Count == 0)
                return ValidationResult.Get(errors);

            await tasks.WhenAll().ConfigureAwait(false);
            return ValidationResult.Get(errors);
        }
    }
}