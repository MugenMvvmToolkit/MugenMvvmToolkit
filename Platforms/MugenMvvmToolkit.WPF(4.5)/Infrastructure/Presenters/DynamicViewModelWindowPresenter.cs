#region Copyright

// ****************************************************************************
// <copyright file="DynamicViewModelWindowPresenter.cs">
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
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure.Callbacks;
using MugenMvvmToolkit.Infrastructure.Presenters;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Mediators;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;

#if APPCOMPAT
using MugenMvvmToolkit.Android.AppCompat.Infrastructure.Mediators;
using MugenMvvmToolkit.Android.AppCompat.Interfaces.Views;

namespace MugenMvvmToolkit.Android.AppCompat.Infrastructure.Presenters
#elif ANDROIDCORE
using MugenMvvmToolkit.Android.Infrastructure.Mediators;
using MugenMvvmToolkit.Android.Interfaces.Views;

namespace MugenMvvmToolkit.Android.Infrastructure.Presenters
#elif XAMARIN_FORMS
using IWindowView = MugenMvvmToolkit.Xamarin.Forms.Interfaces.Views.IModalView;
using WindowViewMediator = MugenMvvmToolkit.Xamarin.Forms.Infrastructure.Mediators.ModalViewMediator;

namespace MugenMvvmToolkit.Xamarin.Forms.Infrastructure.Presenters
#elif TOUCH
using MugenMvvmToolkit.iOS.Interfaces.Views;
using MugenMvvmToolkit.iOS.Infrastructure.Mediators;

namespace MugenMvvmToolkit.iOS.Infrastructure.Presenters
#elif WINFORMS
using MugenMvvmToolkit.WinForms.Interfaces.Views;
using MugenMvvmToolkit.WinForms.Infrastructure.Mediators;

namespace MugenMvvmToolkit.WinForms.Infrastructure.Presenters
#elif WPF
using MugenMvvmToolkit.WPF.Infrastructure.Mediators;
using MugenMvvmToolkit.WPF.Interfaces.Views;

namespace MugenMvvmToolkit.WPF.Infrastructure.Presenters
#elif WINDOWS_UWP
using MugenMvvmToolkit.UWP.Infrastructure.Mediators;
using MugenMvvmToolkit.UWP.Interfaces.Views;

namespace MugenMvvmToolkit.UWP.Infrastructure.Presenters
#endif
{
    public class DynamicViewModelWindowPresenter : IRestorableDynamicViewModelPresenter
    {
        #region Fields

        private readonly IThreadManager _threadManager;
        private readonly IOperationCallbackManager _callbackManager;
        private readonly IWrapperManager _wrapperManager;
        private readonly IViewMappingProvider _viewMappingProvider;
        private readonly IViewManager _viewManager;
        private Task _currentTask;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public DynamicViewModelWindowPresenter([NotNull] IViewMappingProvider viewMappingProvider,
            [NotNull] IViewManager viewManager,
            [NotNull] IWrapperManager wrapperManager, [NotNull] IThreadManager threadManager,
            [NotNull] IOperationCallbackManager callbackManager)
        {
            Should.NotBeNull(viewMappingProvider, nameof(viewMappingProvider));
            Should.NotBeNull(viewManager, nameof(viewManager));
            Should.NotBeNull(wrapperManager, nameof(wrapperManager));
            Should.NotBeNull(threadManager, nameof(threadManager));
            Should.NotBeNull(callbackManager, nameof(callbackManager));
            _viewMappingProvider = viewMappingProvider;
            _viewManager = viewManager;
            _wrapperManager = wrapperManager;
            _threadManager = threadManager;
            _callbackManager = callbackManager;
        }

        #endregion

        #region Properties

        protected IViewMappingProvider ViewMappingProvider => _viewMappingProvider;

        protected IWrapperManager WrapperManager => _wrapperManager;

        protected IThreadManager ThreadManager => _threadManager;

        protected IOperationCallbackManager CallbackManager => _callbackManager;

        protected IViewManager ViewManager => _viewManager;

        #endregion

        #region Implementation of IDynamicViewModelPresenter

        public virtual int Priority => ViewModelPresenter.DefaultWindowPresenterPriority;

        public INavigationOperation TryShowAsync(IViewModel viewModel, IDataContext context,
            IViewModelPresenter parentPresenter)
        {
            var viewMediator = TryCreateWindowViewMediator(viewModel, context);
            if (viewMediator == null)
                return null;
            var tcs = new TaskCompletionSource<object>();
            var operation = new NavigationOperation(tcs.Task);

            if (_currentTask == null)
                Show(viewMediator, operation, context, tcs);
            else
                _currentTask.TryExecuteSynchronously(_ => Show(viewMediator, operation, context, tcs));
            return operation;
        }

        public bool Restore(IViewModel viewModel, IDataContext context, IViewModelPresenter parentPresenter)
        {
            var view = context.GetData(WindowPresenterConstants.RestoredView);
            if (view == null)
                return false;
            var mediator = TryCreateWindowViewMediator(viewModel, context);
            if (mediator == null)
                return false;
            mediator.UpdateView(view, context.GetData(WindowPresenterConstants.IsViewOpened), context);
            return true;
        }

        #endregion

        #region Methods

        [CanBeNull]
        protected virtual IWindowViewMediator CreateWindowViewMediator([NotNull] IViewModel viewModel, Type viewType, [NotNull] IDataContext context)
        {
            var windowViewMediator = ServiceProvider.WindowViewMediatorFactory?.Invoke(viewModel, viewType, context);
            if (windowViewMediator != null)
                return windowViewMediator;
#if TOUCH
            var container = viewModel.GetIocContainer(true);
            if (_wrapperManager.CanWrap(viewType, typeof(IModalView), context))
                return new ModalViewMediator(viewModel, ThreadManager, ViewManager, WrapperManager, CallbackManager, ViewMappingProvider, container.Get<IViewModelProvider>());
#else
            if (_wrapperManager.CanWrap(viewType, typeof(IWindowView), context))
                return new WindowViewMediator(viewModel, ThreadManager, ViewManager, WrapperManager, CallbackManager);
#endif
            return null;
        }

        private void Show(IWindowViewMediator viewMediator, INavigationOperation operation, IDataContext context, TaskCompletionSource<object> tcs)
        {
            try
            {
                var task = viewMediator.ShowAsync(operation.ToOperationCallback(), context);
                _currentTask = task;
                tcs.TrySetFromTask(task);
            }
            catch (Exception e)
            {
                tcs.TrySetException(e);
                throw;
            }
        }

        private IWindowViewMediator TryCreateWindowViewMediator(IViewModel viewModel, IDataContext context)
        {
            bool data;
            if (context.TryGetData(NavigationConstants.SuppressWindowNavigation, out data) && data)
                return null;

            var viewName = viewModel.GetViewName(context);
            IViewMappingItem mappingItem = ViewMappingProvider.FindMappingForViewModel(viewModel.GetType(), viewName, false);
            if (mappingItem == null)
                return null;

            IWindowViewMediator viewMediator;
            if (!viewModel.Settings.Metadata.TryGetData(WindowPresenterConstants.WindowViewMediator, out viewMediator))
            {
                viewMediator = CreateWindowViewMediator(viewModel, mappingItem.ViewType, context);
                if (viewMediator != null)
                    viewModel.Settings.Metadata.Add(WindowPresenterConstants.WindowViewMediator, viewMediator);
            }
            return viewMediator;
        }

        #endregion
    }
}
