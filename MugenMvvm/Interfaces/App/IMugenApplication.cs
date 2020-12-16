using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.App
{
    public interface IMugenApplication : IComponentOwner<IMugenApplication>, IMetadataOwner<IMetadataContext>
    {
        IPlatformInfo PlatformInfo { get; }

        bool IsInState(ApplicationLifecycleState state, IReadOnlyMetadataContext? metadata = null);

        void OnUnhandledException(Exception exception, UnhandledExceptionType type, IReadOnlyMetadataContext? metadata = null);

        void OnLifecycleChanged(ApplicationLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata = null);

        void Initialize(IPlatformInfo platformInfo, object? state, IReadOnlyMetadataContext? metadata = null);
    }
}