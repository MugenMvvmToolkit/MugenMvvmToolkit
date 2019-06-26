using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Validation
{
    public interface IValidatorProviderComponent : IComponent<IValidatorProvider>
    {
        IReadOnlyList<IValidator> GetValidators(IReadOnlyMetadataContext metadata);
    }
}