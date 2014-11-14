#region Copyright
// ****************************************************************************
// <copyright file="INavigationService.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
#if NETFX_CORE || WINDOWSCOMMON
using Windows.UI.Xaml.Navigation;
#elif ANDROID
using Android.App;
#elif TOUCH || XAMARIN_FORMS
#else
using System.Windows.Navigation;
#endif
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Interfaces.Navigation
{
    /// <summary>
    ///     Implemented by services that provide <see cref="IViewMappingItem" /> based navigation.
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
#endif
        /// <summary>
        /// Gets a navigation parameter from event args.
        /// </summary>
        [CanBeNull]
        object GetParameterFromArgs([NotNull]EventArgs args);

        /// <summary>
        ///     Navigates using cancel event args.
        /// </summary>
        bool Navigate([NotNull] NavigatingCancelEventArgsBase args);

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
        bool Navigate([NotNull] IViewMappingItem source, [CanBeNull] object parameter, [CanBeNull] IDataContext dataContext);

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