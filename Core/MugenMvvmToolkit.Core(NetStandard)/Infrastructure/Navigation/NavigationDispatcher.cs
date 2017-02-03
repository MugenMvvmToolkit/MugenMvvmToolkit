using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Infrastructure.Callbacks;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Infrastructure.Navigation
{
    public class NavigationDispatcher : INavigationDispatcher
    {
        #region Fields

        #endregion
//
//        #region Constructors
//
//        public NavigationDispatcher([NotNull] IOperationCallbackManager callbackManager)
//        {
//            Should.NotBeNull(callbackManager, nameof(callbackManager));
//            CallbackManager = callbackManager;
//        }
//
//        #endregion

        #region Properties

        protected IOperationCallbackManager CallbackManager { get; }

        #endregion

        #region Methods

        protected virtual Task<bool> TryNavigateFromAsync(INavigationContext context, object parameter)
        {
            //NOTE: Close view model only on back navigation.
            var closeableViewModel = context.NavigationMode == NavigationMode.Back
                ? context.ViewModelFrom as ICloseableViewModel
                : null;
            if (closeableViewModel == null)
                return OnNavigatingFrom(context) ?? Empty.TrueTask;
            var navigatingTask = OnNavigatingFrom(context) ?? Empty.TrueTask;
            if (navigatingTask.IsCompleted)
            {
                if (navigatingTask.Result)
                    return closeableViewModel.CloseAsync(parameter);
                return Empty.FalseTask;
            }
            return navigatingTask
                .TryExecuteSynchronously(task =>
                {
                    if (task.Result)
                        return closeableViewModel.CloseAsync(parameter);
                    return Empty.FalseTask;
                }).Unwrap();
        }

        protected virtual Task<bool> OnNavigatingFrom(INavigationContext context)
        {
            return (context.ViewModelFrom as INavigableViewModel)?.OnNavigatingFrom(context);
        }

        protected virtual void RaiseNavigated(INavigationContext context)
        {
            Navigated?.Invoke(this, new NavigatedEventArgs(context));
        }

        protected virtual void OnNavigatedInternal(INavigationContext context)
        {
            (context.ViewModelFrom as INavigableViewModel)?.OnNavigatedFrom(context);
            (context.ViewModelTo as INavigableViewModel)?.OnNavigatedTo(context);
//            if (context.NavigationMode == NavigationMode.Back && context.ViewModelFrom != null)
//            {
//                var result = ViewModelExtensions.GetOperationResult(context.ViewModelFrom);
//                var operationResult = OperationResult.CreateResult(null, context.ViewModelFrom, result, context);//todo type
//                CallbackManager.SetResult(operationResult);
//            }
        }

        #endregion

        #region Implementation of interfaces

        public event EventHandler<INavigationDispatcher, NavigatedEventArgs> Navigated;

        public Task<bool> NavigatingFromAsync(INavigationContext context, object parameter)
        {
            Should.NotBeNull(context, nameof(context));
            return TryNavigateFromAsync(context, parameter);
        }

        public void OnNavigated(INavigationContext context)
        {
            Should.NotBeNull(context, nameof(context));
            OnNavigatedInternal(context);
            RaiseNavigated(context);
        }

        #endregion
    }
}