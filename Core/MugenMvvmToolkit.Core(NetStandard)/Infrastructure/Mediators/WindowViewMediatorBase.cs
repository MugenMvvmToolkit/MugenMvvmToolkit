#region Copyright

// ****************************************************************************
// <copyright file="WindowViewMediatorBase.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Mediators;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Infrastructure.Mediators
{
    public abstract class WindowViewMediatorBase<TView> : IWindowViewMediator
        where TView : class
    {
        #region Fields

        private readonly IThreadManager _threadManager;
        private readonly IViewManager _viewManager;
        private readonly IWrapperManager _wrapperManager;
        private readonly INavigationDispatcher _navigationDispatcher;
        private readonly IViewModel _viewModel;
        private CancelEventArgs _cancelArgs;
        private IDataContext _closeParameter;
        private bool _isOpen;
        private bool _shouldClose;

        #endregion

        #region Constructors

        protected WindowViewMediatorBase([NotNull] IViewModel viewModel,
            [NotNull] IThreadManager threadManager, [NotNull] IViewManager viewManager,
            [NotNull] IWrapperManager wrapperManager, [NotNull] INavigationDispatcher navigationDispatcher)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(threadManager, nameof(threadManager));
            Should.NotBeNull(viewManager, nameof(viewManager));
            Should.NotBeNull(wrapperManager, nameof(wrapperManager));
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            _viewModel = viewModel;
            _threadManager = threadManager;
            _viewManager = viewManager;
            _wrapperManager = wrapperManager;
            _navigationDispatcher = navigationDispatcher;
        }

        #endregion

        #region Properties

        public TView View { get; private set; }

        protected bool IsClosing { get; private set; }

        protected IViewManager ViewManager => _viewManager;

        protected IThreadManager ThreadManager => _threadManager;

        protected IWrapperManager WrapperManager => _wrapperManager;

        protected INavigationDispatcher NavigationDispatcher => _navigationDispatcher;

        #endregion

        #region Implementation of IWindowViewMediator

        public bool IsOpen => _isOpen;

        object IWindowViewMediator.View => View;

        public virtual IViewModel ViewModel => _viewModel;

        public Task ShowAsync(IDataContext context)
        {
            ViewModel.NotBeDisposed();
            if (IsOpen)
            {
                ActivateView(View, context);
                return Empty.Task;
            }
            var tcs = new TaskCompletionSource<object>();
            RaiseNavigating(context, NavigationMode.New)
                .TryExecuteSynchronously(task =>
                {
                    try
                    {
                        if (!task.Result)
                        {
                            tcs.TrySetCanceled();
                            NavigationDispatcher.OnNavigationCanceled(CreateOpenContext(context, NavigationMode.New));
                            return;
                        }
                        if (context == null)
                            context = DataContext.Empty;
                        _isOpen = true;
                        ShowInternal(context, tcs);
                    }
                    catch (Exception e)
                    {
                        tcs.TrySetException(e);
                        NavigationDispatcher.OnNavigationFailed(CreateOpenContext(context, NavigationMode.New), e);
                    }
                });
            return tcs.Task;
        }

        public Task<bool> CloseAsync(IDataContext context)
        {
            if (!IsOpen)
            {
                Tracer.Error(ExceptionManager.WindowClosedString);
                return Empty.TrueTask;
            }
            IsClosing = true;
            _closeParameter = context;
            return OnClosing(context)
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
                        NavigationDispatcher.OnNavigationFailed(CreateCloseContext(context), e);
                        throw;
                    }
                    finally
                    {
                        IsClosing = false;
                    }
                });
        }

        public void UpdateView(object view, bool isOpen, IDataContext context)
        {
            if (ReferenceEquals(View, view))
                return;
            var oldView = View;
            if (view != null)
            {
                view = WrapperManager.Wrap(view, typeof(TView), context);
                if (ReferenceEquals(oldView, view))
                    return;
            }
            _isOpen = isOpen;
            if (oldView != null)
                CleanupView(oldView);
            View = (TView)view;
            if (View != null)
                InitializeView(View, context);
            OnViewUpdated(View, context);
            if (oldView == null && view != null && isOpen)
                ThreadManager.Invoke(ExecutionMode.AsynchronousOnUiThread, this, context,
                        (@base, dataContext) => @base.NavigationDispatcher.OnNavigated(@base.CreateOpenContext(dataContext, NavigationMode.Refresh)), OperationPriority.Low);
        }

        #endregion

        #region Methods

        protected abstract void ShowView([NotNull] TView view, bool isDialog, IDataContext context);

        protected abstract void ActivateView([NotNull] TView view, IDataContext context);

        protected abstract void InitializeView([NotNull] TView view, IDataContext context);

        protected abstract void CleanupView([NotNull] TView view);

        protected abstract void CloseView([NotNull] TView view);

        protected virtual void OnShown([NotNull] IDataContext context)
        {
            NavigationDispatcher.OnNavigated(CreateOpenContext(context, NavigationMode.New));
        }

        protected virtual Task<bool> OnClosing(IDataContext context)
        {
            return NavigationDispatcher.NavigatingFromAsync(CreateCloseContext(context));
        }

        protected virtual void OnClosed(INavigationContext context)
        {
            NavigationDispatcher.OnNavigated(context);
        }

        protected virtual void OnViewUpdated(TView view, IDataContext context)
        {
        }

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

        protected void OnViewClosed(object sender, EventArgs e)
        {
            if (!IsClosing)
                CompleteClose();
        }

        private void ShowInternal(IDataContext context, TaskCompletionSource<object> tcs)
        {
            ViewManager
                .GetViewAsync(ViewModel, context)
                .TryExecuteSynchronously(task =>
                {
                    View = (TView)WrapperManager.Wrap(task.Result, typeof(TView), context);
                    ViewManager.InitializeViewAsync(ViewModel, task.Result, context);
                    InitializeView(View, context);

                    bool isDialog;
                    if (!context.TryGetData(NavigationConstants.IsDialog, out isDialog))
                        isDialog = true;
                    //NOTE to call method OnShown after ShowView.
                    ThreadManager.Invoke(ExecutionMode.AsynchronousOnUiThread, this, context, tcs,
                        (@base, dataContext, cs) =>
                        {
                            @base.OnShown(dataContext);
                            cs.TrySetResult(null);
                        }, OperationPriority.Low);
                    ThreadManager.Invoke(ExecutionMode.AsynchronousOnUiThread, this, isDialog, context, (@base, b, arg3) =>
                    {
                        @base.ShowView(@base.View, b, arg3);
                    }, OperationPriority.High);
                }, ViewModel.DisposeCancellationToken);
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
            INavigationContext context = CreateCloseContext(_closeParameter);
            OnClosed(context);

            _closeParameter = null;
            _shouldClose = false;
            _isOpen = false;
            TView view = View;
            if (view == null)
                return;
            ThreadManager.Invoke(ExecutionMode.AsynchronousOnUiThread, this, view, context, (@base, v, ctx) =>
            {
                @base.CleanupView(v);
                @base.ViewManager
                     .CleanupViewAsync(@base.ViewModel, ctx)
                     .WithTaskExceptionHandler(@base.ViewModel);
            });
            View = null;
        }

        private INavigationContext CreateCloseContext(IDataContext context)
        {
            return new NavigationContext(NavigationType.Window, NavigationMode.Back, ViewModel, ViewModel.GetParentViewModel(), this, context);
        }

        private NavigationContext CreateOpenContext(IDataContext context, NavigationMode mode)
        {
            return new NavigationContext(NavigationType.Window, mode, ViewModel.GetParentViewModel(), ViewModel, this, context);
        }

        private Task<bool> RaiseNavigating(IDataContext context, NavigationMode mode)
        {
            var parentViewModel = ViewModel.GetParentViewModel();
            if (parentViewModel == null)
                return Empty.TrueTask;
            var ctx = new NavigationContext(NavigationType.Window, mode, ViewModel.GetParentViewModel(), ViewModel, this, context);
            return NavigationDispatcher.NavigatingFromAsync(ctx);
        }

        #endregion
    }
}
