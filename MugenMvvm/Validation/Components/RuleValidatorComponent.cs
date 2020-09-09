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
            : base(target, false)
        {
            Rules = rules;
            foreach (var rule in rules.Iterator())
            {
                if (rule.IsAsync)
                {
                    HasAsyncValidation = true;
                    break;
                }
            }
        }

        #endregion

        #region Methods

        protected override ValueTask<ValidationResult> GetErrorsAsync(string memberName, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            var errors = new Dictionary<string, object?>(3);
            var tasks = ItemOrListEditor.Get<Task>();
            foreach (var rule in Rules.Iterator())
            {
                var task = rule.ValidateAsync(Target, memberName, errors, cancellationToken, metadata);
                if (task != null && !task.IsCompleted)
                    tasks.Add(task);
            }

            if (tasks.Count == 0)
                return new ValueTask<ValidationResult>(ValidationResult.Get(errors));

            var result = tasks.ToItemOrList().WhenAll().ContinueWith((task, e) =>
            {
                task.Wait(); //rethrow if error
                return ValidationResult.Get((IReadOnlyDictionary<string, object?>) e!);
            }, errors, cancellationToken);
            return new ValueTask<ValidationResult>(result);
        }

        #endregion
    }
}