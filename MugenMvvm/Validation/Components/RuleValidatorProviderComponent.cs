using System;
using System.Collections.Generic;
using MugenMvvm.Collections;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Validation.Components
{
    public sealed class RuleValidatorProviderComponent : IValidatorProviderListener, IHasPriority
    {
        #region Fields

        private readonly List<(IValidationRule rule, Func<IValidator, object, IReadOnlyMetadataContext?, bool> condition)> _rules;

        #endregion

        #region Constructors

        public RuleValidatorProviderComponent()
        {
            _rules = new List<(IValidationRule rule, Func<IValidator, object, IReadOnlyMetadataContext?, bool> condition)>();
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ValidationComponentPriority.ValidatorProvider;

        #endregion

        #region Implementation of interfaces

        public void OnValidatorCreated(IValidationManager validationManager, IValidator validator, object? request, IReadOnlyMetadataContext? metadata)
        {
            if (request is IReadOnlyList<object?> targets)
            {
                for (var i = 0; i < targets.Count; i++)
                {
                    var component = TryGetComponent(validator, targets[i], metadata);
                    if (component != null)
                        validator.AddComponent(component);
                }
            }
            else
            {
                var component = TryGetComponent(validator, request, metadata);
                if (component != null)
                    validator.AddComponent(component);
            }
        }

        #endregion

        #region Methods

        public void AddRule(IValidationRule rule, Func<IValidator, object, IReadOnlyMetadataContext?, bool> condition) => AddRules(ItemOrIReadOnlyList.FromItem(rule), condition);

        public void AddRules(ItemOrIReadOnlyList<IValidationRule> rules, Func<IValidator, object, IReadOnlyMetadataContext?, bool> condition)
        {
            Should.NotBeNull(condition, nameof(condition));
            foreach (var rule in rules)
                _rules.Add((rule, condition));
        }

        private RuleValidatorComponent? TryGetComponent(IValidator validator, object? target, IReadOnlyMetadataContext? metadata)
        {
            if (target == null)
                return null;

            var rules = new ItemOrListEditor<IValidationRule>();
            for (var i = 0; i < _rules.Count; i++)
            {
                if (_rules[i].condition(validator, target, metadata))
                    rules.Add(_rules[i].rule);
            }

            if (rules.Count == 0)
                return null;
            return new RuleValidatorComponent(target, rules.ToItemOrList());
        }

        #endregion
    }
}