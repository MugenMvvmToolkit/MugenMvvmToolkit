using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Validation.Components
{
    public interface IValidatorProviderComponent : IComponent<IValidatorProvider>
    {
        IReadOnlyList<IValidator>? TryGetValidators<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata);
    }
}