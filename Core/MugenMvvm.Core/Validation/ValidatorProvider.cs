using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;

namespace MugenMvvm.Validation
{
    public class ValidatorProvider : ComponentOwnerBase<IValidatorProvider>, IValidatorProvider
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
            var validators = GetValidatorsInternal(metadata) ?? Default.EmptyArray<IValidator>();
            OnValidatorsCreated(validators, metadata);
            return validators;
        }

        public IAggregatorValidator GetAggregatorValidator(IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            var result = GetAggregatorValidatorInternal(metadata);

            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized(this, typeof(IAggregatorValidatorProviderComponent).Name);

            OnAggregatorValidatorCreated(result!, metadata);
            return result!;
        }

        #endregion

        #region Methods

        protected virtual IReadOnlyList<IValidator> GetValidatorsInternal(IReadOnlyMetadataContext metadata)
        {
            var validators = new List<IValidator>();
            var components = GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                var list = (components[i] as IValidatorProviderComponent)?.GetValidators(metadata);
                if (list != null && list.Count != 0)
                    validators.AddRange(list);
            }

            return validators;
        }

        protected virtual IAggregatorValidator? GetAggregatorValidatorInternal(IReadOnlyMetadataContext metadata)
        {
            var components = GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                var validator = (components[i] as IAggregatorValidatorProviderComponent)?.TryGetAggregatorValidator(metadata);
                if (validator != null)
                    return validator;
            }

            return null;
        }

        protected virtual void OnValidatorsCreated(IReadOnlyList<IValidator> validators, IReadOnlyMetadataContext metadata)
        {
            var components = GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                if (!(components[i] is IValidatorProviderListener listener))
                    continue;

                for (var j = 0; j < validators.Count; j++)
                    listener.OnValidatorCreated(this, validators[j], metadata);
            }
        }

        protected virtual void OnAggregatorValidatorCreated(IAggregatorValidator validator, IReadOnlyMetadataContext metadata)
        {
            var components = GetComponents();
            for (var i = 0; i < components.Length; i++)
                (components[i] as IValidatorProviderListener)?.OnAggregatorValidatorCreated(this, validator, metadata);
        }

        #endregion
    }
}