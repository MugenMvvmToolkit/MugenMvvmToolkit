using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Validation.Components
{
    public interface IValidatorProviderListener : IComponent<IValidationManager>
    {
        void OnValidatorCreated<TRequest>(IValidationManager provider, IValidator validator, in TRequest request, IReadOnlyMetadataContext? metadata);
    }
}