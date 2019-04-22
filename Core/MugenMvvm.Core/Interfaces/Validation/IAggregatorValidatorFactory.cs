using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Validation
{
    public interface IAggregatorValidatorFactory : IValidatorFactory
    {
        IAggregatorValidator? TryGetAggregatorValidator(IReadOnlyMetadataContext metadata);
    }
}