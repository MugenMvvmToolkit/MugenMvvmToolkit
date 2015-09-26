#region Copyright

// ****************************************************************************
// <copyright file="WindowViewMediator.cs">
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
using Android.App;
using JetBrains.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure.Mediators;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;
#if APPCOMPAT
using MugenMvvmToolkit.Android.AppCompat.Interfaces.Views;
using Fragment = Android.Support.V4.App.Fragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;

namespace MugenMvvmToolkit.Android.AppCompat.Infrastructure.Mediators
#else
using MugenMvvmToolkit.Android.Interfaces.Views;

namespace MugenMvvmToolkit.Android.Infrastructure.Mediators
#endif
{
    public class WindowViewMediator : WindowViewMediatorBase<IWindowView>
    {
        #region Constructors

        public WindowViewMediator([NotNull] IViewModel viewModel, [NotNull] IThreadManager threadManager,
            [NotNull] IViewManager viewManager, [NotNull] IWrapperManager wrapperManager, [NotNull] IOperationCallbackManager callbackManager)
            : base(viewModel, threadManager, viewManager, wrapperManager, callbackManager)
        {
        }

        #endregion

        #region Overrides of WindowViewMediatorBase<IWindowView>

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
                Should.BeOfType<Activity>(navigationProvider.CurrentContent, "CurrentContent");
                var activity = (Activity)navigationProvider.CurrentContent;
                fragmentManager = activity.GetFragmentManager();
            }
            view.Show(fragmentManager, Guid.NewGuid().ToString("n"));
            fragmentManager.ExecutePendingTransactions();
        }

        protected override void CloseView(IWindowView view)
        {
            view.Dismiss();
        }

        protected override void InitializeView(IWindowView windowView, IDataContext context)
        {
            windowView.Mediator.Closing += OnViewClosing;
            windowView.Mediator.Canceled += OnViewClosed;
            windowView.Mediator.Destroyed += WindowViewOnDestroyed;
        }

        protected override void CleanupView(IWindowView windowView)
        {
            windowView.Mediator.Closing -= OnViewClosing;
            windowView.Mediator.Canceled -= OnViewClosed;
            windowView.Mediator.Destroyed -= WindowViewOnDestroyed;
        }

        #endregion

        #region Methods

        private void WindowViewOnDestroyed(Fragment sender, EventArgs args)
        {
            ((IWindowView)sender).Mediator.Destroyed -= WindowViewOnDestroyed;
            UpdateView(null, false, DataContext.Empty);
        }

        #endregion
    }
}
