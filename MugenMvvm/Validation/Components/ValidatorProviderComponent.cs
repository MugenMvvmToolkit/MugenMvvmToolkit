using MugenMvvm.Attributes;
using MugenMvvm.Constants;
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
        private readonly IMetadataContextManager? _metadataContextManager;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ValidatorProviderComponent(IComponentCollectionManager? componentCollectionManager = null, IMetadataContextManager? metadataContextManager = null)
        {
            _componentCollectionManager = componentCollectionManager;
            _metadataContextManager = metadataContextManager;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ValidationComponentPriority.ValidatorProvider;

        #endregion

        #region Implementation of interfaces

        public IValidator? TryGetValidator<TRequest>(IValidationManager validationManager, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            return new Validator(null, _componentCollectionManager, _metadataContextManager);
        }

        #endregion
    }
}