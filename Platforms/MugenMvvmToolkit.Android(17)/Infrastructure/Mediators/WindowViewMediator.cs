#region Copyright
// ****************************************************************************
// <copyright file="WindowViewMediator.cs">
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
using Android.App;
using Android.Support.V4.App;
using JetBrains.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Utils;

namespace MugenMvvmToolkit.Infrastructure.Mediators
{
    /// <summary>
    ///     Represents the mediator class for dialog view.
    /// </summary>
    public class WindowViewMediator : WindowViewMediatorBase<IWindowView>
    {
        #region Fields

        private readonly INavigationProvider _navigationProvider;
        private string _id;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="WindowViewMediatorBase{TView}" /> class.
        /// </summary>
        public WindowViewMediator([NotNull] IViewModel viewModel, [NotNull] IThreadManager threadManager,
            [NotNull] IViewManager viewManager, [NotNull] IOperationCallbackManager callbackManager)
            : base(viewModel, threadManager, viewManager, callbackManager)
        {
            _navigationProvider = MvvmUtils.GetIocContainer(viewModel, true).Get<INavigationProvider>();
        }

        #endregion

        #region Overrides of WindowViewMediatorBase<IWindowView>

        /// <summary>
        ///     Shows the view in the specified mode.
        /// </summary>
        protected override void ShowView(IWindowView view, bool isDialog, IDataContext context)
        {
            view.Cancelable = !isDialog;
            FragmentManager fragmentManager = null;
            var parentViewModel = ViewModel.GetParentViewModel();
            if (parentViewModel != null)
            {
                var fragment = parentViewModel.Settings.Metadata.GetData(ViewModelConstants.View) as Fragment;
                if (fragment != null)
                    fragmentManager = fragment.ChildFragmentManager;
            }
            if (fragmentManager == null)
            {
                Should.BeOfType<Activity>(_navigationProvider.CurrentContent, "Activity");
                var activity = (Activity)_navigationProvider.CurrentContent;
                fragmentManager = activity.GetFragmentManager();
            }
            _id = Guid.NewGuid().ToString("n");
            view.Show(fragmentManager, _id);
        }

        /// <summary>
        ///     Closes the view.
        /// </summary>
        protected override void CloseView(IWindowView view)
        {
            view.Dismiss();
        }

        /// <summary>
        ///     Initializes the specified dialog view.
        /// </summary>
        /// <param name="windowView">The specified window-view to initialize.</param>
        protected override void InitializeView(IWindowView windowView, IDataContext context)
        {
            windowView.Closing += OnViewClosing;
            windowView.Canceled += OnViewClosed;
        }

        /// <summary>
        ///     Clears the event subscribtions from the specified view.
        /// </summary>
        /// <param name="windowView">The specified window-view to dispose.</param>
        protected override void CleanupView(IWindowView windowView)
        {
            windowView.Closing -= OnViewClosing;
            windowView.Canceled -= OnViewClosed;
        }

        #endregion
    }
}