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

        private readonly IComponentCollectionProvider? _componentCollectionProvider;
        private readonly IMetadataContextProvider? _metadataContextProvider;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ValidatorProviderComponent(IComponentCollectionProvider? componentCollectionProvider = null, IMetadataContextProvider? metadataContextProvider = null)
        {
            _componentCollectionProvider = componentCollectionProvider;
            _metadataContextProvider = metadataContextProvider;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ValidationComponentPriority.ValidatorProvider;

        #endregion

        #region Implementation of interfaces

        public IValidator? TryGetValidator<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            return new Validator(null, _componentCollectionProvider, _metadataContextProvider);
        }

        #endregion
    }
}