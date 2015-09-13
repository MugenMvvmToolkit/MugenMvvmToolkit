#region Copyright

// ****************************************************************************
// <copyright file="INavigationService.cs">
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
#elif SILVERLIGHT
namespace MugenMvvmToolkit.Silverlight.Interfaces.Navigation
#elif WINDOWSCOMMON
namespace MugenMvvmToolkit.WinRT.Interfaces.Navigation
#elif WINDOWS_PHONE
using System.Windows.Navigation;

namespace MugenMvvmToolkit.WinPhone.Interfaces.Navigation
#endif
{
    /// <summary>
    ///     Implemented by services that provide view model based navigation.
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        ///     Indicates whether the navigator can navigate back.
        /// </summary>
        bool CanGoBack { get; }

        /// <summary>
        ///     Indicates whether the navigator can navigate forward.
        /// </summary>
        bool CanGoForward { get; }

        /// <summary>
        ///     The current content.
        /// </summary>
        object CurrentContent { get; }

        /// <summary>
        ///     Navigates back.
        /// </summary>
        void GoBack();

        /// <summary>
        ///     Navigates forward.
        /// </summary>
        void GoForward();

#if WPF || WINDOWS_PHONE
        /// <summary>
        ///     Removes the most recent entry from the back stack.
        /// </summary>
        /// <returns> The entry that was removed. </returns>
        JournalEntry RemoveBackEntry();
#elif ANDROID
        /// <summary>
        ///     Raised as part of the activity lifecycle when an activity is going into the background.
        /// </summary>
        void OnPauseActivity([NotNull] Activity activity, IDataContext context = null);

        /// <summary>
        ///     Called when activity will start interacting with the user.
        /// </summary>
        void OnResumeActivity([NotNull] Activity activity, IDataContext context = null);

        /// <summary>
        ///     Called when the activity had been stopped, but is now again being displayed to the user.
        /// </summary>
        void OnStartActivity([NotNull] Activity activity, IDataContext context = null);

        /// <summary>
        ///     Called when the activity is starting.
        /// </summary>
        void OnCreateActivity([NotNull] Activity activity, IDataContext context = null);

        /// <summary>
        ///     Call this when your activity is done and should be closed.
        /// </summary>
        bool OnFinishActivity([NotNull] Activity activity, bool isBackNavigation, IDataContext context = null);
#elif XAMARIN_FORMS
        /// <summary>
        ///     Updates the current root page.
        /// </summary>
        void UpdateRootPage(NavigationPage page);
#endif
        /// <summary>
        /// Gets a navigation parameter from event args.
        /// </summary>
        [CanBeNull]
        string GetParameterFromArgs([NotNull]EventArgs args);

        /// <summary>
        ///     Navigates using cancel event args.
        /// </summary>
        bool Navigate([NotNull] NavigatingCancelEventArgsBase args, [CanBeNull] IDataContext dataContext);

        /// <summary>
        ///     Displays the content located at the specified <see cref="IViewMappingItem" />.
        /// </summary>
        /// <param name="source">
        ///     The <c>IViewPageMappingItem</c> of the content to display.
        /// </param>
        /// <param name="parameter">
        ///     A <see cref="T:System.Object" /> that contains data to be used for processing during
        ///     navigation.
        /// </param>
        /// <param name="dataContext">
        ///     The specified <see cref="IDataContext" />.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the content was successfully displayed; otherwise, <c>false</c>.
        /// </returns>
        bool Navigate([NotNull] IViewMappingItem source, [CanBeNull] string parameter, [CanBeNull] IDataContext dataContext);

        /// <summary>
        ///     Determines whether the specified command <c>CloseCommand</c> can be execute.
        /// </summary>
        bool CanClose([NotNull] IViewModel viewModel, [CanBeNull] IDataContext dataContext);

        /// <summary>
        ///     Tries to close view-model page.
        /// </summary>
        bool TryClose([NotNull]IViewModel viewModel, [CanBeNull] IDataContext dataContext);

        /// <summary>
        ///     Raised prior to navigation.
        /// </summary>
        event EventHandler<INavigationService, NavigatingCancelEventArgsBase> Navigating;

        /// <summary>
        ///     Raised after navigation.
        /// </summary>
        event EventHandler<INavigationService, NavigationEventArgsBase> Navigated;
    }
}