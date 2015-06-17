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

using System.ComponentModel;
using JetBrains.Annotations;
using MugenMvvmToolkit.Infrastructure.Mediators;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
#if WPF
using System.Windows.Navigation;
using MugenMvvmToolkit.WPF.Interfaces.Views;

namespace MugenMvvmToolkit.WPF.Infrastructure.Mediators
#elif WINFORMS
using MugenMvvmToolkit.WinForms.Interfaces.Views;

namespace MugenMvvmToolkit.WinForms.Infrastructure.Mediators
#endif
{
    /// <summary>
    ///     Represents the mediator class for dialog view.
    /// </summary>
    public class WindowViewMediator : WindowViewMediatorBase<IWindowView>
    {
        #region Fields

#if WPF
        private readonly NavigationWindow _window;
#endif
        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="NotifyPropertyChangedBase" /> class.
        /// </summary>
        public WindowViewMediator([NotNull] IViewModel viewModel, [NotNull] IThreadManager threadManager,
            [NotNull] IViewManager viewManager, [NotNull] IWrapperManager wrapperManager, [NotNull] IOperationCallbackManager callbackManager)
            : base(viewModel, threadManager, viewManager, wrapperManager, callbackManager)
        {
        }

#if WPF
        /// <summary>
        ///     Initializes a new instance of the <see cref="NotifyPropertyChangedBase" /> class.
        /// </summary>
        internal WindowViewMediator([NotNull] NavigationWindow window, [NotNull] IViewModel viewModel, [NotNull] IThreadManager threadManager,
             [NotNull] IViewManager viewManager, [NotNull] IWrapperManager wrapperManager, [NotNull] IOperationCallbackManager callbackManager)
            : base(viewModel, threadManager, viewManager, wrapperManager, callbackManager)
        {
            Should.NotBeNull(window, "window");
            _window = window;
        }
#endif

        #endregion

        #region Overrides of WindowViewMediatorBase<IWindowView>

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
            windowView.Closing += OnClosing;
        }

        /// <summary>
        ///     Clears the specified dialog view.
        /// </summary>
        /// <param name="windowView">The specified window-view to dispose.</param>
        protected override void CleanupView(IWindowView windowView)
        {
            windowView.Closing -= OnClosing;
        }

#if WPF
        /// <summary>
        ///     Gets the underlying view model.
        /// </summary>
        public override IViewModel ViewModel
        {
            get
            {
                if (_window == null)
                    return base.ViewModel;
                if (ThreadManager.IsUiThread)
                    return MugenMvvmToolkit.Infrastructure.ViewManager.GetDataContext(_window.Content) as IViewModel ?? base.ViewModel;
                return base.ViewModel;
            }
        }
#endif
        #endregion

        #region Methods

        private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            OnViewClosing(sender, cancelEventArgs);
        }

        #endregion
    }
}