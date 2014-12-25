#region Copyright
// ****************************************************************************
// <copyright file="DynamicMultiViewModelPresenter.cs">
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
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Infrastructure.Presenters
{
    /// <summary>
    ///     Represents the service that allows to show a view model using multi view model.
    /// </summary>
    public class DynamicMultiViewModelPresenter : IDynamicViewModelPresenter
    {
        #region Fields

        private static Func<IViewModel, IDataContext, IViewModelPresenter, bool> _canShowDelegateDefault;

        private readonly IOperationCallbackManager _callbackManager;
        private readonly Func<IViewModel, IDataContext, IViewModelPresenter, bool> _canShowViewModel;
        private readonly IMultiViewModel _multiViewModel;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="DynamicMultiViewModelPresenter" /> class.
        /// </summary>
        public DynamicMultiViewModelPresenter([NotNull] IMultiViewModel multiViewModel,
            IOperationCallbackManager callbackManager = null, Func<IViewModel, IDataContext, IViewModelPresenter, bool> canShowViewModel = null)
        {
            Should.NotBeNull(multiViewModel, "multiViewModel");
            _multiViewModel = multiViewModel;
            _callbackManager = callbackManager ?? multiViewModel.GetIocContainer(true).Get<IOperationCallbackManager>();
            _canShowViewModel = canShowViewModel;
            multiViewModel.ViewModelRemoved += MultiViewModelOnViewModelClosed;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the delegate that determines that presenter can handle request.
        /// </summary>
        [NotNull]
        public static Func<IViewModel, IDataContext, IViewModelPresenter, bool> CanShowViewModelDefault
        {
            get
            {
                if (_canShowDelegateDefault == null)
                    _canShowDelegateDefault = (model, context, arg3) => true;
                return _canShowDelegateDefault;
            }
            set { _canShowDelegateDefault = value; }
        }

        /// <summary>
        ///     Get the current <see cref="IMultiViewModel" />.
        /// </summary>
        protected IMultiViewModel MultiViewModel
        {
            get { return _multiViewModel; }
        }

        /// <summary>
        ///     Gets the <see cref="IOperationCallbackManager" />.
        /// </summary>
        protected IOperationCallbackManager CallbackManager
        {
            get { return _callbackManager; }
        }

        /// <summary>
        /// Gets the delegate that determines that presenter can handle request.
        /// </summary>
        protected Func<IViewModel, IDataContext, IViewModelPresenter, bool> CanShowViewModel
        {
            get
            {
                if (_canShowViewModel == null)
                    return CanShowViewModelDefault;
                return _canShowViewModel;
            }
        }

        #endregion

        #region Implementation of IDynamicViewModelPresenter

        /// <summary>
        ///     Gets the presenter priority.
        /// </summary>
        public int Priority
        {
            get { return ViewModelPresenter.DefaultMultiViewModelPresenterPriority; }
        }

        /// <summary>
        ///     Tries to show the specified <see cref="IViewModel" />.
        /// </summary>
        /// <param name="viewModel">The specified <see cref="IViewModel" /> to show.</param>
        /// <param name="context">The specified context.</param>
        /// <param name="parentPresenter">The parent presenter, if any.</param>
        public virtual IAsyncOperation<bool?> TryShowAsync(IViewModel viewModel, IDataContext context,
            IViewModelPresenter parentPresenter)
        {
            Should.NotBeNull(viewModel, "viewModel");
            bool data;
            if (context.TryGetData(NavigationConstants.SuppressTabNavigation, out data) && data)
                return null;
            if (!CanShowViewModel(viewModel, context, parentPresenter))
                return null;
            MultiViewModel.AddViewModel(viewModel);
            var operation = new AsyncOperation<bool?>();
            CallbackManager.Register(OperationType.TabNavigation, viewModel, operation.ToOperationCallback(), context);
            return operation;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Occurs when a view model is closed.
        /// </summary>
        protected virtual void MultiViewModelOnViewModelClosed(object sender, ValueEventArgs<IViewModel> args)
        {
            var context = new NavigationContext(NavigationType.Tab, NavigationMode.Back, args.Value,
                MultiViewModel.SelectedItem, MultiViewModel);
            bool? result = null;
            var hasOperationResult = args.Value as IHasOperationResult;
            if (hasOperationResult != null)
                result = hasOperationResult.OperationResult;
            CallbackManager.SetResult(args.Value, OperationResult.CreateResult(OperationType.TabNavigation, args.Value, result, context));
        }

        #endregion
    }
}