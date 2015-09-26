#region Copyright

// ****************************************************************************
// <copyright file="DynamicViewModelWindowPresenter.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
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
#elif SILVERLIGHT
using MugenMvvmToolkit.Silverlight.Infrastructure.Mediators;
using MugenMvvmToolkit.Silverlight.Interfaces.Views;

namespace MugenMvvmToolkit.Silverlight.Infrastructure.Presenters
#elif WINDOWSCOMMON
using MugenMvvmToolkit.WinRT.Infrastructure.Mediators;
using MugenMvvmToolkit.WinRT.Interfaces.Views;

namespace MugenMvvmToolkit.WinRT.Infrastructure.Presenters
#endif
{
    public class DynamicViewModelWindowPresenter : IRestorableDynamicViewModelPresenter
    {
        #region Fields

        public static readonly DataConstant<IWindowViewMediator> WindowViewMediatorConstant;

        public static readonly DataConstant<object> RestoredViewConstant;

        public static readonly DataConstant<bool> IsOpenViewConstant;

        private readonly IThreadManager _threadManager;
        private readonly IOperationCallbackManager _callbackManager;
        private readonly IWrapperManager _wrapperManager;
        private readonly IViewMappingProvider _viewMappingProvider;
        private readonly IViewManager _viewManager;
        private Task _currentTask;

        #endregion

        #region Constructors

        static DynamicViewModelWindowPresenter()
        {
            WindowViewMediatorConstant = DataConstant.Create(() => WindowViewMediatorConstant, true);
            RestoredViewConstant = DataConstant.Create(() => RestoredViewConstant, true);
            IsOpenViewConstant = DataConstant.Create(() => IsOpenViewConstant);
        }

        public DynamicViewModelWindowPresenter([NotNull] IViewMappingProvider viewMappingProvider,
            [NotNull] IViewManager viewManager,
            [NotNull] IWrapperManager wrapperManager, [NotNull] IThreadManager threadManager,
            [NotNull] IOperationCallbackManager callbackManager)
        {
            Should.NotBeNull(viewMappingProvider, "viewMappingProvider");
            Should.NotBeNull(viewManager, "viewManager");
            Should.NotBeNull(wrapperManager, "wrapperManager");
            Should.NotBeNull(threadManager, "threadManager");
            Should.NotBeNull(callbackManager, "callbackManager");
            _viewMappingProvider = viewMappingProvider;
            _viewManager = viewManager;
            _wrapperManager = wrapperManager;
            _threadManager = threadManager;
            _callbackManager = callbackManager;
        }

        #endregion

        #region Properties

        protected IViewMappingProvider ViewMappingProvider
        {
            get { return _viewMappingProvider; }
        }

        protected IWrapperManager WrapperManager
        {
            get { return _wrapperManager; }
        }

        protected IThreadManager ThreadManager
        {
            get { return _threadManager; }
        }

        protected IOperationCallbackManager CallbackManager
        {
            get { return _callbackManager; }
        }

        protected IViewManager ViewManager
        {
            get { return _viewManager; }
        }

        #endregion

        #region Implementation of IDynamicViewModelPresenter

        public virtual int Priority
        {
            get { return ViewModelPresenter.DefaultWindowPresenterPriority; }
        }

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
            var view = context.GetData(RestoredViewConstant);
            if (view == null)
                return false;
            var mediator = TryCreateWindowViewMediator(viewModel, context);
            if (mediator == null)
                return false;
            mediator.UpdateView(view, context.GetData(IsOpenViewConstant), context);
            return true;
        }

        #endregion

        #region Methods

        [CanBeNull]
        protected virtual IWindowViewMediator CreateWindowViewMediator([NotNull] IViewModel viewModel, Type viewType,
            [NotNull] IDataContext context)
        {
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
            if (!viewModel.Settings.Metadata.TryGetData(WindowViewMediatorConstant, out viewMediator))
            {
                viewMediator = CreateWindowViewMediator(viewModel, mappingItem.ViewType, context);
                if (viewMediator != null)
                    viewModel.Settings.Metadata.Add(WindowViewMediatorConstant, viewMediator);
            }
            return viewMediator;
        }

        #endregion
    }
}
