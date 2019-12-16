using System.Collections.Generic;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Validation
{
    public interface IValidatorProvider : IComponentOwner<IValidatorProvider>, IComponent<IMugenApplication>
    {
        IReadOnlyList<IValidator> GetValidators<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata = null);

        IAggregatorValidator GetAggregatorValidator<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata = null);
    }
}