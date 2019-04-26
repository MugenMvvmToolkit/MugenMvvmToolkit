using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Validation
{
    public interface IValidatorProviderListener : IListener
    {
        void OnValidatorCreated(IValidatorProvider validatorProvider, IValidator validator, IReadOnlyMetadataContext metadata);

        void OnAggregatorValidatorCreated(IValidatorProvider validatorProvider, IAggregatorValidator validator, IReadOnlyMetadataContext metadata);
    }
}