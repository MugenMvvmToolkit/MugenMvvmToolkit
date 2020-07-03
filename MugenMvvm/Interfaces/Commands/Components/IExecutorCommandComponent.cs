using System.Threading.Tasks;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Commands.Components
{
    public interface IExecutorCommandComponent : IComponent<ICompositeCommand>
    {
        Task ExecuteAsync(ICompositeCommand command, object? parameter);
    }
}