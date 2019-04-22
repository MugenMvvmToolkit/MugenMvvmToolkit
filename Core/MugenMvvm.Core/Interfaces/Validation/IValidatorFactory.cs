using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Validation
{
    public interface IValidatorFactory : IHasPriority
    {
        IReadOnlyList<IValidator> GetValidators(IReadOnlyMetadataContext metadata);
    }
}