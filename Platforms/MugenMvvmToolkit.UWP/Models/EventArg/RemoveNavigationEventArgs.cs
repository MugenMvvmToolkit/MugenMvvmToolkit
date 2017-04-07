#region Copyright

// ****************************************************************************
// <copyright file="RemoveNavigationEventArgs.cs">
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
    public class RemoveNavigationEventArgs : NavigationEventArgsBase
    {
        #region Fields

        private readonly IDataContext _context;

        #endregion

        #region Constructors

        public RemoveNavigationEventArgs(IDataContext context)
        {
            _context = context;
        }

        #endregion

        #region Properties

        public override string Parameter => null;

        public override object Content => null;

        public override NavigationMode NavigationMode => NavigationMode.Remove;

        public override IDataContext Context => _context;

        #endregion
    }
}