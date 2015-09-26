using System;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Silverlight.Interfaces.Navigation;
using MugenMvvmToolkit.Silverlight.Models.EventArg;
using MugenMvvmToolkit.WinRT.Interfaces.Navigation;
using MugenMvvmToolkit.WinRT.Models.EventArg;
using MugenMvvmToolkit.WPF.Interfaces.Navigation;
using MugenMvvmToolkit.WPF.Models.EventArg;
#if NETFX_CORE || WINDOWSCOMMON
using Windows.UI.Xaml.Navigation;
#elif ANDROID
using Android.App;
#else
using System.Windows.Navigation;
#endif
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public class NavigationServiceMock : INavigationService
    {
        #region Properties

        public Action GoBack { get; set; }

        public Func<EventArgs, string> GetParameterFromArgs { get; set; }

        public Func<NavigatingCancelEventArgsBase, bool> NavigateArgs { get; set; }

        public Func<IViewMappingItem, string, IDataContext, bool> Navigate { get; set; }

        public Func<IViewModel, IDataContext, bool> TryClose { get; set; }
        public Func<IViewModel, IDataContext, bool> CanClose { get; set; }

        #endregion

        #region Implementation of INavigationService

        public bool CanGoBack { get; set; }

        public bool CanGoForward { get; set; }

        public object CurrentContent { get; set; }

        void INavigationService.GoBack()
        {
            if (GoBack != null)
                GoBack();
        }

        void INavigationService.GoForward()
        {
            throw new NotSupportedException();
        }

#if WPF
        JournalEntry INavigationService.RemoveBackEntry()
        {
            throw new NotSupportedException();
        }
#endif

        string INavigationService.GetParameterFromArgs(EventArgs args)
        {
            if (GetParameterFromArgs == null)
                return null;
            return GetParameterFromArgs(args);
        }

        bool INavigationService.Navigate(NavigatingCancelEventArgsBase args, IDataContext dataContext)
        {
            if (NavigateArgs == null)
                return false;
            return NavigateArgs(args);
        }

        bool INavigationService.Navigate(IViewMappingItem source, string parameter, IDataContext dataContext)
        {
            if (Navigate == null)
                return false;
            return Navigate(source, parameter, dataContext);
        }

        bool INavigationService.CanClose(IViewModel viewModel, IDataContext dataContext)
        {
            return CanClose != null && CanClose(viewModel, dataContext);
        }

        bool INavigationService.TryClose(IViewModel viewModel, IDataContext dataContext)
        {
            if (TryClose == null)
                return false;
            return TryClose(viewModel, dataContext);
        }

        public event EventHandler<INavigationService, NavigatingCancelEventArgsBase> Navigating;

        public event EventHandler<INavigationService, NavigationEventArgsBase> Navigated;

        #endregion

        #region Methods

        public virtual void OnNavigated(NavigationEventArgsBase e)
        {
            EventHandler<INavigationService, NavigationEventArgsBase> handler = Navigated;
            if (handler != null) handler(this, e);
        }

        public virtual void OnNavigating(NavigatingCancelEventArgsBase e)
        {
            EventHandler<INavigationService, NavigatingCancelEventArgsBase> handler = Navigating;
            if (handler != null) handler(this, e);
        }

        #endregion
    }
}
