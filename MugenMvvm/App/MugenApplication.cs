using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.App.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;

namespace MugenMvvm.App
{
    public sealed class MugenApplication : IMugenApplication
    {
        #region Fields

        private IComponentCollection? _components;
        private IDeviceInfo? _deviceInfo;
        private IMetadataContext? _metadata;

        #endregion

        #region Constructors

        public MugenApplication()
        {
            MugenService.Configuration.InitializeInstance<IMugenApplication>(this);
        }

        #endregion

        #region Properties

        public bool HasMetadata => !_metadata.IsNullOrEmpty();

        public bool HasComponents => _components != null && _components.Count != 0;

        public IMetadataContext Metadata
        {
            get
            {
                if (_metadata == null)
                    MugenService.MetadataContextManager.LazyInitialize(ref _metadata, this);
                return _metadata;
            }
        }

        public IComponentCollection Components
        {
            get
            {
                if (_components == null)
                    MugenService.ComponentCollectionManager.LazyInitialize(ref _components, this);
                return _components;
            }
        }

        public IDeviceInfo DeviceInfo
        {
            get
            {
                if (_deviceInfo == null)
                    _deviceInfo = new DeviceInfo(PlatformType.Unknown, PlatformIdiom.Unknown, "0", new MetadataContext());
                return _deviceInfo;
            }
            private set => _deviceInfo = value;
        }

        #endregion

        #region Implementation of interfaces

        public void OnLifecycleChanged(ApplicationLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata = null)
        {
            Components.Get<IApplicationLifecycleDispatcherComponent>().OnLifecycleChanged(this, lifecycleState, state, metadata);
        }

        public void Initialize(IDeviceInfo device, object? state, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(device, nameof(device));
            DeviceInfo = device;
            OnLifecycleChanged(ApplicationLifecycleState.Initializing, state, metadata);
            OnLifecycleChanged(ApplicationLifecycleState.Initialized, state, metadata);
        }

        #endregion
    }
}