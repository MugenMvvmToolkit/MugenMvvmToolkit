#region Copyright

// ****************************************************************************
// <copyright file="DynamicMultiViewModelPresenter.cs">
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
using System.Linq;
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
    public class DynamicMultiViewModelPresenter : IDynamicViewModelPresenter
    {
        #region Fields

        private readonly IOperationCallbackManager _callbackManager;
        private readonly Func<IViewModel, IDataContext, IViewModelPresenter, bool> _canShowViewModel;
        private readonly IMultiViewModel _multiViewModel;

        #endregion

        #region Constructors

        public DynamicMultiViewModelPresenter([NotNull] IMultiViewModel multiViewModel,
            IOperationCallbackManager callbackManager = null, Func<IViewModel, IDataContext, IViewModelPresenter, bool> canShowViewModel = null)
        {
            Should.NotBeNull(multiViewModel, nameof(multiViewModel));
            _multiViewModel = multiViewModel;
            _callbackManager = callbackManager ?? multiViewModel.GetIocContainer(true).Get<IOperationCallbackManager>();
            _canShowViewModel = canShowViewModel;
            multiViewModel.ViewModelRemoved += MultiViewModelOnViewModelClosed;
        }

        #endregion

        #region Properties

        protected IMultiViewModel MultiViewModel => _multiViewModel;

        protected IOperationCallbackManager CallbackManager => _callbackManager;

        protected Func<IViewModel, IDataContext, IViewModelPresenter, bool> CanShowViewModel => _canShowViewModel;

        #endregion

        #region Implementation of IDynamicViewModelPresenter

        public int Priority => ViewModelPresenter.DefaultMultiViewModelPresenterPriority;

        public virtual INavigationOperation TryShowAsync(IViewModel viewModel, IDataContext context,
            IViewModelPresenter parentPresenter)
        {
            if (!MultiViewModel.ViewModelType.IsInstanceOfType(viewModel))
                return null;
            bool data;
            if (context.TryGetData(NavigationConstants.SuppressTabNavigation, out data) && data)
                return null;
            Func<IViewModel, IDataContext, IViewModelPresenter, bool> canShow = CanShowViewModel ?? ApplicationSettings.MultiViewModelPresenterCanShowViewModel;
            if (canShow != null && !canShow(viewModel, context, parentPresenter))
                return null;

            if (MultiViewModel.ItemsSource.Any(vm => vm == viewModel))
                MultiViewModel.SelectedItem = viewModel;
            else
                MultiViewModel.AddViewModel(viewModel, true);
            var operation = new NavigationOperation();
            CallbackManager.Register(OperationType.TabNavigation, viewModel, operation.ToOperationCallback(), context);
            return operation;
        }

        #endregion

        #region Methods

        protected virtual void MultiViewModelOnViewModelClosed(IMultiViewModel sender, ValueEventArgs<IViewModel> args)
        {
            var context = new NavigationContext(NavigationType.Tab, NavigationMode.Back, args.Value, MultiViewModel.SelectedItem, MultiViewModel);
            var result = ViewModelExtensions.GetOperationResult(args.Value);
            CallbackManager.SetResult(OperationResult.CreateResult(OperationType.TabNavigation, args.Value, result, context));
        }

        #endregion
    }
}
