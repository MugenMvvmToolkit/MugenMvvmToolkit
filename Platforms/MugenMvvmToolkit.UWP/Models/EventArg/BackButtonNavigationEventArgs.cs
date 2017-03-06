#region Copyright

// ****************************************************************************
// <copyright file="BackButtonNavigationEventArgs.cs">
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

using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.UWP.Models.EventArg
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

        public override NavigationMode NavigationMode => NavigationMode.Back;

        public override IDataContext Context => null;

        #endregion
    }
}