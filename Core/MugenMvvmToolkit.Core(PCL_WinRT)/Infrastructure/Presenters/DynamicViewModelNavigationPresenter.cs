#region Copyright

// ****************************************************************************
// <copyright file="DynamicViewModelNavigationPresenter.cs">
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
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Infrastructure.Presenters
{
    /// <summary>
    ///     Represents the service that allows to show a view model using <see cref="INavigationProvider" />.
    /// </summary>
    public sealed class DynamicViewModelNavigationPresenter : IRestorableDynamicViewModelPresenter
    {
        #region Fields

        private static Func<IViewModel, IDataContext, IViewModelPresenter, bool> _canShowViewModelDefault;
        private readonly Func<IViewModel, IDataContext, IViewModelPresenter, bool> _canShowViewModel;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="DynamicViewModelNavigationPresenter" /> class.
        /// </summary>
        public DynamicViewModelNavigationPresenter()
            : this(null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DynamicViewModelNavigationPresenter" /> class.
        /// </summary>
        public DynamicViewModelNavigationPresenter(
            Func<IViewModel, IDataContext, IViewModelPresenter, bool> canShowViewModel)
        {
            _canShowViewModel = canShowViewModel;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the delegate that determines that presenter can handle request.
        /// </summary>
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

        /// <summary>
        ///     Gets the presenter priority.
        /// </summary>
        public int Priority
        {
            get { return ViewModelPresenter.DefaultNavigationPresenterPriority; }
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
            if (!CanShowViewModel(viewModel, context, parentPresenter))
                return null;
            var operation = new AsyncOperation<bool?>();
            context = context.ToNonReadOnly();
            context.AddOrUpdate(NavigationConstants.ViewModel, viewModel);
            viewModel.GetIocContainer(true)
                .Get<INavigationProvider>()
                .Navigate(operation.ToOperationCallback(), context);
            return operation;
        }

        /// <summary>
        ///     Tries to restore the presenter state of the specified <see cref="IViewModel" />.
        /// </summary>
        /// <param name="viewModel">The specified <see cref="IViewModel" /> to show.</param>
        /// <param name="context">The specified context.</param>
        /// <param name="parentPresenter">The parent presenter, if any.</param>
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