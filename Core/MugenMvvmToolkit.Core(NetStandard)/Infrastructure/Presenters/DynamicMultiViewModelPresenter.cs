#region Copyright

// ****************************************************************************
// <copyright file="DynamicMultiViewModelPresenter.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Infrastructure.Presenters
{
    public class DynamicMultiViewModelPresenter : IDynamicViewModelPresenter
    {
        #region Constructors

        public DynamicMultiViewModelPresenter([NotNull] IMultiViewModel multiViewModel, Func<IViewModel, IDataContext, IViewModelPresenter, bool> canShowViewModel = null)
        {
            Should.NotBeNull(multiViewModel, nameof(multiViewModel));
            MultiViewModel = multiViewModel;
            CanShowViewModel = canShowViewModel;
        }

        #endregion

        #region Properties

        protected IMultiViewModel MultiViewModel { get; }

        protected Func<IViewModel, IDataContext, IViewModelPresenter, bool> CanShowViewModel { get; }

        #endregion

        #region Methods

        protected virtual IAsyncOperation TryShowInternalAsync(IViewModel viewModel, IDataContext context, IViewModelPresenter parentPresenter)
        {
            if (MultiViewModel.ItemsSource.Any(vm => vm == viewModel))
                MultiViewModel.SelectedItem = viewModel;
            else
                MultiViewModel.AddViewModel(viewModel, true);
            return viewModel.RegisterNavigationOperation(OperationType.TabNavigation, context);
        }

        protected virtual Task<bool> TryCloseInternalAsync(IDataContext context, IViewModelPresenter parentPresenter)
        {
            return null;
        }

        #endregion

        #region Implementation of IDynamicViewModelPresenter

        public int Priority => ViewModelPresenter.DefaultMultiViewModelPresenterPriority;

        public IAsyncOperation TryShowAsync(IDataContext context, IViewModelPresenter parentPresenter)
        {
            if (MultiViewModel.IsDisposed)
            {
                Tracer.Warn($"Presenter cannot handle request {MultiViewModel} is disposed");
                parentPresenter.DynamicPresenters.Remove(this);
                return null;
            }
            var viewModel = context.GetData(NavigationConstants.ViewModel);
            if (viewModel == null || !MultiViewModel.ViewModelType.IsInstanceOfType(viewModel))
                return null;
            bool data;
            if (context.TryGetData(NavigationConstants.SuppressTabNavigation, out data) && data)
                return null;
            var canShow = CanShowViewModel ?? ApplicationSettings.MultiViewModelPresenterCanShowViewModel;
            if (canShow != null && !canShow(viewModel, context, parentPresenter))
                return null;

            return TryShowInternalAsync(viewModel, context, parentPresenter);
        }

        public Task<bool> TryCloseAsync(IDataContext context, IViewModelPresenter parentPresenter)
        {
            if (MultiViewModel.IsDisposed)
            {
                Tracer.Warn($"Presenter cannot handle request {MultiViewModel} is disposed");
                parentPresenter.DynamicPresenters.Remove(this);
                return null;
            }
            return TryCloseInternalAsync(context, parentPresenter);
        }

        #endregion
    }
}