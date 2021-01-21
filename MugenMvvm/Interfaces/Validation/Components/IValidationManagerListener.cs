using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Validation.Components
{
    public interface IValidationManagerListener : IComponent<IValidationManager>
    {
        void OnValidatorCreated(IValidationManager validationManager, IValidator validator, ItemOrIReadOnlyList<object> targets, IReadOnlyMetadataContext? metadata);
    }
}