#region Copyright

// ****************************************************************************
// <copyright file="Enums.cs">
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
using Android.Support.V4.View;
using Android.Support.V7.App;

namespace MugenMvvmToolkit.Android.AppCompat.Models
{

    [Flags]
    internal enum ActionBarDisplayOptions
    {
        UseLogo = ActionBar.DisplayUseLogo,
        ShowHome = ActionBar.DisplayShowHome,
        HomeAsUp = ActionBar.DisplayHomeAsUp,
        ShowTitle = ActionBar.DisplayShowTitle,
        ShowCustom = ActionBar.DisplayShowCustom,
    }

    internal enum ActionBarNavigationMode
    {
        Standard,
        List,
        Tabs,
    }

    [Flags]
    internal enum ShowAsAction
    {
        Always = MenuItemCompat.ShowAsActionAlways,
        CollapseActionView = MenuItemCompat.ShowAsActionCollapseActionView,
        IfRoom = MenuItemCompat.ShowAsActionIfRoom,
        Never = MenuItemCompat.ShowAsActionNever,
        WithText = MenuItemCompat.ShowAsActionWithText,
    }
}
