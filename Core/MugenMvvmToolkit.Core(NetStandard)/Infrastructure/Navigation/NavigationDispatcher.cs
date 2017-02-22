using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure.Callbacks;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Infrastructure.Navigation
{
    public class NavigationDispatcher : INavigationDispatcher
    {
        #region Constructors

        public NavigationDispatcher([NotNull] IOperationCallbackManager callbackManager)
        {
            Should.NotBeNull(callbackManager, nameof(callbackManager));
            CallbackManager = callbackManager;
        }

        #endregion

        #region Properties

        protected IOperationCallbackManager CallbackManager { get; }

        #endregion

        #region Methods

        protected virtual Task<bool> NavigatingFromInternalAsync(INavigationContext context)
        {
            bool data;
            if (context.TryGetData(NavigationConstants.ImmediateClose, out data) && data)
                return Empty.TrueTask;
            bool isClose = context.NavigationMode.IsClose() && context.ViewModelFrom != null;
            if (!isClose)
                return OnNavigatingFrom(context) ?? Empty.TrueTask;
            var navigatingTask = OnNavigatingFrom(context) ?? Empty.TrueTask;
            if (navigatingTask.IsCompleted)
            {
                if (navigatingTask.Result)
                    return OnClosingAsync(context.ViewModelFrom, context);
                return Empty.FalseTask;
            }
            return navigatingTask
                .TryExecuteSynchronously(task =>
                {
                    if (task.Result)
                        return OnClosingAsync(context.ViewModelFrom, context);
                    return Empty.FalseTask;
                }).Unwrap();
        }

        protected virtual Task<bool> OnNavigatingFrom(INavigationContext context)
        {
            return (context.ViewModelFrom as INavigableViewModel)?.OnNavigatingFrom(context);
        }

        protected virtual void OnNavigatedInternal(INavigationContext context)
        {
            (context.ViewModelFrom as INavigableViewModel)?.OnNavigatedFrom(context);
            (context.ViewModelTo as INavigableViewModel)?.OnNavigatedTo(context);
            if (context.NavigationMode.IsClose() && context.ViewModelFrom != null)
            {
                OnClosed(context.ViewModelFrom, context);
                if (context.NavigationType.Operation != null && !context.GetData(NavigationConstants.SuppressNavigationCallbackOnClose))
                {
                    var result = ViewModelExtensions.GetOperationResult(context.ViewModelFrom);
                    var operationResult = OperationResult.CreateResult(context.NavigationType.Operation, context.ViewModelFrom, result, context);
                    CallbackManager.SetResult(operationResult);
                }
            }
        }

        protected virtual void OnNavigationFailedInternal(INavigationContext context, Exception exception)
        {
            var viewModel = context.NavigationMode.IsClose() ? context.ViewModelFrom : context.ViewModelTo;
            if (viewModel != null && context.NavigationType.Operation != null)
                CallbackManager.SetResult(OperationResult.CreateErrorResult<bool?>(context.NavigationType.Operation, viewModel, exception, context));
        }

        protected virtual void OnNavigationCanceledInternal(INavigationContext context)
        {
            var viewModel = context.NavigationMode.IsClose() ? context.ViewModelFrom : context.ViewModelTo;
            if (viewModel != null && context.NavigationType.Operation != null)
                CallbackManager.SetResult(OperationResult.CreateCancelResult<bool?>(context.NavigationType.Operation, viewModel, context));//todo change result
        }

        protected virtual void RaiseNavigated(INavigationContext context)
        {
            Navigated?.Invoke(this, new NavigatedEventArgs(context));
        }

        protected virtual Task<bool> OnClosingAsync(IViewModel viewModel, IDataContext context)
        {
            var handler = viewModel.Settings.Metadata.GetData(ViewModelConstants.ClosingEvent);
            var closingTask = (viewModel as ICloseableViewModel)?.OnClosingAsync(context) ?? Empty.TrueTask;
            if (handler == null)
                return closingTask;
            if (closingTask.IsCompleted)
            {
                var args = new ViewModelClosingEventArgs(viewModel, context);
                handler(viewModel, args);
                return args.GetCanCloseAsync();
            }
            return closingTask.TryExecuteSynchronously(task =>
            {
                if (!task.Result)
                    return Empty.FalseTask;
                var args = new ViewModelClosingEventArgs(viewModel, context);
                handler(viewModel, args);
                return args.GetCanCloseAsync();
            }).Unwrap();
        }

        protected virtual void OnClosed(IViewModel viewModel, IDataContext context)
        {
            (viewModel as ICloseableViewModel)?.OnClosed(context);
            viewModel.Settings.Metadata.GetData(ViewModelConstants.ClosedEvent)?.Invoke(viewModel, new ViewModelClosedEventArgs(viewModel, context));
        }

        #endregion

        #region Implementation of interfaces

        public event EventHandler<INavigationDispatcher, NavigatedEventArgs> Navigated;

        public Task<bool> NavigatingFromAsync(INavigationContext context)
        {
            Should.NotBeNull(context, nameof(context));
            return NavigatingFromInternalAsync(context);
        }

        public void OnNavigated(INavigationContext context)
        {
            Should.NotBeNull(context, nameof(context));
            OnNavigatedInternal(context);
            RaiseNavigated(context);
        }

        public void OnNavigationFailed(INavigationContext context, Exception exception)
        {
            Should.NotBeNull(context, nameof(context));
            Should.NotBeNull(exception, nameof(exception));
            OnNavigationFailedInternal(context, exception);
        }

        public void OnNavigationCanceled(INavigationContext context)
        {
            Should.NotBeNull(context, nameof(context));
            OnNavigationCanceledInternal(context);
        }

        #endregion
    }
}