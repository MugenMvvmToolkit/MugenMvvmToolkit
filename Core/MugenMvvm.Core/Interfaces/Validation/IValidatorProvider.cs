using System.Collections.Generic;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Validation
{
    public interface IValidatorProvider : IComponentOwner<IValidatorProvider>, IComponent<IMugenApplication>
    {
        IReadOnlyList<IValidator> GetValidators(IReadOnlyMetadataContext metadata);

        IAggregatorValidator GetAggregatorValidator(IReadOnlyMetadataContext metadata);
    }
}