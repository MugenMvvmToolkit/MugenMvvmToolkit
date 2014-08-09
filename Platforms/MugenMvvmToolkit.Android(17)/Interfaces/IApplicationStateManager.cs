#region Copyright
// ****************************************************************************
// <copyright file="IApplicationStateManager.cs">
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
using Android.App;
using Android.OS;
using Android.Support.V4.App;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Interfaces
{
    public interface IApplicationStateManager
    {
        /// <summary>
        ///     Raised as part of the activity lifecycle when an activity is going into the background.
        /// </summary>
        void OnSaveInstanceStateActivity(Activity activity, Bundle bundle, IDataContext context = null);

        /// <summary>
        ///     Called when the activity had been stopped, but is now again being displayed to the user.
        /// </summary>
        void OnCreateActivity(Activity activity, Bundle bundle, IDataContext context = null);

#if !API8
        /// <summary>
        ///     Raised as part of the activity lifecycle when an activity is going into the background.
        /// </summary>
        void OnSaveInstanceStateFragment(Fragment fragment, Bundle bundle, IDataContext context = null);

        /// <summary>
        ///     Called when the activity had been stopped, but is now again being displayed to the user.
        /// </summary>
        void OnCreateFragment(Fragment fragment, Bundle bundle, IDataContext context = null);
#endif
    }
}