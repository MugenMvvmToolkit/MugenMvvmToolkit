using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Validation
{
    public interface IValidatorProviderListener : IListener
    {
        void OnValidatorsCreated(IValidatorProvider validatorProvider, IReadOnlyList<IValidator> validators, IReadOnlyMetadataContext metadata);

        void OnAggregatorValidatorCreated(IValidatorProvider validatorProvider, IAggregatorValidator validator, IReadOnlyMetadataContext metadata);
    }
}