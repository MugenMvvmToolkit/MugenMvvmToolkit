using System.Collections.Generic;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Commands.Mediators
{
    public interface IExecutorRelayCommandMediator : IConditionRelayCommandMediator, IConditionEventRelayCommandMediator, ISuspendNotifications
    {
        IReadOnlyList<IRelayCommandMediator> Mediators { get; }

        Task ExecuteAsync(object? parameter);
    }
}