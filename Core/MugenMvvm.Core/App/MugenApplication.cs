using MugenMvvm.Components;
using MugenMvvm.Enums;
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

        private IDeviceInfo? _deviceInfo;

        #endregion

        #region Constructors

        public MugenApplication()
        {
            Components = new ComponentCollection(this);
            Metadata = new MetadataContext();
            MugenService.Configuration.InitializeInstance<IMugenApplication>(this);
        }

        #endregion

        #region Properties

        public bool HasComponents => Components.Count != 0;

        public IComponentCollection Components { get; }

        public bool HasMetadata => Metadata.Count != 0;

        public IMetadataContext Metadata { get; }

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

        public void OnLifecycleChanged<TState>(ApplicationLifecycleState lifecycleState, in TState state, IReadOnlyMetadataContext? metadata = null)
        {
            Components.Get<IApplicationLifecycleDispatcherComponent>().OnLifecycleChanged(this, lifecycleState, state, metadata);
        }

        public void Initialize<TState>(IDeviceInfo device, in TState state, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(device, nameof(device));
            DeviceInfo = device;
            OnLifecycleChanged(ApplicationLifecycleState.Initializing, state, metadata);
            OnLifecycleChanged(ApplicationLifecycleState.Initialized, state, metadata);
        }

        public void Start<TState>(in TState state, IReadOnlyMetadataContext? metadata = null)
        {
            OnLifecycleChanged(ApplicationLifecycleState.Starting, state, metadata);
            OnLifecycleChanged(ApplicationLifecycleState.Started, state, metadata);
        }

        #endregion
    }
}