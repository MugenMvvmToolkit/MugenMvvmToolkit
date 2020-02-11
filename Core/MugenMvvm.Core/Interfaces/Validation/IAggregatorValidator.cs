using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Validation
{
    public interface IAggregatorValidator : IValidator, IComponentOwner<IAggregatorValidator>
    {
        void SetErrors(string memberName, IReadOnlyList<object>? errors, IReadOnlyMetadataContext? metadata = null);
    }
}