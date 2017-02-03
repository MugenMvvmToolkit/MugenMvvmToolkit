#region Copyright

// ****************************************************************************
// <copyright file="WindowViewMediator.cs">
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

using System.ComponentModel;
using JetBrains.Annotations;
using MugenMvvmToolkit.Infrastructure.Mediators;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.ViewModels;
#if WPF
using System.Windows.Navigation;
using MugenMvvmToolkit.WPF.Interfaces.Views;

namespace MugenMvvmToolkit.WPF.Infrastructure.Mediators
#elif WINFORMS
using MugenMvvmToolkit.WinForms.Interfaces.Views;

namespace MugenMvvmToolkit.WinForms.Infrastructure.Mediators
#endif
{
    public class WindowViewMediator : WindowViewMediatorBase<IWindowView>
    {
        #region Fields

#if WPF
        private readonly NavigationWindow _window;
#endif
        #endregion

        #region Constructors

        public WindowViewMediator([NotNull] IViewModel viewModel, [NotNull] IThreadManager threadManager,
            [NotNull] IViewManager viewManager, [NotNull] IWrapperManager wrapperManager,
            [NotNull] IOperationCallbackManager callbackManager, [NotNull] INavigationDispatcher navigationDispatcher)
            : base(viewModel, threadManager, viewManager, wrapperManager, callbackManager, navigationDispatcher)
        {
        }

#if WPF
        internal WindowViewMediator([NotNull] NavigationWindow window, [NotNull] IViewModel viewModel, [NotNull] IThreadManager threadManager,
             [NotNull] IViewManager viewManager, [NotNull] IWrapperManager wrapperManager,
             [NotNull] IOperationCallbackManager callbackManager, [NotNull] INavigationDispatcher navigationDispatcher)
            : base(viewModel, threadManager, viewManager, wrapperManager, callbackManager, navigationDispatcher)
        {
            Should.NotBeNull(window, nameof(window));
            _window = window;
        }
#endif

        #endregion

        #region Overrides of WindowViewMediatorBase<IWindowView>

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
            windowView.Closing += OnClosing;
        }

        protected override void CleanupView(IWindowView windowView)
        {
            windowView.Closing -= OnClosing;
        }

#if WPF
        public override IViewModel ViewModel
        {
            get
            {
                if (_window == null)
                    return base.ViewModel;
                if (ThreadManager.IsUiThread)
                    return ToolkitExtensions.GetDataContext(_window.Content) as IViewModel ?? base.ViewModel;
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
