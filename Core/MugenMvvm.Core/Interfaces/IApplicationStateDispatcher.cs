using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces
{
    public interface IApplicationStateDispatcher : IHasListeners<IApplicationStateDispatcherListener>
    {
        ApplicationState State { get; }
    }
}