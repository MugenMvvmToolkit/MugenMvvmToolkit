#region Copyright

// ****************************************************************************
// <copyright file="BackButtonNavigatingEventArgs.cs">
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

using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.UWP.Models.EventArg
{
    internal class BackButtonNavigatingEventArgs : NavigatingCancelEventArgsBase
    {
        #region Properties

        public override bool Cancel { get; set; }

        public override NavigationMode NavigationMode => NavigationMode.Back;

        public override bool IsCancelable => true;

        public override IDataContext Context => null;

        #endregion
    }
}