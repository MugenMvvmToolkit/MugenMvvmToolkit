using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Validation;

namespace MugenMvvm.Infrastructure.Validation
{
    public class ValidatorProvider : IValidatorProvider
    {
        #region Fields

        private IComponentCollection<IValidatorProviderListener>? _listeners;
        private IComponentCollection<IValidatorFactory>? _validatorFactories;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ValidatorProvider(IComponentCollectionProvider componentCollectionProvider)
        {
            Should.NotBeNull(componentCollectionProvider, nameof(componentCollectionProvider));
            ComponentCollectionProvider = componentCollectionProvider;
        }

        #endregion

        #region Properties

        protected IComponentCollectionProvider ComponentCollectionProvider { get; }

        public bool IsListenersInitialized => _listeners != null;

        public IComponentCollection<IValidatorProviderListener> Listeners
        {
            get
            {
                if (_listeners == null)
                    ComponentCollectionProvider.LazyInitialize(ref _listeners, this);
                return _listeners;
            }
        }

        public IComponentCollection<IValidatorFactory> ValidatorFactories
        {
            get
            {
                if (_validatorFactories == null)
                    ComponentCollectionProvider.LazyInitialize(ref _validatorFactories, this);
                return _validatorFactories;
            }
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
                ExceptionManager.ThrowObjectNotInitialized(this, typeof(IAggregatorValidatorFactory).Name);

            OnAggregatorValidatorCreated(result, metadata);
            return result;
        }

        #endregion

        #region Methods

        protected virtual IReadOnlyList<IValidator> GetValidatorsInternal(IReadOnlyMetadataContext metadata)
        {
            var validators = new List<IValidator>();
            var items = ValidatorFactories.GetItems();
            for (var i = 0; i < items.Length; i++)
            {
                var list = items[i].GetValidators(metadata);
                if (list != null)
                    validators.AddRange(list);
            }

            return validators;
        }

        protected virtual IAggregatorValidator? GetAggregatorValidatorInternal(IReadOnlyMetadataContext metadata)
        {
            var items = ValidatorFactories.GetItems();
            for (var i = 0; i < items.Length; i++)
            {
                if (items[i] is IAggregatorValidatorFactory aggregatorValidatorFactory)
                {
                    var validator = aggregatorValidatorFactory.TryGetAggregatorValidator(metadata);
                    if (validator != null)
                        return validator;
                }
            }

            return null;
        }

        protected virtual void OnValidatorsCreated(IReadOnlyList<IValidator> validators, IReadOnlyMetadataContext metadata)
        {
            var listeners = this.GetListeners();
            if (listeners.Length == 0 || validators.Count == 0)
                return;
            for (var i = 0; i < validators.Count; i++)
                for (var j = 0; j < listeners.Length; j++)
                    listeners[j].OnValidatorCreated(this, validators[i], metadata);
        }

        protected virtual void OnAggregatorValidatorCreated(IAggregatorValidator validator, IReadOnlyMetadataContext metadata)
        {
            var listeners = this.GetListeners();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnValidatorCreated(this, validator, metadata);
        }

        #endregion
    }
}