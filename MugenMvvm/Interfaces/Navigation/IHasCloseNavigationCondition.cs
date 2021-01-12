using System.Threading;
using System.Threading.Tasks;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface IHasCloseNavigationCondition
    {
        ValueTask<bool> CanCloseAsync(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, CancellationToken cancellationToken);
    }
}