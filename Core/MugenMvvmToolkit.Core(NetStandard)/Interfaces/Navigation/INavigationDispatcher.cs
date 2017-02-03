using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Interfaces.Navigation
{
    public interface INavigationDispatcher
    {
        Task<bool> NavigatingFromAsync([NotNull] INavigationContext context, [CanBeNull] object parameter);

        void OnNavigated([NotNull]INavigationContext context);

        event EventHandler<INavigationDispatcher, NavigatedEventArgs> Navigated;
    }
}