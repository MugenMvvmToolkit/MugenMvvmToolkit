using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.App
{
    public interface IApplicationStateDispatcher : IComponentOwner<IApplicationStateDispatcher>
    {
        ApplicationState State { get; }

        void SetApplicationState(ApplicationState state, IReadOnlyMetadataContext? metadata = null);
    }
}