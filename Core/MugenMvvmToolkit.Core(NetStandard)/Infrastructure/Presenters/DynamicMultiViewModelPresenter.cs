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
    public class DynamicMultiViewModelPresenter<TViewModel> : IDynamicViewModelPresenter
        where TViewModel : class, IViewModel
    {
        #region Fields

        private readonly IOperationCallbackManager _callbackManager;
        private readonly Func<TViewModel, IDataContext, IViewModelPresenter, bool> _canShowViewModel;
        private readonly IMultiViewModel<TViewModel> _multiViewModel;

        #endregion

        #region Constructors

        public DynamicMultiViewModelPresenter([NotNull] IMultiViewModel<TViewModel> multiViewModel,
            IOperationCallbackManager callbackManager = null, Func<TViewModel, IDataContext, IViewModelPresenter, bool> canShowViewModel = null)
        {
            Should.NotBeNull(multiViewModel, nameof(multiViewModel));
            _multiViewModel = multiViewModel;
            _callbackManager = callbackManager ?? multiViewModel.GetIocContainer(true).Get<IOperationCallbackManager>();
            _canShowViewModel = canShowViewModel;
            multiViewModel.ViewModelRemoved += MultiViewModelOnViewModelClosed;
        }

        #endregion

        #region Properties

        [NotNull]
        public static Func<IViewModel, IDataContext, IViewModelPresenter, bool> CanShowViewModelDefault
        {
            get
            {
                if (ServiceProvider.CanShowMultiViewModelDelegate == null)
                    ServiceProvider.CanShowMultiViewModelDelegate = (model, context, arg3) => true;
                return ServiceProvider.CanShowMultiViewModelDelegate;
            }
            set { ServiceProvider.CanShowMultiViewModelDelegate = value; }
        }

        protected IMultiViewModel<TViewModel> MultiViewModel => _multiViewModel;

        protected IOperationCallbackManager CallbackManager => _callbackManager;

        protected Func<TViewModel, IDataContext, IViewModelPresenter, bool> CanShowViewModel
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

        public int Priority => ViewModelPresenter.DefaultMultiViewModelPresenterPriority;

        public virtual INavigationOperation TryShowAsync(IViewModel viewModel, IDataContext context,
            IViewModelPresenter parentPresenter)
        {
            var vm = viewModel as TViewModel;
            if (vm == null || ReferenceEquals(viewModel, _multiViewModel))
                return null;
            bool data;
            if (context.TryGetData(NavigationConstants.SuppressTabNavigation, out data) && data)
                return null;
            if (!CanShowViewModel(vm, context, parentPresenter))
                return null;
            if (MultiViewModel.ItemsSource.Contains(vm))
                MultiViewModel.SelectedItem = vm;
            else
                MultiViewModel.AddViewModel(vm, true);
            var operation = new NavigationOperation();
            CallbackManager.Register(OperationType.TabNavigation, viewModel, operation.ToOperationCallback(), context);
            return operation;
        }

        #endregion

        #region Methods

        protected virtual void MultiViewModelOnViewModelClosed(IMultiViewModel<TViewModel> sender, ValueEventArgs<TViewModel> args)
        {
            var context = new NavigationContext(NavigationType.Tab, NavigationMode.Back, args.Value, MultiViewModel.SelectedItem, MultiViewModel);
            var result = ViewModelExtensions.GetOperationResult(args.Value);
            CallbackManager.SetResult(OperationResult.CreateResult(OperationType.TabNavigation, args.Value, result, context));
        }

        #endregion
    }
}
