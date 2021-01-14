﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Validation
{
    public interface IValidator : IComponentOwner<IValidator>, IMetadataOwner<IMetadataContext>, IDisposable
    {
        bool HasErrors(string? memberName = null, IReadOnlyMetadataContext? metadata = null);

        ItemOrIReadOnlyList<object> GetErrors(string? memberName, IReadOnlyMetadataContext? metadata = null);

        IReadOnlyDictionary<string, object> GetErrors(IReadOnlyMetadataContext? metadata = null);

        Task ValidateAsync(string? memberName = null, CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null);

        void ClearErrors(string? memberName = null, IReadOnlyMetadataContext? metadata = null);
    }
}