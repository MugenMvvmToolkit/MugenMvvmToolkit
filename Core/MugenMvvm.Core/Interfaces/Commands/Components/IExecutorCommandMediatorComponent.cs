using System.Threading.Tasks;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Commands.Components
{
    public interface IExecutorCommandMediatorComponent : IComponent<ICommandMediator>
    {
        Task ExecuteAsync(object? parameter);
    }
}