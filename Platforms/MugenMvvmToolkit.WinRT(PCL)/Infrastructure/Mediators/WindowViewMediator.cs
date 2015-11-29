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

using JetBrains.Annotations;
using MugenMvvmToolkit.Infrastructure.Mediators;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.WinRT.Interfaces.Views;

namespace MugenMvvmToolkit.WinRT.Infrastructure.Mediators
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

        #region Overrides of WindowViewMediatorBase<IChildWindowView>

        protected override void ShowView(IWindowView view, bool isDialog, IDataContext context)
        {
            if (isDialog)
                view.ShowDialog();
            else
                view.Show();
        }

        protected override void ActivateView(IWindowView view, IDataContext context)
        {
            view.Activate();
        }

        protected override void CloseView(IWindowView view)
        {
            view.Close();
        }

        protected override void InitializeView(IWindowView windowView, IDataContext context)
        {
            windowView.Closing += OnViewClosing;
            windowView.Closed += OnViewClosed;
        }

        protected override void CleanupView(IWindowView windowView)
        {
            windowView.Closing -= OnViewClosing;
            windowView.Closed -= OnViewClosed;
        }

        #endregion
    }
}
