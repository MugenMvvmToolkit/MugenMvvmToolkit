#region Copyright

// ****************************************************************************
// <copyright file="NavigatedEventArgs.cs">
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

        public NavigatedEventArgs([NotNull]INavigationContext context, bool isCanceled, [CanBeNull] Exception exception)
        {
            Should.NotBeNull(context, nameof(context));
            _context = context;
            IsCanceled = isCanceled;
            Exception = exception;
        }

        #endregion

        #region Properties

        public bool IsCanceled { get; }

        [CanBeNull]
        public Exception Exception { get; }

        [NotNull]
        public INavigationContext Context => _context;

        #endregion
    }
}
