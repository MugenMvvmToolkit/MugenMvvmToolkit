using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Validation
{
    public interface IChildValidatorProvider : IHasPriority
    {
        IReadOnlyList<IValidator> GetValidators(IValidatorProvider provider, IReadOnlyMetadataContext metadata);
    }
}