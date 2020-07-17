using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Validation.Components
{
    public interface IValidatorProviderComponent : IComponent<IValidationManager>
    {
        IValidator? TryGetValidator(IValidationManager validationManager, object? request, IReadOnlyMetadataContext? metadata);
    }
}