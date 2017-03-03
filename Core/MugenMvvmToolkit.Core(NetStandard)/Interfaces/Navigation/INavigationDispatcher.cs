using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Interfaces.Navigation
{
    public interface INavigationDispatcher
    {
        Task<bool> OnNavigatingFromAsync([NotNull] INavigationContext context);

        void OnNavigated([NotNull]INavigationContext context);

        void OnNavigationFailed([NotNull]INavigationContext context, [NotNull] Exception exception);

        void OnNavigationCanceled([NotNull]INavigationContext context);

        event EventHandler<INavigationDispatcher, NavigatedEventArgs> Navigated;
    }
}