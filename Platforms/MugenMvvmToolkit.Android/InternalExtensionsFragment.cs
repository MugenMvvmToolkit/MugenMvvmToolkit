#region Copyright

// ****************************************************************************
// <copyright file="InternalExtensionsFragment.cs">
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

using Android.App;
using JetBrains.Annotations;

#if APPCOMPAT
using Android.Support.V4.App;
using FragmentManager = Android.Support.V4.App.FragmentManager;
namespace MugenMvvmToolkit.Android.AppCompat
#else
namespace MugenMvvmToolkit.Android
#endif
{
    // ReSharper disable once PartialTypeWithSinglePart
    internal static partial class InternalExtensions
    {
        #region Methods

        public static FragmentManager GetFragmentManager(this Activity activity)
        {
            Should.NotBeNull(activity, "activity");
#if APPCOMPAT
            Should.BeOfType<FragmentActivity>(activity, "activity");
            return ((FragmentActivity)activity).SupportFragmentManager;
#else
            return activity.FragmentManager;
#endif
        }

        [CanBeNull]
        public static FragmentManager TryGetFragmentManager(this Activity activity)
        {
            if (activity == null)
                return null;
#if APPCOMPAT
            var fragmentActivity = activity as FragmentActivity;
            if (fragmentActivity == null)
                return null;
            return fragmentActivity.SupportFragmentManager;
#else
            return activity.FragmentManager;
#endif
        }

        #endregion
    }
}