#region Copyright

// ****************************************************************************
// <copyright file="WindowViewMediator.cs">
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

using System.ComponentModel;
using System.Linq;
using JetBrains.Annotations;
using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure.Mediators;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Models;
#if WPF
using MugenMvvmToolkit.WPF.Interfaces.Views;

namespace MugenMvvmToolkit.WPF.Infrastructure.Mediators
#elif WINFORMS
using MugenMvvmToolkit.WinForms.Interfaces.Views;

namespace MugenMvvmToolkit.WinForms.Infrastructure.Mediators
#endif
{
    public class WindowViewMediator : WindowViewMediatorBase<IWindowView>
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public WindowViewMediator([NotNull] IThreadManager threadManager, [NotNull] IViewManager viewManager,
            [NotNull] IWrapperManager wrapperManager, [NotNull] INavigationDispatcher navigationDispatcher, [NotNull] IEventAggregator eventAggregator)
            : base(threadManager, viewManager, wrapperManager, navigationDispatcher, eventAggregator)
        {
        }

        #endregion

        #region Overrides of WindowViewMediatorBase<IWindowView>

        protected override void ShowView(IWindowView view, bool isDialog, IDataContext context)
        {
            var currentViewModel = context.GetData(NavigationConstants.ViewModel);
            var topViewModel = NavigationDispatcher.GetOpenedViewModels(NavigationType.Window).LastOrDefault(vm => vm != currentViewModel);
            if (topViewModel != null)
            {
                view.Owner = topViewModel.Settings.Metadata.GetData(ViewModelConstants.View);
            }

            try
            {
                if (isDialog)
                    view.ShowDialog();
                else
                    view.Show();
            }
            catch
            {
                if (isDialog)
                    view.Close();
                throw;
            }
        }

        protected override bool ActivateView(IWindowView view, IDataContext context)
        {
#if WPF
            return view.Activate();
#else
            view.Activate();
            return true;
#endif
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

        #endregion

        #region Methods

        private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            OnViewClosing(sender, cancelEventArgs);
        }

        #endregion
    }
}
