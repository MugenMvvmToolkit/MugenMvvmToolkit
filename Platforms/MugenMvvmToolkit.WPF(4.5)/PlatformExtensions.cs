#region Copyright

// ****************************************************************************
// <copyright file="PlatformExtensions.cs">
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
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.WPF
{
    public static class PlatformExtensions
    {
        #region Methods

        internal static PlatformInfo GetPlatformInfo()
        {
            return new PlatformInfo(PlatformType.WPF, Environment.Version);
        }

        internal static NavigationMode ToNavigationMode(this System.Windows.Navigation.NavigationMode mode)
        {
            switch (mode)
            {
                case System.Windows.Navigation.NavigationMode.New:
                    return NavigationMode.New;
                case System.Windows.Navigation.NavigationMode.Back:
                    return NavigationMode.Back;
                case System.Windows.Navigation.NavigationMode.Forward:
                    return NavigationMode.Forward;
                case System.Windows.Navigation.NavigationMode.Refresh:
                    return NavigationMode.Refresh;
                default:
                    return NavigationMode.Undefined;
            }
        }

        #endregion
    }
}