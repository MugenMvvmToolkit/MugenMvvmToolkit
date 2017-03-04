using System;
using System.Threading.Tasks;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public class NavigationDispatcherMock : INavigationDispatcher
    {
        #region Properties

        public Func<INavigationContext, Task<bool>> OnNavigatingFromAsync { get; set; }

        public Action<INavigationContext> OnNavigated { get; set; }

        public Action<INavigationContext, Exception> OnNavigationFailed { get; set; }

        public Action<INavigationContext> OnNavigationCanceled { get; set; }

        #endregion

        #region Methods

        public void RaiseNavigated(NavigatedEventArgs args)
        {
            Navigated?.Invoke(this, args);
        }

        #endregion

        #region Implementation of interfaces

        Task<bool> INavigationDispatcher.OnNavigatingFromAsync(INavigationContext context)
        {
            return OnNavigatingFromAsync?.Invoke(context) ?? Empty.TrueTask;
        }

        void INavigationDispatcher.OnNavigated(INavigationContext context)
        {
            OnNavigated?.Invoke(context);
        }

        void INavigationDispatcher.OnNavigationFailed(INavigationContext context, Exception exception)
        {
            OnNavigationFailed?.Invoke(context, exception);
        }

        void INavigationDispatcher.OnNavigationCanceled(INavigationContext context)
        {
            OnNavigationCanceled?.Invoke(context);
        }

        public event EventHandler<INavigationDispatcher, NavigatedEventArgs> Navigated;

        #endregion
    }
}