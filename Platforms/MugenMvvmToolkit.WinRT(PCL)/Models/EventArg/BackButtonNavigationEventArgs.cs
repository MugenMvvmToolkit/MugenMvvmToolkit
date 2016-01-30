#region Copyright

// ****************************************************************************
// <copyright file="BackButtonNavigationEventArgs.cs">
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

using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

#if WINDOWSCOMMON
namespace MugenMvvmToolkit.WinRT.Models.EventArg
#else
namespace MugenMvvmToolkit.WinPhone.Models.EventArg
#endif
{
    internal class BackButtonNavigationEventArgs : NavigationEventArgsBase
    {
        #region Fields

        public static readonly BackButtonNavigationEventArgs Instance;

        #endregion

        static BackButtonNavigationEventArgs()
        {
            Instance = new BackButtonNavigationEventArgs();
        }

        private BackButtonNavigationEventArgs()
        {
        }

        #region Properties

        public override object Content => null;

        public override NavigationMode Mode => NavigationMode.Back;

        #endregion
    }
}