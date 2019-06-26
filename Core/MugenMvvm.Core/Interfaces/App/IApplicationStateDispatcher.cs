using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.App
{
    public interface IApplicationStateDispatcher : IComponentOwner<IApplicationStateDispatcher>
    {
        ApplicationState State { get; }
    }
}