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
        private readonly IComponentCollectionManager? _componentCollectionManager;
        private IComponentCollection? _components;
        private IPlatformInfo? _deviceInfo;

        public MugenApplication(IReadOnlyMetadataContext? metadata = null, IComponentCollectionManager? componentCollectionManager = null)
        {
            _componentCollectionManager = componentCollectionManager;
            Metadata = metadata.ToNonReadonly();
        }

        public bool HasComponents => _components != null && _components.Count != 0;

        public IComponentCollection Components => _components ?? _componentCollectionManager.EnsureInitialized(ref _components, this);

        public bool HasMetadata => Metadata.Count != 0;

        public IMetadataContext Metadata { get; }

        public EnumFlags<ApplicationFlags> Flags { get; private set; }

        public IPlatformInfo PlatformInfo
        {
            get
            {
                if (_deviceInfo == null)
                    ExceptionManager.ThrowObjectNotInitialized(this);
                return _deviceInfo;
            }
            private set => _deviceInfo = value;
        }

        public bool IsInState(ApplicationLifecycleState state, IReadOnlyMetadataContext? metadata = null) =>
            Components.Get<ILifecycleTrackerComponent<IMugenApplication, ApplicationLifecycleState>>(metadata).IsInState(this, this, state, metadata);

        public void OnUnhandledException(Exception exception, UnhandledExceptionType type, IReadOnlyMetadataContext? metadata = null) =>
            Components.Get<IUnhandledExceptionHandlerComponent>().OnUnhandledException(this, exception, type, metadata);

        public void OnLifecycleChanged(ApplicationLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata = null) =>
            Components.Get<IApplicationLifecycleListener>().OnLifecycleChanged(this, lifecycleState, state, metadata);

        public void Initialize(IPlatformInfo platformInfo, object? state = null, EnumFlags<ApplicationFlags> flags = default, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(platformInfo, nameof(platformInfo));
            Flags = flags;
            PlatformInfo = platformInfo;
            OnLifecycleChanged(ApplicationLifecycleState.Initializing, state, metadata);
            OnLifecycleChanged(ApplicationLifecycleState.Initialized, state, metadata);
            Flags |= ApplicationFlags.Initialized;
        }
    }
}