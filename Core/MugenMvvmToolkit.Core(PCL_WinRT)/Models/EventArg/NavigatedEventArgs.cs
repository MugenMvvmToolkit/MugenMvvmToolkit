#region Copyright

// ****************************************************************************
// <copyright file="NavigatedEventArgs.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Navigation;

namespace MugenMvvmToolkit.Models.EventArg
{
    public class NavigatedEventArgs : EventArgs
    {
        #region Fields

        private readonly INavigationContext _context;

        #endregion

        #region Constructors

        public NavigatedEventArgs([NotNull]INavigationContext context)
        {
            Should.NotBeNull(context, "context");
            _context = context;
        }

        #endregion

        #region Properties

        [NotNull]
        public INavigationContext Context
        {
            get { return _context; }
        }

        #endregion
    }
}
