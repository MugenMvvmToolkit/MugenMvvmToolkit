using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Validation
{
    public interface IValidationManager : IComponentOwner<IValidationManager>, IComponent<IMugenApplication>
    {
        IValidator? TryGetValidator<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata = null);
    }
}