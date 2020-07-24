using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.App
{
    public interface IMugenApplication : IComponentOwner<IMugenApplication>, IMetadataOwner<IMetadataContext>
    {
        IDeviceInfo DeviceInfo { get; }

        void OnUnhandledException(Exception exception, UnhandledExceptionType type, IReadOnlyMetadataContext? metadata = null);

        void OnLifecycleChanged(ApplicationLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata = null);

        void Initialize(IDeviceInfo device, object? state, IReadOnlyMetadataContext? metadata = null);
    }
}