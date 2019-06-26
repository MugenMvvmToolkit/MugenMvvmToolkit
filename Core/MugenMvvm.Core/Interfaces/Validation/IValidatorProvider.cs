using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Validation
{
    public interface IValidatorProvider : IComponentOwner<IValidatorProvider>
    {
        IReadOnlyList<IValidator> GetValidators(IReadOnlyMetadataContext metadata);

        IAggregatorValidator GetAggregatorValidator(IReadOnlyMetadataContext metadata);
    }
}