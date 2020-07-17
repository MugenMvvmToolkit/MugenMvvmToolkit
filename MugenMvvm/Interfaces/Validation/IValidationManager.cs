﻿using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Validation
{
    public interface IValidationManager : IComponentOwner<IValidationManager>
    {
        IValidator? TryGetValidator(object? request = null, IReadOnlyMetadataContext? metadata = null);
    }
}