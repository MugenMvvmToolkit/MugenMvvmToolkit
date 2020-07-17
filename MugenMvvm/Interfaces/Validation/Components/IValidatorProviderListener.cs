using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Validation.Components
{
    public interface IValidatorProviderListener : IComponent<IValidationManager>
    {
        void OnValidatorCreated(IValidationManager validationManager, IValidator validator, object? request, IReadOnlyMetadataContext? metadata);
    }
}