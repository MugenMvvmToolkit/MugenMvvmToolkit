using System;
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

        public Func<EventArgs, object> GetParameterFromArgs { get; set; }

        public Func<NavigatingCancelEventArgsBase, bool> NavigateArgs { get; set; }

        public Func<IViewMappingItem, object, IDataContext, bool> Navigate { get; set; }

        #endregion

        #region Implementation of INavigationService

        /// <summary>
        ///     Indicates whether the navigator can navigate back.
        /// </summary>
        public bool CanGoBack { get; set; }

        /// <summary>
        ///     Indicates whether the navigator can navigate forward.
        /// </summary>
        public bool CanGoForward { get; set; }

        /// <summary>
        ///     The current content.
        /// </summary>
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

        object INavigationService.GetParameterFromArgs(EventArgs args)
        {
            if (GetParameterFromArgs == null)
                return null;
            return GetParameterFromArgs(args);
        }

        bool INavigationService.Navigate(NavigatingCancelEventArgsBase args)
        {
            if (NavigateArgs == null)
                return false;
            return NavigateArgs(args);
        }

        bool INavigationService.Navigate(IViewMappingItem source, object parameter, IDataContext dataContext)
        {
            if (Navigate == null)
                return false;
            return Navigate(source, parameter, dataContext);
        }

        public void OnNavigated(NavigationEventArgs args)
        {
            throw new NotSupportedException();
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