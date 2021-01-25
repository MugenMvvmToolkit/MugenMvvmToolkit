using System;
using System.Collections.Generic;
using MugenMvvm.Collections;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;

namespace MugenMvvm.Validation.Components
{
    public sealed class RuleValidationManager : IValidationManagerListener, IHasPriority
    {
        private readonly List<(IValidationRule rule, Func<IValidator, object, IReadOnlyMetadataContext?, bool> condition)> _rules;

        public RuleValidationManager() : this(true)
        {
        }

        public RuleValidationManager(bool useCache)
        {
            _rules = new List<(IValidationRule rule, Func<IValidator, object, IReadOnlyMetadataContext?, bool> condition)>();
            UseCache = useCache;
        }

        public bool UseCache { get; }

        public int Priority { get; set; } = ValidationComponentPriority.ValidatorProvider;

        public void AddRule(IValidationRule rule, Func<IValidator, object, IReadOnlyMetadataContext?, bool> condition) => AddRules(ItemOrIReadOnlyList.FromItem(rule), condition);

        public void AddRules(ItemOrIReadOnlyList<IValidationRule> rules, Func<IValidator, object, IReadOnlyMetadataContext?, bool> condition)
        {
            Should.NotBeNull(condition, nameof(condition));
            foreach (var rule in rules)
                _rules.Add((rule, condition));
        }

        public void OnValidatorCreated(IValidationManager validationManager, IValidator validator, ItemOrIReadOnlyList<object> targets, IReadOnlyMetadataContext? metadata)
        {
            foreach (var target in targets)
            {
                var component = TryGetComponent(validator, target, metadata);
                if (component != null)
                    validator.AddComponent(component);
            }
        }

        private RuleValidationHandler? TryGetComponent(IValidator validator, object? target, IReadOnlyMetadataContext? metadata)
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
            return new RuleValidationHandler(target, rules.ToItemOrList(), UseCache);
        }
    }
}