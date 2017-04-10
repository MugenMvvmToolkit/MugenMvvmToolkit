#region Copyright

// ****************************************************************************
// <copyright file="NavigationServiceMock.cs">
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

using System;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.WPF.Interfaces.Navigation;
#if NETFX_CORE || WINDOWS_UWP
using Windows.UI.Xaml.Navigation;
#elif ANDROID
using Android.App;
#else
using System.Windows.Navigation;
#endif
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.UWP.Interfaces.Navigation;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public class NavigationServiceMock : INavigationService
    {
        #region Properties

        public Func<NavigatingCancelEventArgsBase, bool> NavigateArgs { get; set; }

        public Func<IViewMappingItem, string, IDataContext, bool> Navigate { get; set; }

        public Func<IViewModel, IDataContext, bool> TryClose { get; set; }

        public Func<IViewModel, IDataContext, bool> CanClose { get; set; }

        #endregion

        #region Implementation of INavigationService

        public object CurrentContent { get; set; }

        bool INavigationService.Navigate(NavigatingCancelEventArgsBase args)
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

        bool INavigationService.CanClose(IDataContext dataContext)
        {
            return CanClose != null && CanClose(dataContext.GetData(NavigationConstants.ViewModel), dataContext);
        }

        bool INavigationService.TryClose(IDataContext dataContext)
        {
            if (TryClose == null)
                return false;
            return TryClose(dataContext.GetData(NavigationConstants.ViewModel), dataContext);
        }

        public event EventHandler<INavigationService, NavigatingCancelEventArgsBase> Navigating;

        public event EventHandler<INavigationService, NavigationEventArgsBase> Navigated;

        #endregion

        #region Methods

        public virtual void OnNavigated(NavigationEventArgsBase e)
        {
            Navigated?.Invoke(this, e);
        }

        public virtual void OnNavigating(NavigatingCancelEventArgsBase e)
        {
            Navigating?.Invoke(this, e);
        }

        #endregion
    }
}
