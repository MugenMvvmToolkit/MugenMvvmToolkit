using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.App
{
    public interface IApplicationStateDispatcher : IHasListeners<IApplicationStateDispatcherListener>
    {
        ApplicationState State { get; }
    }
}