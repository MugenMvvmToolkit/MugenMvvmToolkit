#region Copyright

// ****************************************************************************
// <copyright file="INavigationService.cs">
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

using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
#if ANDROID
using Android.App;

namespace MugenMvvmToolkit.Android.Interfaces.Navigation
#elif TOUCH
namespace MugenMvvmToolkit.iOS.Interfaces.Navigation
#elif XAMARIN_FORMS
using MugenMvvmToolkit.Interfaces.ViewModels;
using Xamarin.Forms;

namespace MugenMvvmToolkit.Xamarin.Forms.Interfaces.Navigation
#elif WINDOWS_UWP
namespace MugenMvvmToolkit.UWP.Interfaces.Navigation
#endif
{
    public interface INavigationService
    {
        object CurrentContent { get; }

        bool Navigate([NotNull] NavigatingCancelEventArgsBase args);

        bool Navigate([NotNull] IViewMappingItem source, [CanBeNull] string parameter, [NotNull] IDataContext dataContext);

        bool CanClose([NotNull] IDataContext dataContext);

        bool TryClose([NotNull] IDataContext dataContext);

#if ANDROID
        void OnPauseActivity([NotNull] Activity activity, IDataContext context);

        void OnResumeActivity([NotNull] Activity activity, IDataContext context);

        void OnStartActivity([NotNull] Activity activity, IDataContext context);

        void OnCreateActivity([NotNull] Activity activity, IDataContext context);

        bool OnFinishActivity([NotNull] Activity activity, bool isBackNavigation, IDataContext context);

        void OnDestroyActivity([NotNull] Activity activity, IDataContext context);
#elif XAMARIN_FORMS
        void UpdateRootPage(NavigationPage page, IViewModel rootPageViewModel);

        event EventHandler<INavigationService, ValueEventArgs<IViewModel>> RootPageChanged;
#endif

        event EventHandler<INavigationService, NavigatingCancelEventArgsBase> Navigating;

        event EventHandler<INavigationService, NavigationEventArgsBase> Navigated;
    }
}
