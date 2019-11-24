using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Validation
{
    public interface IAggregatorValidator : IValidator
    {
        IComponentCollection<IValidator> Validators { get; }

        void SetErrors(string memberName, IReadOnlyList<object> errors, IReadOnlyMetadataContext? metadata = null);
    }
}