using System.ComponentModel;
using MugenMvvm.Attributes;
using MugenMvvm.Collections;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;

namespace MugenMvvm.Validation.Components
{
    public sealed class ValidatorProvider : IValidatorProviderComponent, IHasPriority
    {
        private readonly IComponentCollectionManager? _componentCollectionManager;

        [Preserve(Conditional = true)]
        public ValidatorProvider(IComponentCollectionManager? componentCollectionManager = null)
        {
            _componentCollectionManager = componentCollectionManager;
        }

        public bool AsyncValidationEnabled { get; set; }

        public int Priority { get; set; } = ValidationComponentPriority.ValidatorProvider;

        public IValidator TryGetValidator(IValidationManager validationManager, ItemOrIReadOnlyList<object> targets, IReadOnlyMetadataContext? metadata)
        {
            var validator = new Validator(metadata, _componentCollectionManager);
            validator.AddComponent(new ValidatorErrorManager());
            validator.AddComponent(new CycleHandlerValidatorBehavior());
            if (AsyncValidationEnabled)
                validator.AddComponent(new AsyncValidationBehavior());
            foreach (var target in targets)
            {
                if (target is INotifyPropertyChanged propertyChanged)
                    validator.AddComponent(new ObservableValidatorBehavior(propertyChanged));
            }

            return validator;
        }
    }
}