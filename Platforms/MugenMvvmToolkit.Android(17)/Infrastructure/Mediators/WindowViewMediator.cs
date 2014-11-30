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
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Infrastructure.Mediators
{
    /// <summary>
    ///     Represents the mediator class for dialog view.
    /// </summary>
    public class WindowViewMediator : WindowViewMediatorBase<IWindowView>
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="WindowViewMediatorBase{TView}" /> class.
        /// </summary>
        public WindowViewMediator([NotNull] IViewModel viewModel, [NotNull] IThreadManager threadManager,
            [NotNull] IViewManager viewManager, [NotNull] IWrapperManager wrapperManager, [NotNull] IOperationCallbackManager callbackManager)
            : base(viewModel, threadManager, viewManager, wrapperManager, callbackManager)
        {
        }

        #endregion

        #region Overrides of WindowViewMediatorBase<IWindowView>

        /// <summary>
        ///     Shows the view in the specified mode.
        /// </summary>
        protected override void ShowView(IWindowView view, bool isDialog, IDataContext context)
        {
            var navigationProvider = ViewModel.GetIocContainer(true).Get<INavigationProvider>();
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
                Should.BeOfType<Activity>(navigationProvider.CurrentContent, "Activity");
                var activity = (Activity)navigationProvider.CurrentContent;
                fragmentManager = activity.GetFragmentManager();
            }
            view.Show(fragmentManager, Guid.NewGuid().ToString("n"));
        }

        /// <summary>
        ///     Closes the view.
        /// </summary>
        protected override void CloseView(IWindowView view)
        {
            view.Dismiss();
        }

        /// <summary>
        ///     Initializes the specified view.
        /// </summary>
        protected override void InitializeView(IWindowView windowView, IDataContext context)
        {
            windowView.Closing += OnViewClosing;
            windowView.Canceled += OnViewClosed;
            windowView.Destroyed += WindowViewOnDestroyed;
        }

        /// <summary>
        ///     Clears the event subscribtions from the specified view.
        /// </summary>
        /// <param name="windowView">The specified window-view to dispose.</param>
        protected override void CleanupView(IWindowView windowView)
        {
            windowView.Closing -= OnViewClosing;
            windowView.Canceled -= OnViewClosed;
            windowView.Destroyed -= WindowViewOnDestroyed;
        }

        #endregion

        #region Methods

        private void WindowViewOnDestroyed(IWindowView sender, EventArgs args)
        {
            sender.Destroyed -= WindowViewOnDestroyed;
            UpdateView(null, false, DataContext.Empty);
        }

        #endregion

    }
}