using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Validation.Components
{
    public interface IValidatorProviderListener : IComponent<IValidatorProvider>
    {
        void OnValidatorCreated(IValidatorProvider provider, IValidator validator, IReadOnlyMetadataContext metadata);

        void OnAggregatorValidatorCreated(IValidatorProvider provider, IAggregatorValidator validator, IReadOnlyMetadataContext metadata);
    }
}