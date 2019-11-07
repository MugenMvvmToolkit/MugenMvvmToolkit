using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.App.Components
{
    public interface IApplicationStateListener : IComponent<IMugenApplication>
    {
        void OnStateChanged(IMugenApplication application, ApplicationState oldState, ApplicationState newState, IReadOnlyMetadataContext? metadata);
    }
}