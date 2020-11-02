using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Internal;

namespace MugenMvvm.Validation.Components
{
    public sealed class RuleValidatorComponent : ValidatorComponentBase<object>
    {
        #region Fields

        public readonly ItemOrList<IValidationRule, IReadOnlyList<IValidationRule>> Rules;

        #endregion

        #region Constructors

        public RuleValidatorComponent(object target, ItemOrList<IValidationRule, IReadOnlyList<IValidationRule>> rules)
            : base(target)
        {
            Rules = rules;
        }

        #endregion

        #region Methods

        protected override async ValueTask<ValidationResult> GetErrorsAsync(string memberName, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            var errors = new Dictionary<string, object?>(3);
            var tasks = ItemOrListEditor.Get<Task>();
            foreach (var rule in Rules)
            {
                var task = rule.ValidateAsync(Target, memberName, errors, cancellationToken, metadata);
                if (task != null && !task.IsCompleted)
                    tasks.Add(task);
            }

            if (tasks.Count == 0)
                return ValidationResult.Get(errors);

            await tasks.WhenAll().ConfigureAwait(false);
            return ValidationResult.Get(errors);
        }

        #endregion
    }
}