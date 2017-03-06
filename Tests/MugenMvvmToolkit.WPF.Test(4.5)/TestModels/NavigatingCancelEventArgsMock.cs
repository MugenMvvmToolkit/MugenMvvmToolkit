#region Copyright

// ****************************************************************************
// <copyright file="NavigatingCancelEventArgsMock.cs">
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

namespace MugenMvvmToolkit.Test.TestModels
{
    public class NavigatingCancelEventArgsMock : NavigatingCancelEventArgsBase
    {
        #region Constructors

        public NavigatingCancelEventArgsMock(NavigationMode mode, bool isCancelable, IDataContext context = null)
        {
            NavigationMode = mode;
            IsCancelable = isCancelable;
            Context = context;
        }

        #endregion

        #region Overrides of NavigatingCancelEventArgsBase

        public override bool Cancel { get; set; }

        public override NavigationMode NavigationMode { get; }

        public override bool IsCancelable { get; }

        public override IDataContext Context { get; }

        #endregion
    }
}