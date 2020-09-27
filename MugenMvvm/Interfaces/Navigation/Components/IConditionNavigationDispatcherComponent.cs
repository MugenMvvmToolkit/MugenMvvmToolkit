using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Navigation.Components
{
    public interface IConditionNavigationDispatcherComponent : IComponent<INavigationDispatcher>
    {
        Task<bool> CanNavigateAsync(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, CancellationToken cancellationToken);
    }
}