using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;

namespace MugenMvvm.Validation.Components
{
    public sealed class ValidatorProviderComponent : IValidatorProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IComponentCollectionManager? _componentCollectionManager;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ValidatorProviderComponent(IComponentCollectionManager? componentCollectionManager = null)
        {
            _componentCollectionManager = componentCollectionManager;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ValidationComponentPriority.ValidatorProvider;

        #endregion

        #region Implementation of interfaces

        public IValidator TryGetValidator(IValidationManager validationManager, object? request, IReadOnlyMetadataContext? metadata)
        {
            if (request is IHasTarget<IValidator> hasTarget)
                return hasTarget.Target;
            var validator = new Validator(metadata, _componentCollectionManager);
            validator.AddComponent(new CycleHandlerValidatorComponent());
            return validator;
        }

        #endregion
    }
}