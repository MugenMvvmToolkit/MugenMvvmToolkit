using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Validation
{
    public interface IAggregatorChildValidatorProvider : IChildValidatorProvider
    {
        IAggregatorValidator? TryGetAggregatorValidator(IValidatorProvider provider, IReadOnlyMetadataContext metadata);
    }
}