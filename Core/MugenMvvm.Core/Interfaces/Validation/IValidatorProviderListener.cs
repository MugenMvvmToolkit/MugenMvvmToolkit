using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Validation
{
    public interface IValidatorProviderListener : IListener
    {
        void OnValidatorCreated(IValidatorProvider provider, IValidator validator, IReadOnlyMetadataContext metadata);

        void OnAggregatorValidatorCreated(IValidatorProvider provider, IAggregatorValidator validator, IReadOnlyMetadataContext metadata);
    }
}