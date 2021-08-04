﻿using JetBrains.Annotations;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Models
{
    public interface ISuspendable
    {
        bool IsSuspended { get; }

        [MustUseReturnValue]
        ActionToken Suspend(IReadOnlyMetadataContext? metadata = null);
    }
}