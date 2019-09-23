﻿using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.App
{
    public interface IApplicationStateDispatcher : IComponentOwner<IApplicationStateDispatcher>, IComponent<IMugenApplication>//todo merge with IMugenApp + add listener app initialized
    {
        ApplicationState State { get; }

        void SetApplicationState(ApplicationState state, IReadOnlyMetadataContext? metadata = null);
    }
}