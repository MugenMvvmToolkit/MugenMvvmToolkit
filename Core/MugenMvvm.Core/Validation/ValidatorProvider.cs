using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
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
            var validators = new List<IValidator>();
            var components = GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                var list = (components[i] as IValidatorProviderComponent)?.GetValidators(metadata);
                if (list != null && list.Count != 0)
                    validators.AddRange(list);
            }

            for (var i = 0; i < components.Length; i++)
            {
                if (!(components[i] is IValidatorProviderListener listener))
                    continue;

                for (var j = 0; j < validators.Count; j++)
                    listener.OnValidatorCreated(this, validators[j], metadata);
            }

            return validators;
        }

        public IAggregatorValidator GetAggregatorValidator(IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            IAggregatorValidator? result = null;
            var components = GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                result = (components[i] as IAggregatorValidatorProviderComponent)?.TryGetAggregatorValidator(metadata);
                if (result != null)
                    break;
            }

            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized(this, typeof(IAggregatorValidatorProviderComponent).Name);

            for (var i = 0; i < components.Length; i++)
                (components[i] as IValidatorProviderListener)?.OnAggregatorValidatorCreated(this, result!, metadata);
            return result;
        }

        #endregion
    }
}