﻿#region Copyright

// ****************************************************************************
// <copyright file="ActionBarExtensions.cs">
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
using Android.Views;
using JetBrains.Annotations;

#if APPCOMPAT
using Fragment = Android.Support.V4.App.Fragment;
using FragmentTransaction = Android.Support.V4.App.FragmentTransaction;

namespace MugenMvvmToolkit.Android.AppCompat
#else
using Android.App;

namespace MugenMvvmToolkit.Android
#endif
{
    public static class ActionBarExtensions
    {
        #region Methods

        internal static void SetContentView([NotNull] this ViewGroup frameLayout, [NotNull] object content,
                    [NotNull] FragmentTransaction transaction,
                    [NotNull] Action<ViewGroup, Fragment, FragmentTransaction> updateAction)
        {
            Should.NotBeNull(frameLayout, nameof(frameLayout));
            var view = content as View;
            if (view == null)
            {
                var fragment = (Fragment)content;
                AndroidToolkitExtensions.ValidateViewIdFragment(frameLayout, fragment);
                updateAction(frameLayout, fragment, transaction);
            }
            else
            {
                if (frameLayout.ChildCount == 1 && frameLayout.GetChildAt(0) == view)
                    return;
                frameLayout.RemoveAllViews();
                frameLayout.AddView(view);
            }
        }

        #endregion
    }
}
