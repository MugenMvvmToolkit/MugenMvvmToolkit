#region Copyright

// ****************************************************************************
// <copyright file="BackButtonNavigatingEventArgs.cs">
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

using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

#if WINDOWSCOMMON
namespace MugenMvvmToolkit.WinRT.Models.EventArg
#else
namespace MugenMvvmToolkit.WinPhone.Models.EventArg
#endif
{
    internal class BackButtonNavigatingEventArgs : NavigatingCancelEventArgsBase
    {
        #region Properties

        public override bool Cancel { get; set; }

        public override NavigationMode NavigationMode
        {
            get { return NavigationMode.Back; }
        }

        public override bool IsCancelable
        {
            get { return true; }
        }

        #endregion
    }
}