#region Copyright

// ****************************************************************************
// <copyright file="DynamicViewModelNavigationPresenter.cs">
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
using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure.Callbacks;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Infrastructure.Presenters
{
    public sealed class DynamicViewModelNavigationPresenter : IRestorableDynamicViewModelPresenter
    {
        #region Fields

        private readonly Func<IViewModel, IDataContext, IViewModelPresenter, bool> _canShowViewModel;
        private readonly IOperationCallbackManager _operationCallbackManager;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public DynamicViewModelNavigationPresenter(IOperationCallbackManager operationCallbackManager)
        {
            Should.NotBeNull(operationCallbackManager, nameof(operationCallbackManager));
            _operationCallbackManager = operationCallbackManager;
        }

        public DynamicViewModelNavigationPresenter(IOperationCallbackManager operationCallbackManager, Func<IViewModel, IDataContext, IViewModelPresenter, bool> canShowViewModel)
            : this(operationCallbackManager)
        {
            _canShowViewModel = canShowViewModel;
        }

        #endregion

        #region Methods

        private bool CanShowViewModel(IViewModel viewModel, IDataContext context,
            IViewModelPresenter parentPresenter)
        {
            bool data;
            if (context.TryGetData(NavigationConstants.SuppressPageNavigation, out data) && data)
                return false;

            if (_canShowViewModel == null)
                return ApplicationSettings.NavigationPresenterCanShowViewModel == null || ApplicationSettings.NavigationPresenterCanShowViewModel(viewModel, context, parentPresenter);
            return _canShowViewModel(viewModel, context, parentPresenter);
        }

        #endregion

        #region Implementation of IDynamicViewModelPresenter

        public int Priority => ViewModelPresenter.DefaultNavigationPresenterPriority;

        public INavigationOperation TryShowAsync(IViewModel viewModel, IDataContext context,
            IViewModelPresenter parentPresenter)
        {
            if (!CanShowViewModel(viewModel, context, parentPresenter))
                return null;
            context = context.ToNonReadOnly();
            context.AddOrUpdate(NavigationConstants.ViewModel, viewModel);

            var operation = new NavigationOperation();
            var provider = viewModel.GetIocContainer(true).Get<INavigationProvider>();
            _operationCallbackManager.Register(OperationType.PageNavigation, viewModel, operation.ToOperationCallback(), context);
            operation.SetNavigationCompletedTask(provider.NavigateAsync(context));
            return operation;
        }

        public Task<bool> TryCloseAsync(IViewModel viewModel, IDataContext context, IViewModelPresenter parentPresenter)
        {
            INavigationProvider provider;
            if (viewModel.GetIocContainer(true).TryGet(out provider))
            {
                context = context.ToNonReadOnly();
                context.AddOrUpdate(NavigationConstants.ViewModel, viewModel);
                return provider.TryCloseAsync(context);
            }
            return null;
        }

        public bool Restore(IViewModel viewModel, IDataContext context, IViewModelPresenter parentPresenter)
        {
            if (!CanShowViewModel(viewModel, context, parentPresenter))
                return false;
            INavigationProvider provider;
            if (viewModel.GetIocContainer(true).TryGet(out provider))
            {
                context = context.ToNonReadOnly();
                context.AddOrUpdate(NavigationConstants.ViewModel, viewModel);
                provider.Restore(context);
                return true;
            }
            return false;
        }

        #endregion
    }
}
