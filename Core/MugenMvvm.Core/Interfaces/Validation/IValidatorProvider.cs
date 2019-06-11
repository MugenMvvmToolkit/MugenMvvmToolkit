using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Validation
{
    public interface IValidatorProvider : IHasListeners<IValidatorProviderListener>
    {
        IComponentCollection<IChildValidatorProvider> Providers { get; }

        IReadOnlyList<IValidator> GetValidators(IReadOnlyMetadataContext metadata);

        IAggregatorValidator GetAggregatorValidator(IReadOnlyMetadataContext metadata);
    }
}