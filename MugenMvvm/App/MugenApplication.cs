using System;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.App.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.App
{
    public sealed class MugenApplication : IMugenApplication
    {
        #region Fields

        private IComponentCollection? _components;
        private IPlatformInfo? _deviceInfo;

        #endregion

        #region Constructors

        public MugenApplication(IReadOnlyMetadataContext? metadata = null)
        {
            Metadata = metadata.ToNonReadonly();
            MugenService.Configuration.InitializeInstance<IMugenApplication>(this);
        }

        #endregion

        #region Properties

        public bool HasMetadata => Metadata.Count != 0;

        public bool HasComponents => _components != null && _components.Count != 0;

        public IMetadataContext Metadata { get; }

        public IComponentCollection Components => _components ?? MugenService.ComponentCollectionManager.EnsureInitialized(ref _components, this);

        public IPlatformInfo PlatformInfo
        {
            get => _deviceInfo ??= new PlatformInfo(PlatformType.Unknown);
            private set => _deviceInfo = value;
        }

        #endregion

        #region Implementation of interfaces

        public bool IsInState(ApplicationLifecycleState state, IReadOnlyMetadataContext? metadata = null) =>
            Components.Get<ILifecycleTrackerComponent<ApplicationLifecycleState>>(metadata).IsInState(this, this, state, metadata);

        public void OnUnhandledException(Exception exception, UnhandledExceptionType type, IReadOnlyMetadataContext? metadata = null) =>
            Components.Get<IUnhandledExceptionHandlerComponent>().OnUnhandledException(this, exception, type, metadata);

        public void OnLifecycleChanged(ApplicationLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata = null) =>
            Components.Get<IApplicationLifecycleListener>().OnLifecycleChanged(this, lifecycleState, state, metadata);

        public void Initialize(IPlatformInfo platformInfo, object? state, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(platformInfo, nameof(platformInfo));
            if (_deviceInfo != null && _deviceInfo.HasMetadata)
                platformInfo.Metadata.Merge(_deviceInfo.Metadata);
            PlatformInfo = platformInfo;
            OnLifecycleChanged(ApplicationLifecycleState.Initializing, state, metadata);
            OnLifecycleChanged(ApplicationLifecycleState.Initialized, state, metadata);
        }

        #endregion
    }
}