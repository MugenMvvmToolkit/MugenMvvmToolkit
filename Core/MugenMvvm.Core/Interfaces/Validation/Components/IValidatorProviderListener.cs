using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Validation.Components
{
    public interface IValidatorProviderListener : IComponent<IValidatorProvider>
    {
        void OnValidatorCreated<TRequest>(IValidatorProvider provider, IValidator validator, in TRequest request, IReadOnlyMetadataContext? metadata);

        void OnAggregatorValidatorCreated<TRequest>(IValidatorProvider provider, IAggregatorValidator validator, in TRequest request, IReadOnlyMetadataContext? metadata);
    }
}