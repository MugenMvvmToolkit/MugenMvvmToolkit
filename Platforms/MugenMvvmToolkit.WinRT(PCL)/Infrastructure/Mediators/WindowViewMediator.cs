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
    /// <summary>
    ///     Represents the mediator class for child window view.
    /// </summary>
    public class WindowViewMediator : WindowViewMediatorBase<IWindowView>
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="WindowViewMediator" /> class.
        /// </summary>
        public WindowViewMediator([NotNull] IViewModel viewModel, [NotNull] IThreadManager threadManager,
            [NotNull] IViewManager viewManager, [NotNull] IWrapperManager wrapperManager, [NotNull] IOperationCallbackManager callbackManager)
            : base(viewModel, threadManager, viewManager, wrapperManager, callbackManager)
        {
        }

        #endregion

        #region Overrides of WindowViewMediatorBase<IChildWindowView>

        /// <summary>
        ///     Shows the view in the specified mode.
        /// </summary>
        protected override void ShowView(IWindowView view, bool isDialog, IDataContext context)
        {
            if (isDialog)
                view.ShowDialog();
            else
                view.Show();
        }

        /// <summary>
        ///     Closes the view.
        /// </summary>
        protected override void CloseView(IWindowView view)
        {
            view.Close();
        }

        /// <summary>
        ///     Initializes the specified dialog view.
        /// </summary>
        protected override void InitializeView(IWindowView windowView, IDataContext context)
        {
            windowView.Closing += OnViewClosing;
            windowView.Closed += OnViewClosed;
        }

        /// <summary>
        ///     Clears the specified dialog view.
        /// </summary>
        /// <param name="windowView">The specified window-view to dispose.</param>
        protected override void CleanupView(IWindowView windowView)
        {
            windowView.Closing -= OnViewClosing;
            windowView.Closed -= OnViewClosed;
        }

        #endregion
    }
}