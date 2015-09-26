#region Copyright

// ****************************************************************************
// <copyright file="DynamicViewModelNavigationPresenter.cs">
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

        private static Func<IViewModel, IDataContext, IViewModelPresenter, bool> _canShowViewModelDefault;
        private readonly Func<IViewModel, IDataContext, IViewModelPresenter, bool> _canShowViewModel;

        #endregion

        #region Constructors

        public DynamicViewModelNavigationPresenter()
            : this(null)
        {
        }

        public DynamicViewModelNavigationPresenter(
            Func<IViewModel, IDataContext, IViewModelPresenter, bool> canShowViewModel)
        {
            _canShowViewModel = canShowViewModel;
        }

        #endregion

        #region Properties

        [NotNull]
        public static Func<IViewModel, IDataContext, IViewModelPresenter, bool> CanShowViewModelDefault
        {
            get
            {
                if (_canShowViewModelDefault == null)
                    _canShowViewModelDefault = (model, context, arg3) => true;
                return _canShowViewModelDefault;
            }
            set { _canShowViewModelDefault = value; }
        }

        private bool CanShowViewModel(IViewModel viewModel, IDataContext context,
            IViewModelPresenter parentPresenter)
        {
            bool data;
            if (context.TryGetData(NavigationConstants.SuppressPageNavigation, out data) && data)
                return false;

            if (_canShowViewModel == null)
                return CanShowViewModelDefault(viewModel, context, parentPresenter);
            return _canShowViewModel(viewModel, context, parentPresenter);
        }

        #endregion

        #region Implementation of IDynamicViewModelPresenter

        public int Priority
        {
            get { return ViewModelPresenter.DefaultNavigationPresenterPriority; }
        }

        public INavigationOperation TryShowAsync(IViewModel viewModel, IDataContext context,
            IViewModelPresenter parentPresenter)
        {
            if (!CanShowViewModel(viewModel, context, parentPresenter))
                return null;
            var tcs = new TaskCompletionSource<object>();
            var operation = new NavigationOperation(tcs.Task);
            context = context.ToNonReadOnly();
            context.AddOrUpdate(NavigationConstants.ViewModel, viewModel);
            var provider = viewModel.GetIocContainer(true).Get<INavigationProvider>();
            provider.CurrentNavigationTask.TryExecuteSynchronously(_ =>
            {
                try
                {
                    var task = provider.NavigateAsync(operation.ToOperationCallback(), context);
                    tcs.TrySetFromTask(task);
                }
                catch (Exception e)
                {
                    tcs.TrySetException(e);
                    throw;
                }
            });
            return operation;
        }

        public bool Restore(IViewModel viewModel, IDataContext context, IViewModelPresenter parentPresenter)
        {
            if (!CanShowViewModel(viewModel, context, parentPresenter))
                return false;
            INavigationProvider provider;
            if (viewModel.GetIocContainer(true).TryGet(out provider))
            {
                provider.OnNavigated(viewModel, NavigationMode.Reset, context);
                return true;
            }
            return false;
        }

        #endregion
    }
}
