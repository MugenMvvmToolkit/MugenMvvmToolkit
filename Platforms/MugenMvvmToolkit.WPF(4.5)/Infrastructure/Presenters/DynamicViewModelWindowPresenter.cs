#region Copyright
// ****************************************************************************
// <copyright file="DynamicViewModelWindowPresenter.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure.Callbacks;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Mediators;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;
#if APPCOMPAT
using MugenMvvmToolkit.Infrastructure.Presenters;
using MugenMvvmToolkit.AppCompat.Infrastructure.Mediators;
using MugenMvvmToolkit.AppCompat.Interfaces.Views;
using Fragment = Android.Support.V4.App.Fragment;
using FragmentTransaction = Android.Support.V4.App.FragmentTransaction;

namespace MugenMvvmToolkit.AppCompat.Infrastructure.Presenters
#elif FRAGMENTSUPPORT
using MugenMvvmToolkit.Infrastructure.Presenters;
using MugenMvvmToolkit.FragmentSupport.Infrastructure.Mediators;
using MugenMvvmToolkit.FragmentSupport.Interfaces.Views;

namespace MugenMvvmToolkit.FragmentSupport.Infrastructure.Presenters
#else
using MugenMvvmToolkit.Infrastructure.Mediators;
using MugenMvvmToolkit.Interfaces.Views;

namespace MugenMvvmToolkit.Infrastructure.Presenters
#endif
{
    /// <summary>
    ///     Represents the service that allows to show a view model using <see cref="IWindowViewMediator" />.
    /// </summary>
    public class DynamicViewModelWindowPresenter : IRestorableDynamicViewModelPresenter
    {
        #region Fields

        /// <summary>
        ///     Gets the window view mediator data constant.
        /// </summary>
        public static readonly DataConstant<IWindowViewMediator> WindowViewMediatorConstant;

        /// <summary>
        ///     Gets the view data constant that allows to restore mediator state.
        /// </summary>
        public static readonly DataConstant<object> RestoredViewConstant;

        /// <summary>
        ///     Gets the view data constant that allows to restore mediator state.
        /// </summary>
        public static readonly DataConstant<bool> IsOpenViewConstant;

        private readonly IThreadManager _threadManager;
        private readonly IOperationCallbackManager _callbackManager;
        private readonly IWrapperManager _wrapperManager;
        private readonly IViewMappingProvider _viewMappingProvider;
        private readonly IViewManager _viewManager;

        #endregion

        #region Constructors

        static DynamicViewModelWindowPresenter()
        {
            WindowViewMediatorConstant = DataConstant.Create(() => WindowViewMediatorConstant, true);
            RestoredViewConstant = DataConstant.Create(() => RestoredViewConstant, true);
            IsOpenViewConstant = DataConstant.Create(() => IsOpenViewConstant);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DynamicViewModelWindowPresenter" /> class.
        /// </summary>
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

        /// <summary>
        ///     Gets the <see cref="IViewMappingProvider" />.
        /// </summary>
        protected IViewMappingProvider ViewMappingProvider
        {
            get { return _viewMappingProvider; }
        }

        /// <summary>
        ///     Gets the <see cref="IWrapperManager" />.
        /// </summary>
        protected IWrapperManager WrapperManager
        {
            get { return _wrapperManager; }
        }

        /// <summary>
        ///     Gets the <see cref="IThreadManager" />.
        /// </summary>
        protected IThreadManager ThreadManager
        {
            get { return _threadManager; }
        }

        /// <summary>
        ///     Gets the <see cref="IOperationCallbackManager" />.
        /// </summary>
        protected IOperationCallbackManager CallbackManager
        {
            get { return _callbackManager; }
        }

        /// <summary>
        ///     Gets the <see cref="IViewManager" />.
        /// </summary>
        protected IViewManager ViewManager
        {
            get { return _viewManager; }
        }

        #endregion

        #region Implementation of IDynamicViewModelPresenter

        /// <summary>
        ///     Gets the presenter priority.
        /// </summary>
        public virtual int Priority
        {
            get { return ViewModelPresenter.DefaultWindowPresenterPriority; }
        }

        /// <summary>
        ///     Tries to show the specified <see cref="IViewModel" />.
        /// </summary>
        /// <param name="viewModel">The specified <see cref="IViewModel" /> to show.</param>
        /// <param name="context">The specified context.</param>
        /// <param name="parentPresenter">The parent presenter, if any.</param>
        public IAsyncOperation<bool?> TryShowAsync(IViewModel viewModel, IDataContext context,
            IViewModelPresenter parentPresenter)
        {
            var viewMediator = TryCreateWindowViewMediator(viewModel, context);
            if (viewMediator == null)
                return null;
            var operation = new AsyncOperation<bool?>();
            viewMediator.Show(operation.ToOperationCallback(), context);
            return operation;
        }

        /// <summary>
        /// Tries to restore the presenter state of the specified <see cref="IViewModel" />.
        /// </summary>
        /// <param name="viewModel">The specified <see cref="IViewModel" /> to show.</param>
        /// <param name="context">The specified context.</param>
        /// <param name="parentPresenter">The parent presenter, if any.</param>
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

        /// <summary>
        ///     Creates an instnace of <see cref="IWindowViewMediator" /> for the specified view model.
        /// </summary>
        [CanBeNull]
        protected virtual IWindowViewMediator CreateWindowViewMediator([NotNull] IViewModel viewModel, Type viewType,
            [NotNull] IDataContext context)
        {
#if TOUCH || XAMARIN_FORMS
            var container = viewModel.GetIocContainer(true);
            if (_wrapperManager.CanWrap(viewType, typeof(IModalView), context))
                return new ModalViewMediator(viewModel, ThreadManager, ViewManager, WrapperManager, CallbackManager, ViewMappingProvider, container.Get<IViewModelProvider>());
#else
            if (_wrapperManager.CanWrap(viewType, typeof(IWindowView), context))
                return new WindowViewMediator(viewModel, ThreadManager, ViewManager, WrapperManager, CallbackManager);
#endif
            return null;
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