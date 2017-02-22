#region Copyright

// ****************************************************************************
// <copyright file="INavigationService.cs">
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

using System;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
#if WPF
using System.Windows.Navigation;

namespace MugenMvvmToolkit.WPF.Interfaces.Navigation
#elif ANDROID
using Android.App;

namespace MugenMvvmToolkit.Android.Interfaces.Navigation
#elif TOUCH
namespace MugenMvvmToolkit.iOS.Interfaces.Navigation
#elif XAMARIN_FORMS
using Xamarin.Forms;

namespace MugenMvvmToolkit.Xamarin.Forms.Interfaces.Navigation
#elif WINDOWS_UWP
namespace MugenMvvmToolkit.UWP.Interfaces.Navigation
#endif
{
    public interface INavigationService//todo remove can go back, remove back entry
    {
        object CurrentContent { get; }
#if ANDROID
        void OnPauseActivity([NotNull] Activity activity, IDataContext context = null);

        void OnResumeActivity([NotNull] Activity activity, IDataContext context = null);

        void OnStartActivity([NotNull] Activity activity, IDataContext context = null);

        void OnCreateActivity([NotNull] Activity activity, IDataContext context = null);

        bool OnFinishActivity([NotNull] Activity activity, bool isBackNavigation, IDataContext context = null);
#elif XAMARIN_FORMS
        void UpdateRootPage(NavigationPage page);
#endif
        [CanBeNull]
        string GetParameterFromArgs([NotNull]EventArgs args);

        bool Navigate([NotNull] NavigatingCancelEventArgsBase args);

        bool Navigate([NotNull] IViewMappingItem source, [CanBeNull] string parameter, [CanBeNull] IDataContext dataContext);
        //todo update
        bool CanClose([NotNull] IDataContext dataContext);

        bool TryClose([NotNull] IDataContext dataContext);

        event EventHandler<INavigationService, NavigatingCancelEventArgsBase> Navigating;

        event EventHandler<INavigationService, NavigationEventArgsBase> Navigated;
    }
}
