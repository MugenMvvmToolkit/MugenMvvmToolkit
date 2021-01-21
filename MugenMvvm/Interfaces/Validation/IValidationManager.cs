using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Validation
{
    public interface IValidationManager : IComponentOwner<IValidationManager>
    {
        IValidator? TryGetValidator(ItemOrIReadOnlyList<object> targets = default, IReadOnlyMetadataContext? metadata = null);
    }
}