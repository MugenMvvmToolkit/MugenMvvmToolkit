#region Copyright
// ****************************************************************************
// <copyright file="WindowViewMediatorBase.cs">
// Copyright © Vyacheslav Volkov 2012-2014
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************
#endregion
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure.Callbacks;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Mediators;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Infrastructure.Mediators
{
    /// <summary>
    ///     Represents the base mediator class for dialog view.
    /// </summary>
    public abstract class WindowViewMediatorBase<TView> : IWindowViewMediator
        where TView : class, IWindowViewBase
    {
        #region Fields

        private readonly IThreadManager _threadManager;
        private readonly IViewManager _viewManager;
        private readonly IOperationCallbackManager _operationCallbackManager;
        private readonly IViewModel _viewModel;
        private CancelEventArgs _cancelArgs;
        private object _closeParameter;
        private bool _isOpen;
        private bool _shouldClose;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="WindowViewMediatorBase{TView}" /> class.
        /// </summary>
        protected WindowViewMediatorBase([NotNull] IViewModel viewModel,
            [NotNull] IThreadManager threadManager, [NotNull] IViewManager viewManager,
            [NotNull] IOperationCallbackManager operationCallbackManager)
        {
            Should.NotBeNull(viewModel, "viewModel");
            Should.NotBeNull(threadManager, "threadManager");
            Should.NotBeNull(viewManager, "viewManager");
            Should.NotBeNull(operationCallbackManager, "operationCallbackManager");
            _viewModel = viewModel;
            _threadManager = threadManager;
            _viewManager = viewManager;
            _operationCallbackManager = operationCallbackManager;
            var closeableViewModel = viewModel as ICloseableViewModel;
            if (closeableViewModel != null)
                closeableViewModel.Closed += CloseableViewModelOnClosed;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the <see cref="IWindowViewBase" />.
        /// </summary>
        public TView View { get; private set; }

        /// <summary>
        ///     Gets the value that indicates that curren view model is closing.
        /// </summary>
        protected bool IsClosing { get; private set; }

        /// <summary>
        ///     Gets or sets the <see cref="IViewManager" />.
        /// </summary>
        protected IViewManager ViewManager
        {
            get { return _viewManager; }
        }

        /// <summary>
        ///     Gets or sets the <see cref="IThreadManager" />.
        /// </summary>
        protected IThreadManager ThreadManager
        {
            get { return _threadManager; }
        }

        /// <summary>
        ///     Gets or sets the <see cref="IOperationCallbackManager" />.
        /// </summary>
        protected IOperationCallbackManager OperationCallbackManager
        {
            get { return _operationCallbackManager; }
        }

        #endregion

        #region Implementation of IWindowViewMediator

        /// <summary>
        ///     Gets a value that indicates whether the dialog is visible. true if the dialog is visible; otherwise, false.
        /// </summary>
        public bool IsOpen
        {
            get { return _isOpen; }
        }

        /// <summary>
        ///     Gets the <see cref="IWindowViewBase" />.
        /// </summary>
        IWindowViewBase IWindowViewMediator.View
        {
            get { return View; }
        }

        /// <summary>
        ///     Gets the underlying view model.
        /// </summary>
        public virtual IViewModel ViewModel
        {
            get { return _viewModel; }
        }

        /// <summary>
        ///     Shows the specified <see cref="IViewModel" />.
        /// </summary>
        /// <param name="callback">The specified callback, if any.</param>
        /// <param name="context">The specified context.</param>
        public void Show(IOperationCallback callback, IDataContext context)
        {
            ViewModel.NotBeDisposed();
            if (IsOpen)
                throw ExceptionManager.WindowOpened();
            _isOpen = true;
            if (context == null)
                context = DataContext.Empty;
            if (callback != null)
                OperationCallbackManager.Register(OperationType.WindowNavigation, ViewModel, callback, context);
            OnShow(context);
            ShowInternal(context);
        }

        /// <summary>
        ///     Tries to close view-model.
        /// </summary>
        /// <param name="parameter">The specified parameter, if any.</param>
        /// <returns>An instance of task with result.</returns>
        public Task<bool> CloseAsync(object parameter)
        {
            if (!IsOpen)
                throw ExceptionManager.WindowClosed();
            IsClosing = true;
            _closeParameter = parameter;
            return OnClosing(parameter)
                .TryExecuteSynchronously(task =>
                {
                    try
                    {
                        if (task.Result)
                            CloseViewImmediate();
                        return task.Result;
                    }
                    catch (Exception e)
                    {
                        OperationCallbackManager.SetResult(ViewModel,
                            OperationResult.CreateErrorResult<bool?>(OperationType.WindowNavigation, ViewModel, e,
                                CreateCloseContext()));
                        throw;
                    }
                    finally
                    {
                        IsClosing = false;
                    }
                });
        }

        /// <summary>
        ///     Updates the current view, for example android platform use this api to update view after recreate a dialog
        ///     fragment.
        /// </summary>
        void IWindowViewMediator.UpdateView(IWindowViewBase view, bool isOpen, IDataContext context)
        {
            if (ReferenceEquals(View, view))
                return;
            _isOpen = isOpen;
            if (View != null)
                CleanupView(View);
            View = (TView)view;
            if (View != null)
                InitializeView(View, context);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Shows the view in the specified mode.
        /// </summary>
        protected abstract void ShowView([NotNull] TView windowView, bool isDialog, IDataContext context);

        /// <summary>
        ///     Initializes the specified view.
        /// </summary>
        protected abstract void InitializeView([NotNull] TView windowView, IDataContext context);

        /// <summary>
        ///     Clears the event subscribtions from the specified view.
        /// </summary>
        /// <param name="windowView">The specified window-view to dispose.</param>
        protected abstract void CleanupView([NotNull] TView windowView);

        /// <summary>
        ///     Closes the view.
        /// </summary>
        protected abstract void CloseView([NotNull] TView view);

        /// <summary>
        ///     Occurs when view model is showed.
        /// </summary>
        protected virtual void OnShow([NotNull] IDataContext context)
        {
            var ctx = new NavigationContext(NavigationMode.New, ViewModel.GetParentViewModel(), ViewModel, this,
                context.GetData(NavigationConstants.Parameters));

            var navigableViewModel = ctx.ViewModelFrom as INavigableViewModel;
            if (navigableViewModel != null)
                navigableViewModel.OnNavigatedFrom(ctx);

            navigableViewModel = ctx.ViewModelTo as INavigableViewModel;
            if (navigableViewModel != null)
                navigableViewModel.OnNavigatedTo(ctx);
        }

        /// <summary>
        ///     Occurs when view model is closing.
        /// </summary>
        /// <returns>
        ///     If <c>true</c> - close, otherwise <c>false</c>.
        /// </returns>
        protected virtual Task<bool> OnClosing(object parameter)
        {
            return ViewModel.TryCloseAsync(parameter, CreateCloseContext());
        }

        /// <summary>
        ///     Occurs when view model is closed.
        /// </summary>
        protected virtual void OnClosed(object parameter, INavigationContext context)
        {
            var navigableViewModel = ViewModel as INavigableViewModel;
            if (navigableViewModel != null)
                navigableViewModel.OnNavigatedFrom(context);

            navigableViewModel = context.ViewModelTo as INavigableViewModel;
            if (navigableViewModel != null)
                navigableViewModel.OnNavigatedTo(context);
        }

        /// <summary>
        ///     Occurred on closing view.
        /// </summary>
        protected void OnViewClosing(object sender, CancelEventArgs e)
        {
            try
            {
                _cancelArgs = e;
                if (_shouldClose)
                {
                    e.Cancel = false;
                    CompleteCloseAsync();
                    return;
                }
                e.Cancel = true;
                CloseAsync(null);
            }
            finally
            {
                _cancelArgs = null;
            }
        }

        /// <summary>
        ///     Occured when view is closed.
        /// </summary>
        protected void OnViewClosed(object sender, EventArgs e)
        {
            if (!IsClosing)
                CompleteClose();
        }

        private void CloseableViewModelOnClosed(object sender, ViewModelClosedEventArgs args)
        {
            if (IsOpen && !IsClosing)
                CloseViewImmediate();
        }

        private void ShowInternal(IDataContext context)
        {
            _viewManager
                .GetViewAsync(ViewModel, context)
                .TryExecuteSynchronously(task =>
                {
                    View = task.Result as TView;
                    if (View == null)
                        throw ExceptionManager.ViewNotFound(GetType(), typeof(TView));
                    _viewManager.InitializeViewAsync(ViewModel, View).WithBusyIndicator(ViewModel, true);
                    InitializeView(View, context);

                    bool isDialog;
                    if (!context.TryGetData(NavigationConstants.IsDialog, out isDialog))
                        isDialog = true;
                    ShowView(View, isDialog, context);
                }, ViewModel.DisposeCancellationToken)
                .WithBusyIndicator(ViewModel, true);
        }

        private void CloseViewImmediate()
        {
            if (ThreadManager.IsUiThread)
                CloseViewImmediateInternal();
            else
                ThreadManager.InvokeOnUiThreadAsync(CloseViewImmediateInternal);
        }

        private void CloseViewImmediateInternal()
        {
            if (_cancelArgs != null)
            {
                _cancelArgs.Cancel = false;
                CompleteCloseAsync();
            }
            else if (View != null)
            {
                _shouldClose = true;
                CloseView(View);
            }
        }

        private void CompleteCloseAsync()
        {
            //NOTE to minimize the time of closing the window.
            ThreadManager.InvokeAsync(CompleteClose);
        }

        private void CompleteClose()
        {
            INavigationContext context = CreateCloseContext();
            OnClosed(_closeParameter, context);

            bool? result = null;
            var operationResult = ViewModel as IHasOperationResult;
            if (operationResult != null)
                result = operationResult.OperationResult;
            OperationCallbackManager.SetResult(ViewModel,
                OperationResult.CreateResult(OperationType.WindowNavigation, ViewModel, result, context));

            _closeParameter = null;
            _shouldClose = false;
            _isOpen = false;
            TView view = View;
            if (view == null)
                return;
            ThreadManager.InvokeOnUiThreadAsync(() => CleanupView(view));
            _viewManager
                .CleanupViewAsync(ViewModel)
                .WithTaskExceptionHandler(ViewModel);
            View = null;
        }

        private INavigationContext CreateCloseContext()
        {
            return _closeParameter as INavigationContext ??
                   new NavigationContext(NavigationMode.Back, ViewModel, ViewModel.GetParentViewModel(), this);
        }

        #endregion
    }
}