using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;

namespace MugenMvvm.Validation
{
    public sealed class ValidatorProvider : ComponentOwnerBase<IValidatorProvider>, IValidatorProvider
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public ValidatorProvider(IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Implementation of interfaces

        public IReadOnlyList<IValidator> GetValidators(IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            var validators = GetComponents<IValidatorProviderComponent>(metadata).TryGetValidators(metadata);

            if (validators != null)
            {
                var listeners = GetComponents<IValidatorProviderListener>(metadata);
                for (var i = 0; i < validators.Count; i++)
                    listeners.OnValidatorCreated(this, validators[i], metadata);
            }

            return validators ?? Default.EmptyArray<IValidator>();
        }

        public IAggregatorValidator GetAggregatorValidator(IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            var result = GetComponents<IAggregatorValidatorProviderComponent>(metadata).TryGetAggregatorValidator(metadata);
            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized(this);

            GetComponents<IValidatorProviderListener>(metadata).OnAggregatorValidatorCreated(this, result, metadata);
            return result;
        }

        #endregion
    }
}